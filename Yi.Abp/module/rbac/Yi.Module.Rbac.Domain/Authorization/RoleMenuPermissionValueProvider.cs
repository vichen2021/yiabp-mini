using System.Security.Claims;
using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Claims;
using Yi.Module.Rbac.Domain.Repositories;
using Yi.Module.Rbac.Domain.Shared.Authorization;
using Yi.Module.Rbac.Domain.Shared.Caches;
using Yi.Module.Rbac.Domain.Shared.Consts;

namespace Yi.Module.Rbac.Domain.Authorization;

/// <summary>
/// 基于角色菜单关系提供 ABP 权限值
/// </summary>
public class RoleMenuPermissionValueProvider : IPermissionValueProvider, ITransientDependency
{
    public const string ProviderName = "RoleMenu";

    private readonly IUserRepository _userRepository;
    private readonly ICurrentTenant _currentTenant;
    private readonly IPermissionCacheVersionManager _cacheVersionManager;
    private readonly IDistributedCache<PermissionGrantCacheItem, PermissionGrantCacheKey> _permissionCache;

    /// <summary>
    /// 创建基于角色菜单关系的 ABP 权限值提供器
    /// </summary>
    public RoleMenuPermissionValueProvider(
        IUserRepository userRepository,
        ICurrentTenant currentTenant,
        IPermissionCacheVersionManager cacheVersionManager,
        IDistributedCache<PermissionGrantCacheItem, PermissionGrantCacheKey> permissionCache)
    {
        _userRepository = userRepository;
        _currentTenant = currentTenant;
        _cacheVersionManager = cacheVersionManager;
        _permissionCache = permissionCache;
    }

    public string Name => ProviderName;

    /// <summary>
    /// 检查当前主体是否通过角色菜单获得指定权限
    /// </summary>
    public async Task<PermissionGrantResult> CheckAsync(PermissionValueCheckContext context)
    {
        var snapshot = await GetGrantedPermissionsAsync(context.Principal);
        return snapshot.GrantAll || snapshot.PermissionCodes.Contains(context.Permission.Name, StringComparer.OrdinalIgnoreCase)
            ? PermissionGrantResult.Granted
            : PermissionGrantResult.Prohibited;
    }

    /// <summary>
    /// 批量检查当前主体通过角色菜单获得的权限
    /// </summary>
    public async Task<MultiplePermissionGrantResult> CheckAsync(PermissionValuesCheckContext context)
    {
        var snapshot = await GetGrantedPermissionsAsync(context.Principal);
        var result = new MultiplePermissionGrantResult();
        foreach (var permission in context.Permissions)
        {
            result.Result[permission.Name] = snapshot.GrantAll || snapshot.PermissionCodes.Contains(permission.Name, StringComparer.OrdinalIgnoreCase)
                ? PermissionGrantResult.Granted
                : PermissionGrantResult.Prohibited;
        }

        return result;
    }

    /// <summary>
    /// 获取当前主体在当前租户内由启用角色菜单授予的权限集合
    /// </summary>
    private async Task<PermissionGrantCacheItem> GetGrantedPermissionsAsync(ClaimsPrincipal? principal)
    {
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return new PermissionGrantCacheItem();
        }

        var userIdValue = principal.FindFirst(AbpClaimTypes.UserId)?.Value;
        if (!Guid.TryParse(userIdValue, out var userId))
        {
            return new PermissionGrantCacheItem();
        }

        var version = await _cacheVersionManager.GetAsync(_currentTenant.Id);
        return await _permissionCache.GetOrAddAsync(
                   new PermissionGrantCacheKey(_currentTenant.Id, userId, version),
                   () => BuildGrantedPermissionsAsync(userId),
                   () => new DistributedCacheEntryOptions
                   {
                       AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(30)
                   }) ?? new PermissionGrantCacheItem();
    }

    /// <summary>
    /// 从当前租户数据库构建用户授权快照
    /// </summary>
    private async Task<PermissionGrantCacheItem> BuildGrantedPermissionsAsync(Guid userId)
    {
        var result = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        var user = await _userRepository.GetUserAllInfoAsync(userId);
        if (user == null || user.IsDeleted || !user.State)
        {
            return new PermissionGrantCacheItem();
        }

        // 超级管理员仅在 Host 上下文放行，防止租户请求绕过套餐物化后的角色菜单边界
        if (_currentTenant.Id == null && string.Equals(user.UserName, UserConst.SuperAdminUserName, StringComparison.Ordinal))
        {
            return new PermissionGrantCacheItem { GrantAll = true };
        }

        foreach (var role in user.Roles.Where(role => !role.IsDeleted && role.State && role.TenantId == _currentTenant.Id))
        {
            foreach (var permissionCode in role.Menus?
                         .Where(menu => !menu.IsDeleted && menu.State && !string.IsNullOrWhiteSpace(menu.PermissionCode))
                         .Select(menu => menu.PermissionCode!) ?? Enumerable.Empty<string>())
            {
                result.Add(permissionCode);
            }
        }

        return new PermissionGrantCacheItem { PermissionCodes = result.ToArray() };
    }
}
