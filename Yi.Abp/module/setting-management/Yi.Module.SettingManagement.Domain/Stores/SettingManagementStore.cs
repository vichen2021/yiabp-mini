using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Volo.Abp.Settings;
using Volo.Abp.Uow;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// <see cref="ISettingManagementStore"/> 的实现，封装对 <see cref="ISettingRepository"/> 的所有 CRUD 操作，
/// 并在读取时引入分布式缓存层以减少数据库压力。
/// 写入和删除同时更新缓存，实现最终一致性。
/// </summary>
public class SettingManagementStore : ISettingManagementStore, ITransientDependency
{
    /// <summary>分布式缓存，用于缓存 Setting 值避免重复查询数据库。</summary>
    protected IDistributedCache<SettingCacheItem> Cache { get; }
    /// <summary>Setting 定义管理器，用于批量刷新缓存时遍历所有已定义的 Setting。</summary>
    protected ISettingDefinitionManager SettingDefinitionManager { get; }
    /// <summary>Setting 聚合根仓储，提供底层数据库操作。</summary>
    protected ISettingRepository SettingRepository { get; }
    /// <summary>GUID 生成器，用于新建 Setting 记录时生成主键。</summary>
    protected IGuidGenerator GuidGenerator { get; }

    /// <summary>注入存储、GUID 生成器、缓存和 Setting 定义管理器。</summary>
    public SettingManagementStore(
        ISettingRepository settingRepository,
        IGuidGenerator guidGenerator,
        IDistributedCache<SettingCacheItem> cache,
        ISettingDefinitionManager settingDefinitionManager)
    {
        SettingRepository = settingRepository;
        GuidGenerator = guidGenerator;
        Cache = cache;
        SettingDefinitionManager = settingDefinitionManager;
    }

    /// <summary>
    /// 读取单个 Setting 值，优先命中缓存；未命中时批量刷新同一 Provider 维度下的全部 Setting 缓存。
    /// </summary>
    [UnitOfWork]
    public virtual async Task<string?> GetOrNullAsync(string name, string providerName, string providerKey)
    {
        return (await GetCacheItemAsync(name, providerName, providerKey)).Value;
    }

    /// <summary>
    /// 写入一个 Setting 值（不存则新增，已存则更新），并同时刷新对应缓存项。
    /// </summary>
    [UnitOfWork]
    public virtual async Task SetAsync(string name, string value, string providerName, string providerKey)
    {
        var setting = await SettingRepository.FindAsync(name, providerName, providerKey);
        if (setting == null)
        {
            setting = new SettingAggregateRoot(GuidGenerator.Create(), name, value, providerName, providerKey);
            await SettingRepository.InsertAsync(setting);
        }
        else
        {
            setting.Value = value;
            await SettingRepository.UpdateAsync(setting);
        }

        await Cache.SetAsync(CalculateCacheKey(name, providerName, providerKey), new SettingCacheItem(setting?.Value), considerUow: true);
    }

    /// <summary>读取指定 Provider 维度下所有 Setting 记录（直接查库，不经过缓存）。</summary>
    public virtual async Task<List<SettingValue>> GetListAsync(string providerName, string providerKey)
    {
        var settings = await SettingRepository.GetListAsync(providerName, providerKey);
        return settings.Select(s => new SettingValue(s.Name, s.Value)).ToList();
    }

    /// <summary>删除指定 Provider 维度下的 Setting 记录，并移除对应缓存项。</summary>
    [UnitOfWork]
    public virtual async Task DeleteAsync(string name, string providerName, string providerKey)
    {
        var setting = await SettingRepository.FindAsync(name, providerName, providerKey);
        if (setting != null)
        {
            await SettingRepository.DeleteAsync(setting);
            await Cache.RemoveAsync(CalculateCacheKey(name, providerName, providerKey), considerUow: true);
        }
    }

    /// <summary>读取单个 Setting 缓存项；未命中时触发 <see cref="SetCacheItemsAsync(string, string, string, SettingCacheItem)"/> 批量刷新。</summary>
    protected virtual async Task<SettingCacheItem> GetCacheItemAsync(string name, string providerName, string providerKey)
    {
        var cacheKey = CalculateCacheKey(name, providerName, providerKey);
        var cacheItem = await Cache.GetAsync(cacheKey, considerUow: true);

        if (cacheItem != null)
        {
            return cacheItem;
        }

        cacheItem = new SettingCacheItem(null);

        await SetCacheItemsAsync(providerName, providerKey, name, cacheItem);

        return cacheItem;
    }

