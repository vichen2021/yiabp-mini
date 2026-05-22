using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// 全局 Setting Provider，对应所有租户共享的系统级默认配置。
/// <see cref="NormalizeProviderKey"/> 始终返回 <c>null</c>，表示没有隔离维度。
/// </summary>
public class GlobalSettingManagementProvider : SettingManagementProvider, ITransientDependency
{
    /// <inheritdoc/>
    public override string Name => GlobalSettingValueProvider.ProviderName;

    /// <summary>注入 <see cref="ISettingManagementStore"/>。</summary>
    public GlobalSettingManagementProvider(ISettingManagementStore settingManagementStore)
        : base(settingManagementStore)
    {

    }

    /// <summary>全局维度无 Key，始终返回 <c>null</c>。</summary>
    protected override string NormalizeProviderKey(string providerKey)
    {
        return null;
    }
}
