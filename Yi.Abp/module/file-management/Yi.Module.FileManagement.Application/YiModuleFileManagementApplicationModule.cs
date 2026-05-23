using Volo.Abp.BlobStoring.Aliyun;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application;
using Yi.Module.FileManagement.Application.Contracts;
using Yi.Module.FileManagement.Domain;
using Yi.Module.SettingManagement.Domain.Shared;

namespace Yi.Module.FileManagement.Application
{
    [DependsOn(typeof(YiModuleFileManagementApplicationContractsModule),
        typeof(YiModuleFileManagementDomainModule),
        typeof(YiFrameworkDddApplicationModule),
        typeof(AbpBlobStoringFileSystemModule),
        typeof(AbpBlobStoringAliyunModule),
        typeof(YiModuleSettingManagementDomainSharedModule))]
    public class YiModuleFileManagementApplicationModule : AbpModule
    {
    }
}
