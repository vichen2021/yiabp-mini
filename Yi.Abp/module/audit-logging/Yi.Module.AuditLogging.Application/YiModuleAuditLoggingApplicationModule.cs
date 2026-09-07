using Volo.Abp.Modularity;
using Yi.Framework.Authorization.Abstractions.Options;
using Yi.Module.AuditLogging.Application.Contracts;
using Yi.Module.AuditLogging.Domain;
using Yi.Framework.Ddd.Application;

namespace Yi.Module.AuditLogging.Application
{
    [DependsOn(
        typeof(YiFrameworkDddApplicationModule),
        typeof(YiModuleAuditLoggingApplicationContractsModule),
        typeof(YiModuleAuditLoggingDomainModule))]
    public class YiModuleAuditLoggingApplicationModule : AbpModule
    {
        /// <summary>
        /// 登记审计应用服务程序集供权限定义扫描
        /// </summary>
        /// <param name="context">服务配置上下文</param>
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<PermissionDefinitionOptions>(options =>
            {
                options.AddApplicationAssembly(typeof(YiModuleAuditLoggingApplicationModule).Assembly);
            });
        }
    }
}
