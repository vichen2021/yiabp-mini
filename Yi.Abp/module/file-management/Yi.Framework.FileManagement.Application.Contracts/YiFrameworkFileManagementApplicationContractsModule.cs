using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.FileManagement.Domain.Shared;

namespace Yi.Framework.FileManagement.Application.Contracts
{
    [DependsOn(typeof(YiFrameworkFileManagementDomainSharedModule),
        typeof(YiFrameworkDddApplicationContractsModule))]
    public class YiFrameworkFileManagementApplicationContractsModule : AbpModule
    {

    }
}
