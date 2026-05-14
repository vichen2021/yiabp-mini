using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Yi.Module.FileManagement.Domain.Shared
{
    [DependsOn(typeof(AbpDddDomainSharedModule))]
    public class YiModuleFileManagementDomainSharedModule : AbpModule
    {

    }
}
