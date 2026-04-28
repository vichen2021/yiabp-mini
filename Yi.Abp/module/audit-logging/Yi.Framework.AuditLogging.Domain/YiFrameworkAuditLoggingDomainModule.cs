using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Auditing;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Yi.Framework.AuditLogging.Domain.Shared;
using Yi.Framework.Operation.Abstractions;
using Yi.Framework.SqlSugarCore;

namespace Yi.Framework.AuditLogging.Domain
{
    [DependsOn(
        typeof(YiFrameworkAuditLoggingDomainSharedModule),
        typeof(YiFrameworkOperationAbstractionsModule),
        typeof(YiFrameworkSqlSugarCoreModule),
        typeof(AbpDddDomainModule),
        typeof(AbpAuditingModule))]
    public class YiFrameworkAuditLoggingDomainModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // 配置 Yi 审计日志选项
            context.Services.Configure<YiAuditLoggingOptions>(options =>
            {
                options.SaveAuditLog = false;
                options.SaveOperationLog = true;
            });

            // 注册操作日志映射器
            context.Services.AddTransient<IOperationLogMapper, DefaultOperationLogMapper>();
        }
    }
}
