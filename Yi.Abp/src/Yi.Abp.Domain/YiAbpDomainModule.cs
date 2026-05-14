using Volo.Abp.Caching;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Yi.Abp.Domain.Shared;
using Yi.Module.AuditLogging.Domain;
using Yi.Framework.Mapster;
using Yi.Module.FileManagement.Domain;
using Yi.Module.Rbac.Domain;
using Yi.Module.SettingManagement.Domain;
using Yi.Module.TenantManagement.Domain;

namespace Yi.Abp.Domain
{
    [DependsOn(
        typeof(YiAbpDomainSharedModule),
        typeof(YiModuleTenantManagementDomainModule),
        typeof(YiModuleRbacDomainModule),
        typeof(YiModuleAuditLoggingDomainModule),
        typeof(YiModuleSettingManagementDomainModule),
        typeof(YiFrameworkMapsterModule),
        typeof(AbpDddDomainModule),
        typeof(AbpCachingModule),
        typeof(YiModuleFileManagementDomainModule))]
    public class YiAbpDomainModule : AbpModule
    {

    }
}