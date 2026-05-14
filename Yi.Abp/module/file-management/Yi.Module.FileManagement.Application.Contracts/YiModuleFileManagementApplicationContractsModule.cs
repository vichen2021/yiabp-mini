using Volo.Abp.Modularity;
using Yi.Module.FileManagement.Domain.Shared;
using Yi.Framework.Ddd.Application.Contracts;

namespace Yi.Module.FileManagement.Application.Contracts
{
    [DependsOn(typeof(YiModuleFileManagementDomainSharedModule),
        typeof(YiFrameworkDddApplicationContractsModule))]
    public class YiModuleFileManagementApplicationContractsModule : AbpModule
    {

    }
}
