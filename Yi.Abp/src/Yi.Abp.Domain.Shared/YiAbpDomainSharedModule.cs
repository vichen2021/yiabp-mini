using Volo.Abp.Domain;
using Volo.Abp.SettingManagement;
using Yi.Module.AuditLogging.Domain.Shared;
using Yi.Module.FileManagement.Domain.Shared;
using Yi.Module.Rbac.Domain.Shared;

namespace Yi.Abp.Domain.Shared
{
    [DependsOn(
        typeof(YiModuleRbacDomainSharedModule),
        typeof(YiModuleAuditLoggingDomainSharedModule),
        typeof(AbpSettingManagementDomainSharedModule),
        typeof(AbpDddDomainSharedModule),
        typeof(YiModuleFileManagementDomainSharedModule))]
    public class YiAbpDomainSharedModule : AbpModule
    {

    }
}