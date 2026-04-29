using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application;
using Yi.Module.TenantManagement.Domain;

namespace Yi.Module.TenantManagement.Application
{
    [DependsOn(typeof(YiModuleTenantManagementDomainModule))]
    public class YiModuleTenantManagementApplicationModule: AbpModule
    {

    }
}
