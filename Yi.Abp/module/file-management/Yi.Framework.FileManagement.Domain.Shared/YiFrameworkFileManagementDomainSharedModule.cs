using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Yi.Framework.FileManagement.Domain.Shared
{
    [DependsOn(typeof(AbpDddDomainSharedModule))]
    public class YiFrameworkFileManagementDomainSharedModule : AbpModule
    {

    }
}
