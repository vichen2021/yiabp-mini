using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.Aliyun;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Modularity;
using Yi.Framework.Ddd.Application;
using Yi.Module.FileManagement.Application.Contracts;
using Yi.Module.FileManagement.Domain;
using Yi.Module.FileManagement.Domain.File;

namespace Yi.Module.FileManagement.Application
{
    [DependsOn(typeof(YiModuleFileManagementApplicationContractsModule),
        typeof(YiModuleFileManagementDomainModule),
        typeof(YiFrameworkDddApplicationModule),
        typeof(AbpBlobStoringFileSystemModule),
        typeof(AbpBlobStoringAliyunModule))]
    public class YiModuleFileManagementApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            ConfigureFileBlobStoring(context);
        }

        private void ConfigureFileBlobStoring(ServiceConfigurationContext context)
        {
            var configuration = context.Services.GetConfiguration();
            var provider = configuration["BlobStoring:Provider"] ?? "FileSystem";

            Configure<AbpBlobStoringOptions>(options =>
            {
                options.Containers.Configure<FileManagementContainer>(container =>
                {
                    if (string.Equals(provider, "Aliyun", StringComparison.OrdinalIgnoreCase))
                    {
                        container.UseAliyun(aliyun =>
                        {
                            aliyun.AccessKeyId = configuration["BlobStoring:Aliyun:AccessKeyId"] ?? "";
                            aliyun.AccessKeySecret = configuration["BlobStoring:Aliyun:AccessKeySecret"] ?? "";
                            aliyun.Endpoint = configuration["BlobStoring:Aliyun:Endpoint"] ?? "";
                            aliyun.ContainerName = configuration["BlobStoring:Aliyun:ContainerName"] ?? "";
                            aliyun.CreateContainerIfNotExists = configuration.GetValue<bool>("BlobStoring:Aliyun:CreateContainerIfNotExists");
                        });
                    }
                    else
                    {
                        container.UseFileSystem(fileSystem =>
                        {
                            fileSystem.BasePath = configuration["BlobStoring:FileSystem:BasePath"] ?? "wwwroot/FileStorage";
                        });
                    }
                });
            });
        }
    }
}
