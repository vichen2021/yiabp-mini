using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Yi.Framework.ActionMetadata.Abstractions.Metadata;
using Yi.Framework.ActionMetadata.Core.Metadata;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Metadata;

namespace Yi.Framework.Authorization.Core.Metadata
{
    /// <summary>
    /// 默认权限要求解析器。仅解析权限决策所需信息，不涉及操作记录。
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

        private static PermissionRequirement ResolvePermission(ActionIdentity identity, MethodInfo methodInfo)
        {
            var requirement = new PermissionRequirement();

            if (HasIgnorePermission(methodInfo, identity.ServiceType))
            {
                requirement.Ignore = true;
                requirement.IsResolved = true;
                requirement.Source = "IgnorePermission";
                return requirement;
            }

            var permissionAttr = ActionReflectionHelper.GetMethodAttribute<PermissionAttribute>(methodInfo, identity.ServiceType);
            if (permissionAttr != null && !string.IsNullOrEmpty(permissionAttr.Code))
            {
                requirement.Code = permissionAttr.Code;
                requirement.IsResolved = true;
                requirement.Source = "ExplicitPermission";
                return requirement;
            }

            var resourceAttr = ResolvePermissionResource(identity.ServiceType);
            if (resourceAttr == null)
            {
                requirement.IsResolved = false;
                requirement.UnresolvedReason = "No PermissionResource on class";
                return requirement;
            }

            var actionAttr = ActionReflectionHelper.GetMethodAttribute<PermissionActionAttribute>(methodInfo, identity.ServiceType);
            if (actionAttr == null || string.IsNullOrWhiteSpace(actionAttr.Action))
            {
                requirement.IsResolved = false;
                requirement.UnresolvedReason = "No PermissionAction on method or base method";
                return requirement;
            }

            requirement.Code = $"{resourceAttr.Module}:{resourceAttr.Resource}:{actionAttr.Action}";
            requirement.IsResolved = true;
            requirement.Source = "PermissionAction+PermissionResource";

            return requirement;
        }

        private static bool HasIgnorePermission(MethodInfo methodInfo, Type serviceType)
        {
            if (ActionReflectionHelper.GetMethodAttribute<IgnorePermissionAttribute>(methodInfo, serviceType) != null)
            {
                return true;
            }

            return serviceType.GetCustomAttribute<IgnorePermissionAttribute>() != null;
        }

        private static PermissionResourceAttribute? ResolvePermissionResource(Type serviceType)
        {
            var resourceAttr = serviceType.GetCustomAttribute<PermissionResourceAttribute>(inherit: true);
            if (resourceAttr != null)
            {
                return resourceAttr;
            }

            foreach (var interfaceType in serviceType.GetInterfaces())
            {
                resourceAttr = interfaceType.GetCustomAttribute<PermissionResourceAttribute>(inherit: true);
                if (resourceAttr != null)
                {
                    return resourceAttr;
                }
            }

            return null;
        }
    }
}
