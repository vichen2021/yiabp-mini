using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain.Shared;

/// <summary>
/// <see cref="ISettingManager"/> 全局维度的扩展方法，对所有租户共享的系统级默认配置进行读写。
/// </summary>
public static class GlobalSettingManagerExtensions
{
    /// <summary>读取全局维度下指定 Setting 的值。</summary>
    public static Task<string?> GetOrNullGlobalAsync(this ISettingManager settingManager, [NotNull] string name, bool fallback = true)
    {
        return settingManager.GetOrNullAsync(name, GlobalSettingValueProvider.ProviderName, null, fallback);
    }

    /// <summary>读取全局维度下所有 Setting 的值列表。</summary>
    public static Task<List<SettingValue>> GetAllGlobalAsync(this ISettingManager settingManager, bool fallback = true)
    {
        return settingManager.GetAllAsync(GlobalSettingValueProvider.ProviderName, null, fallback);
    }

    /// <summary>写入全局维度下指定 Setting 的值。</summary>
    public static Task SetGlobalAsync(this ISettingManager settingManager, [NotNull] string name, [CanBeNull] string? value)
    {
        return settingManager.SetAsync(name, value, GlobalSettingValueProvider.ProviderName, null);
    }
}
