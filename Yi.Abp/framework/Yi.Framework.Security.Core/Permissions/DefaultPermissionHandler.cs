using Microsoft.AspNetCore.Http;
using Yi.Framework.Security.Abstractions.Consts;
using Yi.Framework.Security.Abstractions.Permissions;

namespace Yi.Framework.Security.Core.Permissions
{
    /// <summary>
    /// 默认权限检查实现（基于 HttpContext 中的 Claims）
    /// </summary>
    public class DefaultPermissionHandler : IPermissionHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DefaultPermissionHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<bool> IsGrantedAsync(string permissionCode)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            if (httpContext == null || httpContext.User?.Identity?.IsAuthenticated != true)
            {
                return false;
            }

            // 从 Claims 中获取权限列表
            var permissions = httpContext.User.Claims
                .Where(c => c.Type == TokenTypeConst.Permission)
                .Select(c => c.Value)
                .ToList();

            // 检查是否拥有权限
            return permissions.Contains(permissionCode);
        }
    }
}