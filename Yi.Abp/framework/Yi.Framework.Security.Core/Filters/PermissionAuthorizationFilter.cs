using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Yi.Framework.Security.Abstractions.Attributes;
using Yi.Framework.Security.Abstractions.Metadata;
using Yi.Framework.Security.Abstractions.Permissions;

namespace Yi.Framework.Security.Core.Filters
{
    /// <summary>
    /// 权限授权 Filter
    /// </summary>
    public class PermissionAuthorizationFilter : IAuthorizationFilter
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

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var descriptor = context.ActionDescriptor as ControllerActionDescriptor;
            if (descriptor == null) return;

            // 解析元数据
            var metadata = _metadataResolver.Resolve(descriptor);

            // 忽略权限检查
            if (metadata.IgnorePermission)
            {
                return;
            }

            // 检查白名单
            if (IsInWhitelist(descriptor))
            {
                return;
            }

            // 获取权限码（显式优先）
            var permissionCode = metadata.ExplicitPermissionCode ?? metadata.PermissionCode;

            // 未推断权限码的处理
            if (permissionCode == null || !metadata.IsResolved)
            {
                if (_options.DefaultDenyUnresolved && !_options.AllowUnresolvedActions)
                {
                    context.Result = new ForbidResult();
                    return;
                }
                return; // 允许通过
            }

            // 权限检查
            var isGranted = _permissionHandler.IsGrantedAsync(permissionCode).Result;
            if (!isGranted)
            {
                context.Result = new ForbidResult();
            }
        }

        private bool IsInWhitelist(ControllerActionDescriptor descriptor)
        {
            var actionKey = $"{descriptor.ControllerTypeInfo.Name}:{descriptor.MethodInfo.Name}";
            return _options.WhitelistActions.Contains(actionKey);
        }
    }
}