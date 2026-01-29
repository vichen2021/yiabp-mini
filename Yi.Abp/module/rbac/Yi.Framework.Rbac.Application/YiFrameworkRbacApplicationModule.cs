using Lazy.Captcha.Core.Generator;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.FileSystem;
using Yi.Framework.Ddd.Application;
using Yi.Framework.Rbac.Application.Contracts;
using Yi.Framework.Rbac.Domain;
using Yi.Framework.Rbac.Domain.File;

namespace Yi.Framework.Rbac.Application
{
    [DependsOn(
        typeof(YiFrameworkRbacApplicationContractsModule),
        typeof(YiFrameworkRbacDomainModule),


        typeof(YiFrameworkDddApplicationModule),
        typeof(AbpBlobStoringFileSystemModule))]
    public class YiFrameworkRbacApplicationModule : AbpModule
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
