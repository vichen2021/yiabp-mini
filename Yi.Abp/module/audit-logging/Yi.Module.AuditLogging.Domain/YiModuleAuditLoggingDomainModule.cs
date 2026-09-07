using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Auditing;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Yi.Module.AuditLogging.Domain.Shared;
using Yi.Framework.Auditing.Abstractions;
using Yi.Framework.SqlSugarCore;

namespace Yi.Module.AuditLogging.Domain
{
    [DependsOn(
        typeof(YiModuleAuditLoggingDomainSharedModule),
        typeof(FrameworkAuditingAbstractionsModule),
        typeof(YiFrameworkSqlSugarCoreModule),
        typeof(AbpDddDomainModule),
        typeof(AbpAuditingModule))]
    public class YiModuleAuditLoggingDomainModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.Configure<YiAuditLoggingOptions>(options =>
            {
                options.SaveMode = AuditLogSaveMode.BusinessOperations;
            });
        }
    }
}
