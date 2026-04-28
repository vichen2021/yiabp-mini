using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.Operation.Abstractions;
using Yi.Framework.AuditLogging.Domain.Shared;

namespace Yi.Framework.AuditLogging.Application.Contracts
{
    [DependsOn(
        typeof(YiFrameworkDddApplicationContractsModule),
        typeof(YiFrameworkOperationAbstractionsModule),
        typeof(YiFrameworkAuditLoggingDomainSharedModule))]
    public class YiFrameworkAuditLoggingApplicationContractsModule : AbpModule
    {
    }
}