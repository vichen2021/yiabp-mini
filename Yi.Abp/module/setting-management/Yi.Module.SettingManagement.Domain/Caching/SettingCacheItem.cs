using System;
using System.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Text.Formatting;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// Setting 分布式缓存项，以 <c>pn:{providerName},pk:{providerKey},n:{name}</c> 为 key 存储单个 Setting 值。
/// 标记 <see cref="IgnoreMultiTenancyAttribute"/> 以避免缓存 key 被租户 ID 污染（ProviderKey 已包含租户信息）。
/// </summary>
[Serializable]
[IgnoreMultiTenancy]
public class SettingCacheItem
{
    private const string CacheKeyFormat = "pn:{0},pk:{1},n:{2}";

    /// <summary>Setting 的缓存值；<c>null</c> 表示该 Provider 维度下无记录。</summary>
    public string? Value { get; set; }

    /// <summary>无参构造（反序列化使用）。</summary>
    public SettingCacheItem()
    {

    }

    /// <summary>以指定值初始化缓存项。</summary>
    public SettingCacheItem(string? value)
    {
        Value = value;
    }

    /// <summary>
    /// 根据 Setting 名称和 Provider 信息计算分布式缓存 Key。
    /// </summary>
    public static string CalculateCacheKey(string name, string providerName, string providerKey)
    {
        return string.Format(CacheKeyFormat, providerName, providerKey, name);
    }

    /// <summary>
    /// 从缓存 Key 反向解析 Setting 名称，格式不匹配时返回 <c>null</c>。
    /// </summary>
    public static string? GetSettingNameFormCacheKeyOrNull(string cacheKey)
    {
        var result = FormattedStringValueExtracter.Extract(cacheKey, CacheKeyFormat, true);
        return result.IsMatch ? result.Matches.Last().Value : null;
    }
}
