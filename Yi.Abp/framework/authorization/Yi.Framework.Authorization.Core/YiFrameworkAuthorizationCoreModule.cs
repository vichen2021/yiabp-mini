using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Yi.Framework.ActionMetadata.Core;
using Yi.Framework.Authorization.Abstractions;
using Yi.Framework.Authorization.Abstractions.Metadata;
using Yi.Framework.Authorization.Abstractions.Permissions;
using Yi.Framework.Authorization.Core.Filters;
using Yi.Framework.Authorization.Core.Metadata;
using Yi.Framework.Authorization.Core.Permissions;

namespace Yi.Framework.Authorization.Core
{
    [DependsOn(
        typeof(YiFrameworkAuthorizationAbstractionsModule),
        typeof(YiFrameworkActionMetadataCoreModule),
        typeof(AbpAspNetCoreMvcModule))]
    public class YiFrameworkAuthorizationCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IPermissionRequirementResolver, DefaultPermissionRequirementResolver>();
            context.Services.AddSingleton<IPermissionDefinitionProvider, DefaultPermissionDefinitionProvider>();
            context.Services.AddTransient<IPermissionDefinitionValidator, DefaultPermissionDefinitionValidator>();
            context.Services.AddTransient<IPermissionHandler, DefaultPermissionHandler>();
            context.Services.AddTransient<PermissionAuthorizationFilter>();
        }
    }
}
