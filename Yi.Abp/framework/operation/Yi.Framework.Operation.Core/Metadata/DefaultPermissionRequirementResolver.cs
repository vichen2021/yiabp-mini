using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Yi.Framework.Operation.Abstractions.Attributes;
using Yi.Framework.Operation.Abstractions.Metadata;

namespace Yi.Framework.Operation.Core.Metadata
{
    /// <summary>
    /// 默认权限要求解析器
    /// 仅解析权限决策所需信息，不涉及日志
    /// </summary>
    public class DefaultPermissionRequirementResolver : IPermissionRequirementResolver
    {
        private readonly IActionIdentityResolver _identityResolver;

        public DefaultPermissionRequirementResolver(IActionIdentityResolver identityResolver)
        {
            _identityResolver = identityResolver;
        }

        public PermissionRequirement Resolve(ControllerActionDescriptor descriptor)
        {
            var identity = _identityResolver.Resolve(descriptor);
            return ResolvePermission(identity, descriptor.MethodInfo);
        }

        public PermissionRequirement Resolve(Type serviceType, MethodInfo methodInfo)
        {
            var identity = _identityResolver.Resolve(serviceType, methodInfo);
            return ResolvePermission(identity, methodInfo);
        }

        private PermissionRequirement ResolvePermission(ActionIdentity identity, MethodInfo methodInfo)
        {
            var requirement = new PermissionRequirement();

            // 1. 检查 [IgnorePermission]（方法级 + 类级）
            if (HasIgnorePermission(methodInfo, identity.ServiceType))
            {
                requirement.Ignore = true;
                requirement.IsResolved = true;
                requirement.Source = "IgnorePermission";
                return requirement;
            }

            // 2. 检查显式 [Permission("完整权限码")]
            var permissionAttr = methodInfo.GetCustomAttribute<PermissionAttribute>();
            if (permissionAttr != null && !string.IsNullOrEmpty(permissionAttr.Code))
            {
                requirement.Code = permissionAttr.Code;
                requirement.IsResolved = true;
                requirement.Source = "ExplicitPermission";
                return requirement;
            }

            // 3. 检查是否有 Module 和 Resource
            if (string.IsNullOrEmpty(identity.Module) && string.IsNullOrEmpty(identity.Resource))
            {
                requirement.IsResolved = false;
                requirement.UnresolvedReason = "No PermissionResource on class and no module/resource fallback";
                return requirement;
            }

            // 4. 检查是否有 CrudAction（来自 PermissionAction 或基类）
            if (string.IsNullOrEmpty(identity.CrudAction))
            {
                requirement.IsResolved = false;
                requirement.UnresolvedReason = "No PermissionAction on method or base method";
                return requirement;
            }

            // 5. 组合权限码：module:resource:action
            requirement.Code = $"{identity.Module}:{identity.Resource}:{identity.CrudAction}";
            requirement.IsResolved = true;
            requirement.Source = identity.HasExplicitAction
                ? (identity.HasExplicitResource ? "PermissionAction+PermissionResource" : "PermissionAction+FallbackResource")
                : "BaseMethodPermissionAction+PermissionResource";

            return requirement;
        }

        /// <summary>
        /// 检查是否有 [IgnorePermission] 特性
        /// </summary>
        private bool HasIgnorePermission(MethodInfo methodInfo, Type serviceType)
        {
            // 方法级优先
            if (methodInfo.GetCustomAttribute<IgnorePermissionAttribute>() != null)
            {
                return true;
            }

            // 类级
            if (serviceType.GetCustomAttribute<IgnorePermissionAttribute>() != null)
            {
                return true;
            }

            return false;
        }
    }
}