using System.Threading.Tasks;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities.Events;
using Volo.Abp.EventBus;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// Setting 缓存失效处理器。
/// 监听 <see cref="SettingAggregateRoot"/> 的任意变更事件（新增/更新/删除），自动从分布式缓存中移除对应 Key。
/// </summary>
public class SettingCacheItemInvalidator : ILocalEventHandler<EntityChangedEventData<SettingAggregateRoot>>, ITransientDependency
{
    /// <summary>分布式缓存，用于移除失效的 Setting 缓存项。</summary>
    protected IDistributedCache<SettingCacheItem> Cache { get; }

    /// <summary>注入分布式缓存。</summary>
    public SettingCacheItemInvalidator(IDistributedCache<SettingCacheItem> cache)
    {
        Cache = cache;
    }

    /// <summary>
    /// 当 Setting 实体发生变更时，移除对应缓存 Key，确保下次读取时重新从数据库加载。
    /// </summary>
    public virtual async Task HandleEventAsync(EntityChangedEventData<SettingAggregateRoot> eventData)
    {
        var cacheKey = CalculateCacheKey(
            eventData.Entity.Name,
            eventData.Entity.ProviderName,
            eventData.Entity.ProviderKey
        );

        await Cache.RemoveAsync(cacheKey, considerUow: true);
    }

    /// <summary>
    /// 计算缓存 Key，委托给 <see cref="SettingCacheItem.CalculateCacheKey"/>。
    /// </summary>
    protected virtual string CalculateCacheKey(string name, string providerName, string providerKey)
    {
        return SettingCacheItem.CalculateCacheKey(name, providerName, providerKey);
    }
}
