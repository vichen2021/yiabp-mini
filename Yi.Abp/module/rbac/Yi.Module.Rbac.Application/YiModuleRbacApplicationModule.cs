using Lazy.Captcha.Core.Generator;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
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
        typeof(AbpBlobStoringFileSystemModule))]
    public class YiModuleRbacApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var service = context.Services;

            service.AddCaptcha(options =>
            {
                options.CaptchaType = CaptchaType.ARITHMETIC;
                options.CodeLength = 1;
            });

            Configure<AbpBlobStoringOptions>(options =>
            {
                options.Containers.Configure<FileManagementContainer>(container =>
                {
                    container.UseFileSystem(fileSystem =>
                    {
                        fileSystem.BasePath = "wwwroot/FileStorage";
                    });
                });
            });
        }

        public async override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
        }
    }
}