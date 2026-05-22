using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain.Shared;

/// <summary>
/// 设置管理入口接口，负责按 Provider 链顺序查询并写入 Setting 值。
/// 通常不直接使用，推荐通过各 Provider 的扩展方法（如 SetForTenantAsync）调用。
///
/// <para>
/// 本接口位于 Domain.Shared（契约层），实现 <c>SettingManager</c> 在
/// <c>Yi.Module.SettingManagement.Domain</c>。仅引用 Domain.Shared 编译能过但运行时 DI 会失败，
/// Host 必须显式 <c>[DependsOn]</c> <c>YiModuleSettingManagementDomainModule</c>。
/// 这是为消除跨模块对实现层的耦合所付的代价，详见 <c>YiModuleSettingManagementDomainSharedModule</c> 的注释。
/// </para>
/// </summary>
public interface ISettingManager
{
    /// <summary>
    /// 读取指定 Provider 维度下的 Setting 值，<paramref name="fallback"/> 为 true 时自动向下级 Provider 回退。
    /// </summary>
    Task<string> GetOrNullAsync([NotNull] string name, [NotNull] string providerName, [CanBeNull] string providerKey, bool fallback = true);

    /// <summary>
    /// 读取指定 Provider 维度下所有 Setting 值，<paramref name="fallback"/> 为 true 时自动向下级 Provider 回退。
    /// </summary>
    Task<List<SettingValue>> GetAllAsync([NotNull] string providerName, [CanBeNull] string providerKey, bool fallback = true);

    /// <summary>
    /// 写入 Setting 值到指定 Provider 维度，<paramref name="forceToSet"/> 为 true 时强制写入（即使值未变化）。
    /// </summary>
    Task SetAsync([NotNull] string name, [CanBeNull] string value, [NotNull] string providerName, [CanBeNull] string providerKey, bool forceToSet = false);
}
