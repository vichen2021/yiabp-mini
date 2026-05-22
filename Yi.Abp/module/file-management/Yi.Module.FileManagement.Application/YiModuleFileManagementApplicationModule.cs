using Volo.Abp.BlobStoring.Aliyun;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application;
using Yi.Module.FileManagement.Application.Contracts;
using Yi.Module.FileManagement.Domain;

namespace Yi.Module.FileManagement.Application
{
    [DependsOn(typeof(YiModuleFileManagementApplicationContractsModule),
        typeof(YiModuleFileManagementDomainModule),
        typeof(YiFrameworkDddApplicationModule),
        typeof(AbpBlobStoringFileSystemModule),
        typeof(AbpBlobStoringAliyunModule))]
    public class YiModuleFileManagementApplicationModule : AbpModule
    {
    }
}
