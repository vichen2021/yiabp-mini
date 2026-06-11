using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// 设置管理 Provider 接口，每个 Provider 对应一个隔离维度（默认值 / 配置文件 / 全局 / 租户 / 用户）。
/// </summary>
public interface ISettingManagementProvider
{
    /// <summary>
    /// Provider 名称，对应 ABP <c>SettingValueProvider.ProviderName</c>。
    /// </summary>
    string Name { get; }

    /// <summary>
    /// 读取指定 Setting 在当前 Provider 维度下的值，无值时返回 <c>null</c>。
    /// </summary>
    Task<string?> GetOrNullAsync([NotNull] SettingDefinition setting, [CanBeNull] string? providerKey);

    /// <summary>
    /// 写入指定 Setting 在当前 Provider 维度下的值。
    /// </summary>
    Task SetAsync([NotNull] SettingDefinition setting, [NotNull] string value, [CanBeNull] string? providerKey);

    /// <summary>
    /// 删除指定 Setting 在当前 Provider 维度下的值（使其回退到下一级 Provider）。
    /// </summary>
    Task ClearAsync([NotNull] SettingDefinition setting, [CanBeNull] string? providerKey);
}
