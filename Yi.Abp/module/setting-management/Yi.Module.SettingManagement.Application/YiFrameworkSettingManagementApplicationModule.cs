using Volo.Abp.Application;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;
using Volo.Abp.Timing;
using Yi.Module.SettingManagement.Domain;

namespace Yi.Module.SettingManagement.Application;

[DependsOn(
    typeof(AbpDddApplicationModule),
    typeof(AbpSettingManagementApplicationContractsModule),
    typeof(YiModuleSettingManagementDomainModule),
    typeof(AbpTimingModule)

)]
public class YiModuleSettingManagementApplicationModule : AbpModule
{
}
