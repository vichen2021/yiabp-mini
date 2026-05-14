using Yi.Framework.SqlSugarCore;
using Volo.Abp.Modularity;
using Yi.Module.FileManagement.Domain;

namespace Yi.Module.FileManagement.SqlSugarCore
{
    [DependsOn(
        typeof(YiModuleFileManagementDomainModule),
        typeof(YiFrameworkSqlSugarCoreModule))]
    public class YiModuleFileManagementSqlSugarCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddYiDbContext<YiFileManagementDbContext>();
        }
    }
}
