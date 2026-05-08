using System.Reflection;
using Volo.Abp.DependencyInjection;
using Yi.Framework.ActionMetadata.Core.Metadata;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Permissions;

namespace Yi.Framework.Authorization.Core.Permissions
{
    /// <summary>
    /// 从后端代码契约中枚举权限定义。
    /// </summary>
    public class DefaultPermissionDefinitionProvider : IPermissionDefinitionProvider, ISingletonDependency
    {
        private readonly Lazy<IReadOnlyList<PermissionResourceDefinition>> _resources;

        public DefaultPermissionDefinitionProvider()
        {
            _resources = new Lazy<IReadOnlyList<PermissionResourceDefinition>>(BuildResources);
        }

        public IReadOnlyList<PermissionResourceDefinition> GetResources()
        {
            return _resources.Value;
        }

        public IReadOnlySet<string> GetPermissionCodes()
        {
            return _resources.Value
                .SelectMany(resource => resource.Permissions)
                .Select(permission => permission.Code)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
        }

        private static IReadOnlyList<PermissionResourceDefinition> BuildResources()
        {
            var resourceMap = new Dictionary<string, PermissionResourceDefinition>(StringComparer.OrdinalIgnoreCase);

            foreach (var type in GetCandidateTypes())
            {
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    AddExplicitPermission(resourceMap, type, method);
                }

                var resourceAttribute = ResolvePermissionResource(type);
                if (resourceAttribute == null)
                {
                    continue;
                }

                var resource = GetOrAddResource(resourceMap, resourceAttribute);
                foreach (var method in type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    AddActionPermission(resource, type, method);
                }
            }

            return resourceMap.Values
                .OrderBy(resource => resource.Module, StringComparer.OrdinalIgnoreCase)
                .ThenBy(resource => resource.Resource, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private static PermissionResourceDefinition GetOrAddResource(
            Dictionary<string, PermissionResourceDefinition> resourceMap,
            PermissionResourceAttribute resourceAttribute)
        {
            var key = $"{resourceAttribute.Module}:{resourceAttribute.Resource}";
            if (resourceMap.TryGetValue(key, out var resource))
            {
                return resource;
            }

            resource = new PermissionResourceDefinition(resourceAttribute.Module, resourceAttribute.Resource);
            resourceMap.Add(key, resource);
            return resource;
        }

        private static PermissionResourceAttribute? ResolvePermissionResource(Type type)
        {
            var resourceAttribute = type.GetCustomAttribute<PermissionResourceAttribute>(inherit: true);
            if (resourceAttribute != null)
            {
                return resourceAttribute;
            }

            foreach (var interfaceType in type.GetInterfaces())
            {
                resourceAttribute = interfaceType.GetCustomAttribute<PermissionResourceAttribute>(inherit: true);
                if (resourceAttribute != null)
                {
                    return resourceAttribute;
                }
            }

            return null;
        }

        private static void AddExplicitPermission(
            Dictionary<string, PermissionResourceDefinition> resourceMap,
            Type serviceType,
            MethodInfo method)
        {
            var permissionAttribute = ActionReflectionHelper.GetMethodAttribute<PermissionAttribute>(method, serviceType);
            if (permissionAttribute == null || string.IsNullOrWhiteSpace(permissionAttribute.Code))
            {
                return;
            }

            if (!TryParsePermissionCode(permissionAttribute.Code, out var module, out var permissionResource, out var action))
            {
                return;
            }

            var resource = GetOrAddResource(resourceMap, new PermissionResourceAttribute(module, permissionResource));
            AddPermission(resource, new PermissionDefinition(permissionAttribute.Code, module, permissionResource, action));
        }

        private static void AddActionPermission(PermissionResourceDefinition resource, Type serviceType, MethodInfo method)
        {
            var actionAttribute = ActionReflectionHelper.GetMethodAttribute<PermissionActionAttribute>(method, serviceType);
            if (actionAttribute == null || string.IsNullOrWhiteSpace(actionAttribute.Action))
            {
                return;
            }

            var code = $"{resource.Module}:{resource.Resource}:{actionAttribute.Action}";
            AddPermission(resource, new PermissionDefinition(code, resource.Module, resource.Resource, actionAttribute.Action));
        }

        private static void AddPermission(PermissionResourceDefinition resource, PermissionDefinition permission)
        {
            if (resource.Permissions.Any(existing => string.Equals(existing.Code, permission.Code, StringComparison.OrdinalIgnoreCase)))
            {
                return;
            }

            resource.Permissions.Add(permission);
        }

        private static bool TryParsePermissionCode(string code, out string module, out string resource, out string action)
        {
            module = string.Empty;
            resource = string.Empty;
            action = string.Empty;

            var parts = code.Split(':', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            if (parts.Length != 3)
            {
                return false;
            }

            module = parts[0];
            resource = parts[1];
            action = parts[2];
            return true;
        }

        private static IEnumerable<Type> GetCandidateTypes()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies().Where(IsCandidateAssembly))
            {
                foreach (var type in GetTypesSafely(assembly))
                {
                    if (type is { IsClass: true, IsAbstract: false })
                    {
                        yield return type;
                    }
                }
            }
        }

        private static bool IsCandidateAssembly(Assembly assembly)
        {
            var assemblyName = assembly.GetName().Name;
            return assemblyName != null &&
                   (assemblyName.StartsWith("Yi.Module.", StringComparison.OrdinalIgnoreCase) ||
                    assemblyName.StartsWith("Yi.Abp.", StringComparison.OrdinalIgnoreCase));
        }

        private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null)!;
            }
        }
    }
}
