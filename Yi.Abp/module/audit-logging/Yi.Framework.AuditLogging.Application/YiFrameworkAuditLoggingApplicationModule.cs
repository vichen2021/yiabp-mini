using Volo.Abp.AutoMapper;
using Volo.Abp.Modularity;
using Yi.Framework.AuditLogging.Application.Contracts;
using Yi.Framework.AuditLogging.Domain;
using Yi.Framework.Ddd.Application;

namespace Yi.Framework.AuditLogging.Application
{
    [DependsOn(
        typeof(YiFrameworkDddApplicationModule),
        typeof(YiFrameworkAuditLoggingApplicationContractsModule),
        typeof(YiFrameworkAuditLoggingDomainModule))]
    public class YiFrameworkAuditLoggingApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpAutoMapperOptions>(options =>
            {
                options.AddMaps<YiFrameworkAuditLoggingApplicationModule>();
            });
        }
    }
}