    /// <summary>批量刷新指定 Provider 维度下所有 Setting 的缓存，并回填 <paramref name="currentCacheItem"/>。</summary>
    private async Task SetCacheItemsAsync(
        string providerName,
        string providerKey,
        string currentName,
        SettingCacheItem currentCacheItem)
    {
        var settingDefinitions =await SettingDefinitionManager.GetAllAsync();
        var settingsDictionary = (await SettingRepository.GetListAsync(providerName, providerKey))
            .ToDictionary(s => s.Name, s => s.Value);

        var cacheItems = new List<KeyValuePair<string, SettingCacheItem>>();

        foreach (var settingDefinition in settingDefinitions)
        {
            var settingValue = settingsDictionary.GetOrDefault(settingDefinition.Name);

            cacheItems.Add(
                new KeyValuePair<string, SettingCacheItem>(
                    CalculateCacheKey(settingDefinition.Name, providerName, providerKey),
                    new SettingCacheItem(settingValue)
                )
            );

            if (settingDefinition.Name == currentName)
            {
                currentCacheItem.Value = settingValue;
            }
        }

        await Cache.SetManyAsync(cacheItems, considerUow: true);
    }

    /// <summary>按名称数组批量读取 Setting 值，优先命中缓存，未命中部分按需对数据库批量补充。</summary>
    [UnitOfWork]
    public async Task<List<SettingValue>> GetListAsync(string[] names, string providerName, string providerKey)
    {
        Check.NotNullOrEmpty(names, nameof(names));

        var result = new List<SettingValue>();

        if (names.Length == 1)
        {
            var name = names.First();
            result.Add(new SettingValue(name, (await GetCacheItemAsync(name, providerName, providerKey)).Value));
            return result;
        }

        var cacheItems = await GetCacheItemsAsync(names, providerName, providerKey);
        foreach (var item in cacheItems)
        {
            result.Add(new SettingValue(GetSettingNameFormCacheKeyOrNull(item.Key), item.Value?.Value));
        }

        return result;
    }

    /// <summary>批量读取缓存项；对于缺失项自动补充并合并返回最终结果。</summary>
    protected virtual async Task<List<KeyValuePair<string, SettingCacheItem>>> GetCacheItemsAsync(string[] names, string providerName, string providerKey)
    {
        var cacheKeys = names.Select(x => CalculateCacheKey(x, providerName, providerKey)).ToList();

        var cacheItems = (await Cache.GetManyAsync(cacheKeys, considerUow: true)).ToList();

        if (cacheItems.All(x => x.Value != null))
        {
            return cacheItems.Select(x => new KeyValuePair<string, SettingCacheItem>(x.Key, x.Value!)).ToList();
        }

        var notCacheKeys = cacheItems.Where(x => x.Value == null).Select(x => x.Key).ToList();

        var newCacheItems = await SetCacheItemsAsync(providerName, providerKey, notCacheKeys);

        var result = new List<KeyValuePair<string, SettingCacheItem>>();
        foreach (var key in cacheKeys)
        {
            var item = newCacheItems.FirstOrDefault(x => x.Key == key);
            if (item.Value == null)
            {
                item = cacheItems.FirstOrDefault(x => x.Key == key)!;
            }

            result.Add(new KeyValuePair<string, SettingCacheItem>(key, item.Value!));
        }

        return result;
    }

    /// <summary>根据缺失的缓存 Key 列表查询数据库并回写缓存。</summary>
    private async Task<List<KeyValuePair<string, SettingCacheItem>>> SetCacheItemsAsync(
        string providerName,
        string providerKey,
        List<string> notCacheKeys)
    {
        var settingDefinitions =(await SettingDefinitionManager.GetAllAsync()).Where(x => notCacheKeys.Any(k => GetSettingNameFormCacheKeyOrNull(k) == x.Name));

        var settingsDictionary = (await SettingRepository.GetListAsync(notCacheKeys.Select(GetSettingNameFormCacheKeyOrNull).ToArray(), providerName, providerKey))
            .ToDictionary(s => s.Name, s => s.Value);

        var cacheItems = new List<KeyValuePair<string, SettingCacheItem>>();

        foreach (var settingDefinition in settingDefinitions)
        {
            var settingValue = settingsDictionary.GetOrDefault(settingDefinition.Name);
            cacheItems.Add(
                new KeyValuePair<string, SettingCacheItem>(
                    CalculateCacheKey(settingDefinition.Name, providerName, providerKey),
                    new SettingCacheItem(settingValue)
                )
            );
        }

        await Cache.SetManyAsync(cacheItems, considerUow: true);

        return cacheItems;
    }


    /// <summary>计算缓存 Key，委托给 <see cref="SettingCacheItem.CalculateCacheKey"/>。</summary>
    protected virtual string CalculateCacheKey(string name, string providerName, string providerKey)
    {
        return SettingCacheItem.CalculateCacheKey(name, providerName, providerKey);
    }

    /// <summary>从缓存 Key 反向解析 Setting 名称，委托给 <see cref="SettingCacheItem.GetSettingNameFormCacheKeyOrNull"/>。</summary>
    protected virtual string? GetSettingNameFormCacheKeyOrNull(string key)
    {
        //TODO: throw ex when name is null?
        return SettingCacheItem.GetSettingNameFormCacheKeyOrNull(key);
    }
}
