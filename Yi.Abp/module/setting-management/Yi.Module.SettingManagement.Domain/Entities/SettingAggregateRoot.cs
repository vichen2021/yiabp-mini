using JetBrains.Annotations;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Domain.Entities;
using Check = Volo.Abp.Check;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// Setting 聚合根
/// 每条记录代表一个 Setting 在特定 Provider 维度（ProviderName + ProviderKey）下的存储值。
/// </summary>
[SugarTable("Setting")]
public class SettingAggregateRoot : Entity<Guid>, IAggregateRoot<Guid>
{
    /// <summary>Setting 键名，对应 <see cref="Volo.Abp.Settings.SettingDefinition.Name"/>。</summary>
    [NotNull]
    public virtual string Name { get; protected set; }

    /// <summary>Setting 的存储值（明文或加密后的密文）。</summary>
    [NotNull]
    public virtual string Value { get; internal set; }

    /// <summary>Provider 名称，如 T（Tenant）、U（User）、G（Global）等。</summary>
    [CanBeNull]
    public virtual string? ProviderName { get; protected set; }

    /// <summary>Provider 唯一键，如租户 ID、用户 ID；全局/默认级别为 null。</summary>
    [CanBeNull]
    public virtual string? ProviderKey { get; protected set; }

    public SettingAggregateRoot()
    {

    }

    public SettingAggregateRoot(
        Guid id,
        [NotNull] string name,
        [NotNull] string value,
        [CanBeNull] string? providerName = null,
        [CanBeNull] string? providerKey = null)
    {
        Check.NotNull(name, nameof(name));
        Check.NotNull(value, nameof(value));

        Id = id;
        Name = name;
        Value = value;
        ProviderName = providerName;
        ProviderKey = providerKey;
    }

    public override string ToString()
    {
        return $"{base.ToString()}, Name = {Name}, Value = {Value}, ProviderName = {ProviderName}, ProviderKey = {ProviderKey}";
    }
}
