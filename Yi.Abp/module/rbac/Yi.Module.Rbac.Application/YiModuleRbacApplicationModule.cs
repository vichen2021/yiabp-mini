using Lazy.Captcha.Core.Generator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.BlobStoring.Aliyun;
using Yi.Framework.Ddd.Application;
using Yi.Module.Rbac.Application.Contracts;
using Yi.Module.Rbac.Domain;
using Yi.Module.Rbac.Domain.File;
using Yi.Module.TenantManagement.Application.Contracts;

namespace Yi.Module.Rbac.Application
{
    [DependsOn(
        typeof(YiModuleRbacApplicationContractsModule),
        typeof(YiModuleRbacDomainModule),
        typeof(YiModuleTenantManagementApplicationContractsModule),

        typeof(YiFrameworkDddApplicationModule),
        typeof(AbpBlobStoringFileSystemModule),
        typeof(AbpBlobStoringAliyunModule))]
    public class YiModuleRbacApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var service = context.Services;
            var configuration = service.GetConfiguration();

            service.AddCaptcha(options =>
            {
                options.CaptchaType = CaptchaType.ARITHMETIC;
                options.CodeLength = 1;
            });

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
                            aliyun.AccessKeyId = configuration["AliyunOptions:AccessKeyId"] ?? "";
                            aliyun.AccessKeySecret = configuration["AliyunOptions:AccessKeySecret"] ?? "";
                            aliyun.Endpoint = configuration["AliyunOptions:Oss:Endpoint"] ?? "";
                            aliyun.ContainerName = configuration["AliyunOptions:Oss:ContainerName"] ?? "";
                            aliyun.CreateContainerIfNotExists = configuration.GetValue<bool>("AliyunOptions:Oss:CreateContainerIfNotExists");
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

        public async override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
        }
    }
}