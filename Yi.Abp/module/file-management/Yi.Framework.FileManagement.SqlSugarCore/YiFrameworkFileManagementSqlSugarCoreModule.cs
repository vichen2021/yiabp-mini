using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Yi.Framework.FileManagement.Domain;
using Yi.Framework.FileManagement.Files;
using Yi.Framework.FileManagement.SqlSugarCore.Repositories;
using Yi.Framework.SqlSugarCore;

namespace Yi.Framework.FileManagement.SqlSugarCore
{
    [DependsOn(typeof(YiFrameworkFileManagementDomainModule), typeof(YiFrameworkSqlSugarCoreModule))]
    public class YiFrameworkFileManagementSqlSugarCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IFileRepository, FileRepository>();
        }
    }
}
