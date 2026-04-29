using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement.EntityFrameworkCore;
using Yi.Module.SettingManagement.Domain;
using Yi.Framework.SqlSugarCore;

namespace Yi.Module.SettingManagement.SqlSugarCore
{
    [DependsOn(
        typeof(YiModuleSettingManagementDomainModule),
        typeof(YiFrameworkSqlSugarCoreModule)
        )]
    public class YiModuleSettingManagementSqlSugarCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var services = context.Services;
            services.AddTransient<ISettingRepository, SqlSugarCoreSettingRepository>();
        }
    }
}
