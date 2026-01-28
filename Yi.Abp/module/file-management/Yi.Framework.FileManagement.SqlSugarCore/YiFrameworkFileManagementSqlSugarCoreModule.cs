using Volo.Abp.Modularity;
using Yi.Framework.FileManagement.Domain;
using Yi.Framework.SqlSugarCore;

namespace Yi.Framework.FileManagement.SqlSugarCore
{
    [DependsOn(typeof(YiFrameworkFileManagementDomainModule), typeof(YiFrameworkSqlSugarCoreModule))]
    public class YiFrameworkFileManagementSqlSugarCoreModule : AbpModule
    {
    }
}
