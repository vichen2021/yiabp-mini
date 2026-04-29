using Volo.Abp.Modularity;
using Yi.Module.TenantManagement.Domain;

namespace Yi.Module.TenantManagement.SqlSugarCore
{
    [DependsOn(typeof(YiModuleTenantManagementDomainModule))]
    public class YiModuleTenantManagementSqlSugarCoreModule : AbpModule
    {
    }
}
