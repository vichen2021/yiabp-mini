using Lazy.Captcha.Core.Generator;
using Microsoft.Extensions.DependencyInjection;
using Yi.Framework.Authorization.Abstractions.Options;
using Yi.Framework.Ddd.Application;
using Yi.Module.Rbac.Application.Contracts;
using Yi.Module.Rbac.Domain;
using Yi.Module.TenantManagement.Application.Contracts;

namespace Yi.Module.Rbac.Application
{
    [DependsOn(
        typeof(YiModuleRbacApplicationContractsModule),
        typeof(YiModuleRbacDomainModule),
        typeof(YiModuleTenantManagementApplicationContractsModule),

        typeof(YiFrameworkDddApplicationModule))]
    public class YiModuleRbacApplicationModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var service = context.Services;

            Configure<PermissionDefinitionOptions>(options =>
            {
                options.AddApplicationAssembly(typeof(YiModuleRbacApplicationModule).Assembly);
            });

            service.AddCaptcha(options =>
            {
                options.CaptchaType = CaptchaType.ARITHMETIC;
                options.CodeLength = 1;
            });
        }

        public async override Task OnApplicationInitializationAsync(ApplicationInitializationContext context)
        {
        }
    }
}
