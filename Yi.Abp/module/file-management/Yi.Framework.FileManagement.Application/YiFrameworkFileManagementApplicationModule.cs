using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application;
using Yi.Framework.FileManagement.Application.Contracts;
using Yi.Framework.FileManagement.Domain;
using Yi.Framework.FileManagement.Files;

namespace Yi.Framework.FileManagement.Application
{
    [DependsOn(
        typeof(YiFrameworkFileManagementApplicationContractsModule),
        typeof(YiFrameworkFileManagementDomainModule),
        typeof(YiFrameworkDddApplicationModule),
        typeof(AbpBlobStoringFileSystemModule))]
    public class YiFrameworkFileManagementApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpBlobStoringOptions>(options =>
            {
                options.Containers.Configure<YiFileManagementContainer>(container =>
                {
                    container.UseFileSystem(fileSystem =>
                    {
                        fileSystem.BasePath = "App_Data/FileStorage";
                    });
                });
            });
        }
    }
}
