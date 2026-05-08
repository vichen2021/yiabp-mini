using Microsoft.AspNetCore.Http;
using Yi.Framework.Authorization.Abstractions.Consts;
using Yi.Framework.Authorization.Abstractions.Permissions;

namespace Yi.Framework.Authorization.Core.Permissions
{
    /// <summary>
    /// 默认权限检查实现，基于 HttpContext 中的 Claims。
    /// </summary>
    public class DefaultPermissionHandler : IPermissionHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultPermissionHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Task<bool> IsGrantedAsync(string permissionCode)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User?.Identity?.IsAuthenticated != true)
            {
                return Task.FromResult(false);
            }

            var permissions = httpContext.User.Claims
                .Where(c => c.Type == TokenTypeConst.Permission)
                .Select(c => c.Value)
                .ToList();

            return Task.FromResult(permissions.Any(permission => IsPermissionMatch(permission, permissionCode)));
        }

        private static bool IsPermissionMatch(string ownedPermission, string requiredPermission)
        {
            if (string.IsNullOrWhiteSpace(ownedPermission) || string.IsNullOrWhiteSpace(requiredPermission))
            {
                return false;
            }

            if (ownedPermission == "*" || ownedPermission == requiredPermission)
            {
                return true;
            }

            var ownedParts = ownedPermission.Split(':');
            var requiredParts = requiredPermission.Split(':');
            if (ownedParts.Length != requiredParts.Length)
            {
                return false;
            }

            for (var i = 0; i < ownedParts.Length; i++)
            {
                if (ownedParts[i] != "*" && ownedParts[i] != requiredParts[i])
                {
                    return false;
                }
            }

            return true;
        }
    }
}
