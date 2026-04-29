using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Module.Rbac.Domain.Shared;

namespace Yi.Module.Rbac.Application.Contracts
{
    [DependsOn(
        typeof(YiModuleRbacDomainSharedModule),


        typeof(YiFrameworkDddApplicationContractsModule))]
    public class YiModuleRbacApplicationContractsModule : AbpModule
    {

    }
}