using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Yi.Framework.ActionMetadata.Abstractions.Metadata;

namespace Yi.Framework.ActionMetadata.Core.Metadata
{
    /// <summary>
    /// 默认 Action 身份解析器。只解析稳定事实，不做权限或操作记录决策。
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

        private static ActionIdentity ParseIdentity(Type serviceType, MethodInfo methodInfo)
        {
            return new ActionIdentity
            {
                ServiceType = serviceType,
                MethodInfo = methodInfo,
                RemoteServiceName = GetRemoteServiceName(serviceType)
            };
        }

        private static string? GetRemoteServiceName(Type serviceType)
        {
            var assemblyName = serviceType.Assembly.GetName().Name;
            if (assemblyName == null)
            {
                return null;
            }

            if (assemblyName.StartsWith("Yi.Module.", StringComparison.Ordinal))
            {
                var modulePart = assemblyName.Substring("Yi.Module.".Length);
                if (modulePart.Contains(".Application", StringComparison.Ordinal))
                {
                    modulePart = modulePart.Substring(0, modulePart.IndexOf(".Application", StringComparison.Ordinal));
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
