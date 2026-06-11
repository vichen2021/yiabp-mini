using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// 配置文件 Provider，从 <c>appsettings.json</c>（<see cref="IConfiguration"/>）读取 Setting 值。
/// 只读，不支持写入或清除。
/// </summary>
public class ConfigurationSettingManagementProvider : ISettingManagementProvider, ITransientDependency
{
    /// <inheritdoc/>
    public string Name => ConfigurationSettingValueProvider.ProviderName;

    /// <summary>ABP 应用配置对象。</summary>
    protected IConfiguration Configuration { get; }

    /// <summary>注入 <see cref="IConfiguration"/>。</summary>
    public ConfigurationSettingManagementProvider(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    /// <summary>从 <c>appsettings.json</c> 读取对应 Setting 的配置值。</summary>
    public virtual Task<string?> GetOrNullAsync(SettingDefinition setting, string? providerKey)
    {
        return Task.FromResult(Configuration[ConfigurationSettingValueProvider.ConfigurationNamePrefix + setting.Name]);
    }

    /// <summary>不支持写入配置文件，调用则抛出异常。</summary>
    public virtual Task SetAsync(SettingDefinition setting, string value, string? providerKey)
    {
        throw new AbpException($"Can not set a setting value to the application configuration.");
    }

    /// <summary>不支持清除配置文件中的值，调用则抛出异常。</summary>
    public virtual Task ClearAsync(SettingDefinition setting, string? providerKey)
    {
        throw new AbpException($"Can not set a setting value to the application configuration.");
    }
}
