using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.Auditing.Abstractions;
using Yi.Module.AuditLogging.Domain.Shared;

namespace Yi.Module.AuditLogging.Application.Contracts
{
    [DependsOn(
        typeof(YiFrameworkDddApplicationContractsModule),
        typeof(FrameworkAuditingAbstractionsModule),
        typeof(YiModuleAuditLoggingDomainSharedModule))]
    public class YiModuleAuditLoggingApplicationContractsModule : AbpModule
    {
    }
}
