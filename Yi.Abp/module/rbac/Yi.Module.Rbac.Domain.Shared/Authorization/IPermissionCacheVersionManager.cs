namespace Yi.Module.Rbac.Domain.Shared.Authorization;

/// <summary>
/// 管理租户维度的权限缓存版本
/// </summary>
public interface IPermissionCacheVersionManager
{
    /// <summary>
    /// 获取指定租户当前权限缓存版本
    /// </summary>
    /// <param name="tenantId">租户标识，空值表示宿主</param>
    /// <returns>当前权限缓存版本</returns>
    Task<string> GetAsync(Guid? tenantId);

    /// <summary>
    /// 切换指定租户权限缓存版本使旧授权快照立即失效
    /// </summary>
    /// <param name="tenantId">租户标识，空值表示宿主</param>
    Task InvalidateAsync(Guid? tenantId);
}
