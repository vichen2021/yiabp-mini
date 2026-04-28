using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Volo.Abp;
using Yi.Framework.Operation.Abstractions.Attributes;
using Yi.Framework.Operation.Abstractions.Metadata;
using Yi.Framework.Operation.Abstractions.Permissions;

namespace Yi.Framework.Operation.Core.Filters
{
    /// <summary>
    /// 权限授权 Filter - 异步实现
    /// 权限准入判断顺序：
    /// 1. [AllowAnonymous] -> 放行
    /// 2. [IgnorePermission] -> 放行
    /// 3. ABP RemoteService 禁用 -> 放行
    /// 4. [Permission("...")] -> 校验指定权限码
    /// 5. 自动推断出权限码 -> 校验推断权限码
    /// 6. 未推断出权限码：
    ///    - AllowUnresolvedActions = true  -> 放行
    ///    - AllowUnresolvedActions = false -> 403
    /// </summary>
    public class PermissionAuthorizationFilter : IAsyncAuthorizationFilter
    {
        private readonly IActionMetadataResolver _metadataResolver;
        private readonly IPermissionHandler _permissionHandler;
        private readonly PermissionOptions _options;

        public PermissionAuthorizationFilter(
            IActionMetadataResolver metadataResolver,
            IPermissionHandler permissionHandler,
            PermissionOptions options)
        {
            _metadataResolver = metadataResolver;
            _permissionHandler = permissionHandler;
            _options = options;
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

            // 2. 检查 [IgnorePermission]
            if (HasIgnorePermissionAttribute(descriptor))
            {
                return;
            }

            // 3. 检查 ABP RemoteService 是否禁用
            if (IsRemoteServiceDisabled(descriptor))
            {
                return;
            }

            // 4. 检查白名单
            if (IsInWhitelist(descriptor))
            {
                return;
            }

            // 解析元数据
            var metadata = _metadataResolver.Resolve(descriptor);

            // 获取权限码（显式优先）
            var permissionCode = metadata.ExplicitPermissionCode ?? metadata.PermissionCode;

            // 5. 未推断权限码的处理
            if (string.IsNullOrEmpty(permissionCode) || !metadata.IsResolved)
            {
                if (!_options.AllowUnresolvedActions)
                {
                    context.Result = new ForbidResult();
                }
                return;
            }

            // 6. 权限检查
            var isGranted = await _permissionHandler.IsGrantedAsync(permissionCode);
            if (!isGranted)
            {
                context.Result = new ForbidResult();
            }
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

            // 检查控制器上的特性
            if (descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any())
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// 检查是否标记了 [IgnorePermission]
        /// </summary>
        private bool HasIgnorePermissionAttribute(ControllerActionDescriptor descriptor)
        {
            return descriptor.MethodInfo.GetCustomAttributes(typeof(IgnorePermissionAttribute), true).Any();
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

        /// <summary>
        /// 检查白名单
        /// </summary>
        private bool IsInWhitelist(ControllerActionDescriptor descriptor)
        {
            var actionKey = $"{descriptor.ControllerTypeInfo.Name}:{descriptor.MethodInfo.Name}";
            return _options.WhitelistActions.Contains(actionKey);
        }
    }
}