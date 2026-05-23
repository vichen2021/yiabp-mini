using Volo.Abp.Autofac;
using Volo.Abp.Modularity;
using Yi.Abp.SqlsugarCore;

namespace Yi.Abp.DbMigrator;

[DependsOn(
    typeof(YiAbpSqlSugarCoreModule),
    typeof(AbpAutofacModule)
)]
public class DbMigratorModule : AbpModule
{
}
