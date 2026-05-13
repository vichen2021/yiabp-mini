using Volo.Abp.Modularity;
using Yi.Framework.Mapster;
using Yi.Module.Rbac.Domain;
using Yi.Framework.SqlSugarCore;

namespace Yi.Module.Rbac.SqlSugarCore
{
    [DependsOn(
        typeof(YiModuleRbacDomainModule),

        typeof(YiFrameworkMapsterModule),
        typeof(YiFrameworkSqlSugarCoreModule)
        )]
    public class YiModuleRbacSqlSugarCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddYiDbContext<YiRbacDbContext>();
        }
    }
}
