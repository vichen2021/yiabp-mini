using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.Operation.Abstractions;
using Yi.Module.AuditLogging.Domain.Shared;

namespace Yi.Module.AuditLogging.Application.Contracts
{
    [DependsOn(
        typeof(YiFrameworkDddApplicationContractsModule),
        typeof(YiFrameworkOperationAbstractionsModule),
        typeof(YiModuleAuditLoggingDomainSharedModule))]
    public class YiModuleAuditLoggingApplicationContractsModule : AbpModule
    {
    }
}