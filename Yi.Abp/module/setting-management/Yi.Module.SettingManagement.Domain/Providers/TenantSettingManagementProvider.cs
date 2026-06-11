using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// 租户级 Setting Provider，对应当前租户下的独立配置隔离。
/// <see cref="NormalizeProviderKey"/> 使用当前租户 ID 作为 Key。
/// </summary>
public class TenantSettingManagementProvider : SettingManagementProvider, ITransientDependency
{
    /// <inheritdoc/>
    public override string Name => TenantSettingValueProvider.ProviderName;

    /// <summary>当前租户上下文。</summary>
    protected ICurrentTenant CurrentTenant { get; }

    /// <summary>注入 Store 和租户上下文。</summary>
    public TenantSettingManagementProvider(
        ISettingManagementStore settingManagementStore,
        ICurrentTenant currentTenant)
        : base(settingManagementStore)
    {
        CurrentTenant = currentTenant;
    }

    /// <summary>
    /// 若传入了显式 Key 则优先使用；否则取当前租户 ID。
    /// </summary>
    protected override string? NormalizeProviderKey(string? providerKey)
    {
        if (providerKey != null)
        {
            return providerKey;
        }

        return CurrentTenant.Id?.ToString();
    }
}
