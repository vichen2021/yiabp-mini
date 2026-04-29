using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
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
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<YiModuleAuditLoggingApplicationModule>();
            });
        }
    }
}