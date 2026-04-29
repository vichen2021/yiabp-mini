using Volo.Abp.Modularity;
using Volo.Abp.TenantManagement;
using Yi.Framework.Ddd.Application.Contracts;

namespace Yi.Module.TenantManagement.Application.Contracts
{
    [DependsOn(typeof(AbpTenantManagementDomainSharedModule),
        typeof(YiFrameworkDddApplicationContractsModule))]
    public class YiModuleTenantManagementApplicationContractsModule:AbpModule
    {

    }
}
