using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
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
            IOptions<PermissionOptions> options)
        {
            _metadataResolver = metadataResolver;
            _permissionHandler = permissionHandler;
            _options = options.Value;
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

            // 2. 解析元数据，后续权限和日志共用同一套规则
            var metadata = _metadataResolver.Resolve(descriptor);

            // 3. 检查 [IgnorePermission]
            if (HasIgnorePermissionAttribute(descriptor) || metadata.IgnorePermission)
            {
                return;
            }

            // 4. 检查 ABP RemoteService 是否禁用
            if (IsRemoteServiceDisabled(descriptor))
            {
                return;
            }

            // 5. 检查白名单
            if (IsInWhitelist(descriptor))
            {
                return;
            }

            // 6. 显式 [Permission] 必须校验（不受 IsResolved 影响）
            if (!string.IsNullOrEmpty(metadata.ExplicitPermissionCode))
            {
                var isGranted = await _permissionHandler.IsGrantedAsync(metadata.ExplicitPermissionCode);
                if (!isGranted)
                {
                    context.Result = new ForbidResult();
                }
                return;
            }

            // 7. 自动推断权限码的处理
            if (metadata.IsResolved && !string.IsNullOrEmpty(metadata.PermissionCode))
            {
                // AutoCheckResolvedActions 控制是否校验自动推断的权限
                if (!_options.AutoCheckResolvedActions)
                {
                    return;
                }

                var isGranted = await _permissionHandler.IsGrantedAsync(metadata.PermissionCode);
                if (!isGranted)
                {
                    context.Result = new ForbidResult();
                }
                return;
            }

            // 8. 未推断且无显式权限的处理
            if (!_options.AllowUnresolvedActions)
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
        /// 检查是否标记了 [IgnorePermission]
        /// 支持类级和方法级
        /// </summary>
        private bool HasIgnorePermissionAttribute(ControllerActionDescriptor descriptor)
        {
            // 方法级优先
            if (descriptor.MethodInfo.GetCustomAttributes(typeof(IgnorePermissionAttribute), true).Any())
            {
                return true;
            }

            // 检查 EndpointMetadata，兼容 ABP 动态控制器
            if (descriptor.EndpointMetadata.OfType<IgnorePermissionAttribute>().Any())
            {
                return true;
            }

            // 类级
            if (descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(IgnorePermissionAttribute), true).Any())
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
