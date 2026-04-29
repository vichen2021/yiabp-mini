using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Auditing;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Yi.Module.AuditLogging.Domain.Shared;
using Yi.Framework.Operation.Abstractions;
using Yi.Framework.SqlSugarCore;

namespace Yi.Module.AuditLogging.Domain
{
    [DependsOn(
        typeof(YiModuleAuditLoggingDomainSharedModule),
        typeof(YiFrameworkOperationAbstractionsModule),
        typeof(YiFrameworkSqlSugarCoreModule),
        typeof(AbpDddDomainModule),
        typeof(AbpAuditingModule))]
    public class YiModuleAuditLoggingDomainModule : AbpModule
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
