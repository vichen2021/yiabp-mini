using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Authorization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Modularity;
using Yi.Framework.Authorization.Abstractions;
using Yi.Framework.Authorization.Abstractions.Metadata;
using Yi.Framework.Authorization.Core.Interceptors;
using Yi.Framework.Authorization.Core.Metadata;
using Yi.Framework.Authorization.Core.Permissions;

namespace Yi.Framework.Authorization.Core
{
    [DependsOn(
        typeof(YiFrameworkAuthorizationAbstractionsModule),
        typeof(AbpAuthorizationModule))]
    public class YiFrameworkAuthorizationCoreModule : AbpModule
    {
        /// <summary>
        /// 在常规依赖注入扫描前登记 Yi 权限拦截器
        /// </summary>
        public override void PreConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.OnRegistered(ApplicationServicePermissionAuthorizationInterceptorRegistrar.RegisterIfNeeded);
        }

        /// <summary>
        /// 注册权限解析器和 ABP 权限定义适配器
        /// </summary>
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IPermissionRequirementResolver, DefaultPermissionRequirementResolver>();
            context.Services.AddTransient<AttributePermissionDefinitionProvider>();

            Configure<AbpPermissionOptions>(options =>
            {
                options.DefinitionProviders.AddIfNotContains(typeof(AttributePermissionDefinitionProvider));
            });
        }
    }
}
