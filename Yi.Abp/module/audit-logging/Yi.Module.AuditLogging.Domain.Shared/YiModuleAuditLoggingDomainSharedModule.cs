using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Yi.Module.AuditLogging.Domain.Shared
{
    [DependsOn(typeof(AbpDddDomainSharedModule))]
    public class YiModuleAuditLoggingDomainSharedModule:AbpModule
    {

    }
}
