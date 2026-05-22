using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application;
using Yi.Module.FileManagement.Domain.Shared;
using Yi.Module.SettingManagement.Domain.Shared;
using Yi.Module.TenantManagement.Application.Contracts;
using Yi.Module.TenantManagement.Domain;

namespace Yi.Module.TenantManagement.Application
{
    [DependsOn(
        typeof(YiModuleTenantManagementDomainModule),
        typeof(YiModuleTenantManagementApplicationContractsModule),
        typeof(YiModuleSettingManagementDomainSharedModule),
        typeof(YiModuleFileManagementDomainSharedModule))]
    public class YiModuleTenantManagementApplicationModule : AbpModule
    {
    }
}
