using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Yi.Abp.Domain;
using Yi.Abp.SqlSugarCore;
using Yi.Module.AuditLogging.SqlSugarCore;
using Yi.Framework.Mapster;
using Yi.Module.FileManagement.SqlSugarCore;
using Yi.Module.Rbac.SqlSugarCore;
using Yi.Module.SettingManagement.SqlSugarCore;
using Yi.Framework.SqlSugarCore;
using Yi.Module.TenantManagement.SqlSugarCore;

namespace Yi.Abp.SqlsugarCore
{
    [DependsOn(
        typeof(YiAbpDomainModule),
        typeof(YiModuleRbacSqlSugarCoreModule),
        typeof(YiModuleSettingManagementSqlSugarCoreModule),
        typeof(YiModuleAuditLoggingSqlSugarCoreModule),
        typeof(YiModuleTenantManagementSqlSugarCoreModule),
        typeof(YiFrameworkMapsterModule),
        typeof(YiFrameworkSqlSugarCoreModule),
        typeof(YiModuleFileManagementSqlSugarCoreModule))]
    public class YiAbpSqlSugarCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddYiDbContext<YiDbContext>();
            //默认不开放，可根据项目需要是否Db直接对外开放
            //context.Services.AddTransient(x => x.GetRequiredService<ISqlSugarDbContext>().SqlSugarClient);
        }
    }
}