using Microsoft.Extensions.Caching.Distributed;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Yi.Module.Rbac.Domain.Shared.Authorization;
using Yi.Module.Rbac.Domain.Shared.Caches;

namespace Yi.Module.Rbac.Domain.Authorization;

/// <summary>
/// 使用分布式缓存维护租户权限版本
/// </summary>
public class PermissionCacheVersionManager : IPermissionCacheVersionManager, ITransientDependency
{
    private readonly IDistributedCache<PermissionCacheVersionItem, PermissionCacheVersionKey> _cache;

    /// <summary>
    /// 创建租户权限缓存版本管理器
    /// </summary>
    public PermissionCacheVersionManager(
        IDistributedCache<PermissionCacheVersionItem, PermissionCacheVersionKey> cache)
    {
        _cache = cache;
    }

    /// <summary>
    /// 获取指定租户当前权限缓存版本，不存在时创建初始版本
    /// </summary>
    public async Task<string> GetAsync(Guid? tenantId)
    {
        var item = await _cache.GetOrAddAsync(
            new PermissionCacheVersionKey(tenantId),
            () => Task.FromResult(CreateVersionItem()),
            () => new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365)
            });

        return item!.Version;
    }

    /// <summary>
    /// 写入全新版本使指定租户的旧授权快照不再命中
    /// </summary>
    public async Task InvalidateAsync(Guid? tenantId)
    {
        await _cache.SetAsync(
            new PermissionCacheVersionKey(tenantId),
            CreateVersionItem(),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(365)
            });
    }

    /// <summary>
    /// 创建不可预测且不会与旧值碰撞的权限版本
    /// </summary>
    private static PermissionCacheVersionItem CreateVersionItem()
    {
        return new PermissionCacheVersionItem { Version = Guid.NewGuid().ToString("N") };
    }
}
