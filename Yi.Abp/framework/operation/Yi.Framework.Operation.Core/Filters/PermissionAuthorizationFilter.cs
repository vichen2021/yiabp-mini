using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Yi.Framework.Operation.Abstractions.Metadata;
using Yi.Framework.Operation.Abstractions.Permissions;

namespace Yi.Framework.Operation.Core.Filters
{
    /// <summary>
    /// 权限授权 Filter - 使用 IPermissionRequirementResolver
    /// 权限准入判断顺序：
    /// 1. [AllowAnonymous] -> 放行
    /// 2. PermissionRequirement.Ignore = true -> 放行
    /// 3. ABP RemoteService 禁用 -> 放行
    /// 4. PermissionRequirement.IsResolved + Code -> 校验权限码
    /// 5. 未解析 -> 403
    /// </summary>
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IPermissionRequirementResolver _permissionResolver;
        private readonly IPermissionHandler _permissionHandler;

        public PermissionAuthorizationFilter(
            IPermissionRequirementResolver permissionResolver,
            IPermissionHandler permissionHandler)
        {
            _permissionResolver = permissionResolver;
            _permissionHandler = permissionHandler;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor == null) return;

            // 1. 检查 [AllowAnonymous]
            if (IsAllowAnonymous(descriptor))
            {
                return;
            }

            // 2. 解析权限要求（使用新 Resolver）
            var requirement = _permissionResolver.Resolve(descriptor);

            // 3. 检查 Ignore
            if (requirement.Ignore)
            {
                return;
            }

            // 4. 检查 ABP RemoteService 是否禁用
            if (IsRemoteServiceDisabled(descriptor))
            {
                return;
            }

            // 5. 已解析权限码 -> 校验
            if (requirement.IsResolved && !string.IsNullOrEmpty(requirement.Code))
            {
                var isGranted = await _permissionHandler.IsGrantedAsync(requirement.Code);
                if (!isGranted)
                {
                    context.Result = new ForbidResult();
                }
                return;
            }

            // 6. 未解析 -> 固定 403
            context.Result = new ForbidResult();
        }

        /// <summary>
        /// 检查是否标记了 [AllowAnonymous]
        /// </summary>
        private bool IsAllowAnonymous(ControllerActionDescriptor descriptor)
        {
            // 检查方法上的特性
            if (descriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
            {
                return true;
            }

            // 检查 EndpointMetadata，兼容 ABP 动态控制器
            if (descriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
            {
                return true;
            }

            // 检查控制器上的特性
            if (descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查 ABP RemoteService 是否禁用
        /// </summary>
        private bool IsRemoteServiceDisabled(ControllerActionDescriptor descriptor)
        {
            // ABP 的 RemoteServiceAttribute 在方法上标记 IsEnabled = false 时，该方法不是 API
            var remoteServiceAttr = descriptor.MethodInfo
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType().Name == "RemoteServiceAttribute");

            if (remoteServiceAttr != null)
            {
                var isEnabledProperty = remoteServiceAttr.GetType().GetProperty("IsEnabled");
                if (isEnabledProperty != null)
                {
                    var isEnabled = isEnabledProperty.GetValue(remoteServiceAttr) as bool?;
                    if (isEnabled == false)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

    }
}
