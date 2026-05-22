using Volo.Abp.Collections;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// 设置管理模块的注册选项。
/// 通过 <see cref="Providers"/> 控制 Provider 的注册及 fallback 顺序，
/// 列表越靠后优先级越高（User &gt; Tenant &gt; Global &gt; Configuration &gt; Default）。
/// </summary>
public class SettingManagementOptions
{
    /// <summary>
    /// 已注册的 <see cref="ISettingManagementProvider"/> 列表，决定 fallback 链顺序。
    /// </summary>
    public ITypeList<ISettingManagementProvider> Providers { get; }

    /// <summary>
    /// 初始化空的 Provider 列表。
    /// </summary>
    public SettingManagementOptions()
    {
        Providers = new TypeList<ISettingManagementProvider>();
    }
}
