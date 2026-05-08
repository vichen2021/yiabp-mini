using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Yi.Framework.Operation.Abstractions.Attributes;
using Yi.Framework.Operation.Abstractions.Metadata;

namespace Yi.Framework.Operation.Core.Metadata
{
    /// <summary>
    /// 默认 Action 身份解析器
    /// 只解析稳定事实（服务类型、方法、模块、资源），不做权限或日志决策
    /// </summary>
    public class DefaultActionIdentityResolver : IActionIdentityResolver
    {
        private readonly ActionIdentityCache _cache;

        public DefaultActionIdentityResolver(ActionIdentityCache cache)
        {
            _cache = cache;
        }

        public ActionIdentity Resolve(ControllerActionDescriptor descriptor)
        {
            var cacheKey = BuildCacheKey("MVC", descriptor.ControllerTypeInfo.AsType(), descriptor.MethodInfo);
            if (_cache.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            var identity = ParseIdentity(descriptor.ControllerTypeInfo.AsType(), descriptor.MethodInfo);
            _cache.Set(cacheKey, identity);
            return identity;
        }

        public ActionIdentity Resolve(Type serviceType, MethodInfo methodInfo)
        {
            var cacheKey = BuildCacheKey("Service", serviceType, methodInfo);
            if (_cache.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            var identity = ParseIdentity(serviceType, methodInfo);
            _cache.Set(cacheKey, identity);
            return identity;
        }

        private static string BuildCacheKey(string prefix, Type serviceType, MethodInfo methodInfo)
        {
            var parameterTypes = string.Join(",", methodInfo.GetParameters()
                .Select(parameter => parameter.ParameterType.FullName ?? parameter.ParameterType.Name));
            return $"{prefix}:{serviceType.FullName}:{methodInfo.Name}({parameterTypes})";
        }

        private ActionIdentity ParseIdentity(Type serviceType, MethodInfo methodInfo)
        {
            var identity = new ActionIdentity
            {
                ServiceType = serviceType,
                MethodInfo = methodInfo
            };

            // 1. 解析类级 PermissionResource
            var resourceAttr = serviceType.GetCustomAttribute<PermissionResourceAttribute>();
            if (resourceAttr != null)
            {
                identity.Module = resourceAttr.Module;
                identity.Resource = resourceAttr.Resource;
                identity.HasExplicitResource = true;
            }

            // 2. 解析方法级 PermissionAction（三层查找：当前方法 → 基类方法 → 接口方法）
            var actionAttr = ResolvePermissionAction(methodInfo, serviceType);
            if (actionAttr != null)
            {
                identity.CrudAction = actionAttr.Action;
                identity.HasExplicitAction = true;
            }

            // 3. 推断 Resource（仅当无显式声明时）
            if (!identity.HasExplicitResource)
            {
                identity.Resource = InferResourceName(serviceType);
            }

            // 4. 推断 Module（仅当无显式声明时）
            if (!identity.HasExplicitResource)
            {
                identity.Module = InferModuleName(serviceType);
            }

            // 5. 解析 RemoteServiceName（从 ABP 配置）
            identity.RemoteServiceName = GetRemoteServiceName(serviceType);

            return identity;
        }

        /// <summary>
        /// 解析 PermissionAction 特性（三层查找）
        /// 1. 优先读取当前方法上的 [PermissionAction]
        /// 2. 当前方法没有时，读取 GetBaseDefinition() 对应基类方法上的 [PermissionAction]
        /// 3. 如果方法来自接口代理，还需要查找接口方法上的 [PermissionAction]
        /// </summary>
        private PermissionActionAttribute? ResolvePermissionAction(MethodInfo methodInfo, Type serviceType)
        {
            // 1. 当前方法上的特性
            var actionAttr = methodInfo.GetCustomAttribute<PermissionActionAttribute>();
            if (actionAttr != null)
            {
                return actionAttr;
            }

            // 2. 基类方法上的特性（GetBaseDefinition）
            var baseMethod = methodInfo.GetBaseDefinition();
            if (baseMethod != null && baseMethod != methodInfo)
            {
                actionAttr = baseMethod.GetCustomAttribute<PermissionActionAttribute>();
                if (actionAttr != null)
                {
                    return actionAttr;
                }
            }

            // 3. 基类中按签名查找（泛型基类方法场景）
            foreach (var candidate in FindBaseMethods(serviceType, methodInfo))
            {
                actionAttr = candidate.GetCustomAttribute<PermissionActionAttribute>();
                if (actionAttr != null)
                {
                    return actionAttr;
                }
            }

            // 4. 接口方法上的特性（ABP 动态代理场景）
            var interfaceMethods = FindInterfaceMethods(serviceType, methodInfo);
            foreach (var interfaceMethod in interfaceMethods)
            {
                actionAttr = interfaceMethod.GetCustomAttribute<PermissionActionAttribute>();
                if (actionAttr != null)
                {
                    return actionAttr;
                }
            }

            return null;
        }

        private IEnumerable<MethodInfo> FindBaseMethods(Type serviceType, MethodInfo methodInfo)
        {
            var current = serviceType.BaseType;
            while (current != null && current != typeof(object))
            {
                foreach (var candidate in current.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (IsSameSignature(candidate, methodInfo))
                    {
                        yield return candidate;
                    }
                }

                current = current.BaseType;
            }
        }

        /// <summary>
        /// 查找服务类型对应接口中匹配的方法
        /// </summary>
        private IEnumerable<MethodInfo> FindInterfaceMethods(Type serviceType, MethodInfo methodInfo)
        {
            var interfaces = serviceType.GetInterfaces();
            foreach (var interfaceType in interfaces)
            {
                var interfaceMethod = interfaceType.GetMethod(
                    methodInfo.Name,
                    methodInfo.GetParameters().Select(p => p.ParameterType).ToArray());
                if (interfaceMethod != null)
                {
                    yield return interfaceMethod;
                }
            }
        }

        private static bool IsSameSignature(MethodInfo candidate, MethodInfo methodInfo)
        {
            if (!string.Equals(candidate.Name, methodInfo.Name, StringComparison.Ordinal))
            {
                return false;
            }

            var candidateParameters = candidate.GetParameters();
            var methodParameters = methodInfo.GetParameters();
            if (candidateParameters.Length != methodParameters.Length)
            {
                return false;
            }

            for (var i = 0; i < candidateParameters.Length; i++)
            {
                var candidateType = candidateParameters[i].ParameterType;
                var methodType = methodParameters[i].ParameterType;
                if (candidateType == methodType)
                {
                    continue;
                }

                if (candidateType.IsGenericParameter)
                {
                    continue;
                }

                if (candidateType.ContainsGenericParameters && methodType.IsConstructedGenericType)
                {
                    continue;
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// 推断资源名（从服务类型名）
        /// </summary>
        private string? InferResourceName(Type serviceType)
        {
            var serviceName = serviceType.Name;
            if (serviceName.EndsWith("Service"))
            {
                serviceName = serviceName.Substring(0, serviceName.Length - 7);
            }

            return EntityNameNormalizer.Normalize(serviceName);
        }

        /// <summary>
        /// 推断模块名（从 RemoteServiceName）
        /// 注意：这是 fallback，优先使用 PermissionResource
        /// </summary>
        private string? InferModuleName(Type serviceType)
        {
            // 从 RemoteServiceName 推断
            var remoteServiceName = GetRemoteServiceName(serviceType);
            if (!string.IsNullOrEmpty(remoteServiceName))
            {
                return MapRemoteServiceNameToModule(remoteServiceName);
            }

            // 未解析返回 null（不再有默认 system）
            return null;
        }

        /// <summary>
        /// RemoteServiceName 到权限模块前缀的映射
        /// </summary>
        private string? MapRemoteServiceNameToModule(string remoteServiceName)
        {
            return remoteServiceName switch
            {
                "rbac" => "system",
                "tenant-management" => "system",
                "audit-logging" => "monitor",
                "setting-management" => "system",
                _ => remoteServiceName
            };
        }

        /// <summary>
        /// 获取 ABP RemoteServiceName（从类型配置推断）
        /// </summary>
        private string? GetRemoteServiceName(Type serviceType)
        {
            // ABP 动态控制器配置的 RemoteServiceName 通常通过 Assembly 配置
            // 这里从类型所在 Assembly 的约定推断
            var assembly = serviceType.Assembly;
            var assemblyName = assembly.GetName().Name;

            if (assemblyName == null) return null;

            // Yi.Module.Rbac.Application -> rbac
            // Yi.Module.AuditLogging.Application -> audit-logging
            if (assemblyName.StartsWith("Yi.Module."))
            {
                var modulePart = assemblyName.Substring("Yi.Module.".Length);
                if (modulePart.Contains(".Application"))
                {
                    modulePart = modulePart.Substring(0, modulePart.IndexOf(".Application"));
                }
                return ToKebabCase(modulePart);
            }

            return null;
        }

        private static string ToKebabCase(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return value;
            }

            var chars = new List<char>(value.Length + 4);
            for (var i = 0; i < value.Length; i++)
            {
                var current = value[i];
                if (char.IsUpper(current) && i > 0)
                {
                    chars.Add('-');
                }

                chars.Add(char.ToLowerInvariant(current));
            }

            return new string(chars.ToArray());
        }
    }
}
