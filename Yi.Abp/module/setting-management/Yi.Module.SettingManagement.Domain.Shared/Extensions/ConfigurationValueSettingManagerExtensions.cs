using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain.Shared;

/// <summary>
/// <see cref="ISettingManager"/> 配置文件维度的扩展方法，封装从 <c>appsettings.json</c> 读取 Setting 的操作。
/// </summary>
public static class ConfigurationValueSettingManagerExtensions
{
    /// <summary>读取配置文件维度下指定 Setting 的值。</summary>
    public static Task<string> GetOrNullConfigurationAsync(this ISettingManager settingManager, [NotNull] string name, bool fallback = true)
    {
        return settingManager.GetOrNullAsync(name, ConfigurationSettingValueProvider.ProviderName, null, fallback);
    }

    /// <summary>读取配置文件维度下所有 Setting 的值列表。</summary>
    public static Task<List<SettingValue>> GetAllConfigurationAsync(this ISettingManager settingManager, bool fallback = true)
    {
        return settingManager.GetAllAsync(ConfigurationSettingValueProvider.ProviderName, null, fallback);
    }
}
