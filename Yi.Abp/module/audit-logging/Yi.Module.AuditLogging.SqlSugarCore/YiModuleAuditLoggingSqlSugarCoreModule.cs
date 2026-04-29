using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Yi.Module.AuditLogging.Domain;
using Yi.Module.AuditLogging.Domain.Repositories;
using Yi.Module.AuditLogging.SqlSugarCore.Repositories;
using Yi.Framework.SqlSugarCore;

namespace Yi.Module.AuditLogging.SqlSugarCore
{
    [DependsOn(
        typeof(YiModuleAuditLoggingDomainModule),
        typeof(YiFrameworkSqlSugarCoreModule))]
    public class YiModuleAuditLoggingSqlSugarCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IAuditLogRepository, SqlSugarCoreAuditLogRepository>();
            context.Services.AddTransient<IOperationLogRepository, SqlSugarCoreOperationLogRepository>();
        }
    }
}
