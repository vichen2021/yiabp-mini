using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Yi.Framework.Authorization.Abstractions.Metadata;
using Yi.Framework.Authorization.Abstractions.Permissions;

namespace Yi.Framework.Authorization.Core.Filters
{
    /// <summary>
    /// 权限授权 Filter。
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
            if (context.ActionDescriptor is not ControllerActionDescriptor descriptor)
            {
                return;
            }

            if (IsAllowAnonymous(descriptor))
            {
                return;
            }

            var requirement = _permissionResolver.Resolve(descriptor);
            if (requirement.Ignore)
            {
                return;
            }

            if (IsRemoteServiceDisabled(descriptor))
            {
                return;
            }

            if (requirement.IsResolved && !string.IsNullOrEmpty(requirement.Code))
            {
                var isGranted = await _permissionHandler.IsGrantedAsync(requirement.Code);
                if (!isGranted)
                {
                    context.Result = new ForbidResult();
                }

                return;
            }

            context.Result = new ForbidResult();
        }

        private static bool IsAllowAnonymous(ControllerActionDescriptor descriptor)
        {
            return descriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any()
                   || descriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any()
                   || descriptor.ControllerTypeInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Any();
        }

        private static bool IsRemoteServiceDisabled(ControllerActionDescriptor descriptor)
        {
            var remoteServiceAttr = descriptor.MethodInfo
                .GetCustomAttributes(true)
                .FirstOrDefault(a => a.GetType().Name == "RemoteServiceAttribute");

            if (remoteServiceAttr == null)
            {
                return false;
            }

            var isEnabledProperty = remoteServiceAttr.GetType().GetProperty("IsEnabled");
            var isEnabled = isEnabledProperty?.GetValue(remoteServiceAttr) as bool?;
            return isEnabled == false;
        }
    }
}
