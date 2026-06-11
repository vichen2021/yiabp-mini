using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// 默认值 Provider，优先级最低。
/// 直接返回 <see cref="SettingDefinition.DefaultValue"/>，不支持写入或清除（默认值只能在代码中定义）。
/// </summary>
public class DefaultValueSettingManagementProvider : ISettingManagementProvider, ISingletonDependency
{
    /// <inheritdoc/>
    public string Name => DefaultValueSettingValueProvider.ProviderName;

    /// <summary>返回 <see cref="SettingDefinition.DefaultValue"/>。</summary>
    public virtual Task<string?> GetOrNullAsync(SettingDefinition setting, string? providerKey)
    {
        return Task.FromResult(setting.DefaultValue);
    }

    /// <summary>不支持设置默认值，调用则抛出异常。</summary>
    public virtual Task SetAsync(SettingDefinition setting, string value, string? providerKey)
    {
        throw new AbpException($"Can not set default value of a setting. It is only possible while defining the setting in a {typeof(ISettingDefinitionProvider)} implementation.");
    }

    /// <summary>不支持清除默认值，调用则抛出异常。</summary>
    public virtual Task ClearAsync(SettingDefinition setting, string? providerKey)
    {
        throw new AbpException($"Can not clear default value of a setting. It is only possible while defining the setting in a {typeof(ISettingDefinitionProvider)} implementation.");
    }
}
