using Volo.Abp.Modularity;
using Yi.Module.Rbac.Domain;
using Yi.Module.TenantManagement.Domain;

namespace Yi.Module.TenantManagement.SqlSugarCore
{
    [DependsOn(
        typeof(YiModuleTenantManagementDomainModule),
        typeof(YiModuleRbacDomainModule)
    )]
    public class YiModuleTenantManagementSqlSugarCoreModule : AbpModule
    {
    }
}
