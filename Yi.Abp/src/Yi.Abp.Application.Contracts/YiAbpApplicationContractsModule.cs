using Volo.Abp.SettingManagement;
using Yi.Abp.Domain.Shared;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Module.FileManagement.Application.Contracts;
using Yi.Module.Rbac.Application.Contracts;
using Yi.Module.TenantManagement.Application.Contracts;

namespace Yi.Abp.Application.Contracts
{
    [DependsOn(
        typeof(YiAbpDomainSharedModule),
        typeof(YiModuleRbacApplicationContractsModule),
        typeof(AbpSettingManagementApplicationContractsModule),
        typeof(YiModuleTenantManagementApplicationContractsModule),
        typeof(YiFrameworkDddApplicationContractsModule),
        typeof(YiModuleFileManagementApplicationContractsModule))]
    public class YiAbpApplicationContractsModule:AbpModule
    {

    }
}