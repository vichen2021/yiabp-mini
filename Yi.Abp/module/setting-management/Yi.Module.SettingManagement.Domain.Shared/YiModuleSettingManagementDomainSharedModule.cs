using Volo.Abp.Modularity;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain.Shared;

/// <summary>
/// SettingManagement 模块的契约层 AbpModule。
/// 仅包含 <c>ISettingManager</c> 接口及各维度扩展方法，不依赖任何实现。
/// 跨模块（如 TenantManagement、FileManagement 的运营管理 API）只需依赖本模块即可使用 SettingManager。
///
/// <para>
/// 注意：运行时 DI 隐性约束：
/// 本模块只声明契约，<c>ISettingManager</c> 的实现 <c>SettingManager</c> 位于
/// <c>Yi.Module.SettingManagement.Domain</c>。仅引用本模块（Domain.Shared）只能通过编译，
/// 启动时 ABP 不会自动加载 Domain 模块，<c>ISettingManager</c> 解析会失败。
/// 必须由 Host 工程或依赖链上的某个模块显式 <c>[DependsOn]</c> <c>YiModuleSettingManagementDomainModule</c>，
/// 让实现进入 DI 容器。
/// </para>
///
/// <para>
/// 注意：偏离 ABP 官方惯例：
/// ABP 自家 <c>Volo.Abp.SettingManagement</c> 将 <c>ISettingManager</c> 放在 Domain 层。
/// 本项目特意拆出 Domain.Shared 是为了消除"跨模块依赖 Domain 实现层"的耦合，并且我有一些代码洁癖。
/// 代价是与 ABP 标准结构不一致。新增功能时仍应遵守：契约/扩展放本模块，实现/Provider/Store 放 Domain。
/// </para>
/// </summary>
[DependsOn(typeof(AbpSettingsModule))]
public class YiModuleSettingManagementDomainSharedModule : AbpModule
{
}
