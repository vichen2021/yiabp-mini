using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Metadata;

namespace Yi.Framework.Authorization.Core.Metadata
{
    /// <summary>
    /// 默认权限要求解析器。仅解析权限决策所需信息，不涉及操作记录。
    /// </summary>
    public class DefaultPermissionRequirementResolver : IPermissionRequirementResolver
    {
        /// <summary>
        /// 从 MVC Action 解析权限要求
        /// </summary>
        public PermissionRequirement Resolve(ControllerActionDescriptor descriptor)
        {
            return Resolve(descriptor.ControllerTypeInfo.AsType(), descriptor.MethodInfo);
        }

        /// <summary>
        /// 从应用服务类型和方法解析权限要求
        /// </summary>
        public PermissionRequirement Resolve(Type serviceType, MethodInfo methodInfo)
        {
            var requirement = new PermissionRequirement();

            if (HasIgnorePermission(methodInfo, serviceType))
            {
                requirement.Ignore = true;
                requirement.IsResolved = true;
                requirement.Source = "IgnorePermission";
                return requirement;
            }

            var permissionAttr = PermissionReflectionHelper.GetMethodAttribute<PermissionAttribute>(methodInfo, serviceType);
            if (permissionAttr != null && !string.IsNullOrEmpty(permissionAttr.Code))
            {
                requirement.Code = permissionAttr.Code;
                requirement.IsResolved = true;
                requirement.Source = "ExplicitPermission";
                return requirement;
            }

            var resourceAttr = ResolvePermissionResource(serviceType);
            if (resourceAttr == null)
            {
                requirement.IsResolved = false;
                requirement.UnresolvedReason = "No PermissionResource on class";
                return requirement;
            }

            var actionAttr = PermissionReflectionHelper.GetMethodAttribute<PermissionActionAttribute>(methodInfo, serviceType);
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
            if (PermissionReflectionHelper.GetMethodAttribute<IgnorePermissionAttribute>(methodInfo, serviceType) != null)
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
