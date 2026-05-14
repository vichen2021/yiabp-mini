using Volo.Abp.SettingManagement;
using Yi.Abp.Application.Contracts;
using Yi.Abp.Domain;
using Yi.Framework.Ddd.Application;
using Yi.Module.FileManagement.Application;
using Yi.Module.Rbac.Application;
using Yi.Module.SettingManagement.Application;
using Yi.Module.TenantManagement.Application;

namespace Yi.Abp.Application
{
    [DependsOn(
        typeof(YiAbpApplicationContractsModule),
        typeof(YiAbpDomainModule),
        typeof(YiModuleRbacApplicationModule),
        typeof(YiModuleTenantManagementApplicationModule),
        typeof(YiModuleSettingManagementApplicationModule),
        typeof(YiFrameworkDddApplicationModule),
        typeof(YiModuleFileManagementApplicationModule))]
    public class YiAbpApplicationModule : AbpModule
    {
    }
}
