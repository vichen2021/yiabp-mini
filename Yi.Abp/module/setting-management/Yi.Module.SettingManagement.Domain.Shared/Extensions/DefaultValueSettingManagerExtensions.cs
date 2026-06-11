using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain.Shared;

/// <summary>
/// <see cref="ISettingManager"/> 默认值维度的扩展方法，读取 <see cref="Volo.Abp.Settings.SettingDefinition.DefaultValue"/>。
/// </summary>
public static class DefaultValueSettingManagerExtensions
{
    /// <summary>读取默认值维度下指定 Setting 的值。</summary>
    public static Task<string?> GetOrNullDefaultAsync(this ISettingManager settingManager, [NotNull] string name, bool fallback = true)
    {
        return settingManager.GetOrNullAsync(name, DefaultValueSettingValueProvider.ProviderName, null, fallback);
    }

    /// <summary>读取默认值维度下所有 Setting 的值列表。</summary>
    public static Task<List<SettingValue>> GetAllDefaultAsync(this ISettingManager settingManager, bool fallback = true)
    {
        return settingManager.GetAllAsync(DefaultValueSettingValueProvider.ProviderName, null, fallback);
    }
}
