using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Yi.Framework.Security.Abstractions;
using Yi.Framework.Security.Abstractions.Logging;
using Yi.Framework.Security.Abstractions.Metadata;
using Yi.Framework.Security.Abstractions.Permissions;
using Yi.Framework.Security.Core.Metadata;
using Yi.Framework.Security.Core.Filters;
using Yi.Framework.Security.Core.Permissions;
using Yi.Framework.Security.Core.Logging;

namespace Yi.Framework.Security.Core
{
    [DependsOn(
        typeof(YiFrameworkSecurityAbstractionsModule),
        typeof(AbpAspNetCoreMvcModule))]
    public class YiFrameworkSecurityCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // 元数据解析
            context.Services.AddSingleton<ActionMetadataCache>();
            context.Services.AddTransient<IActionMetadataResolver, DefaultActionMetadataResolver>();

            // 权限
            context.Services.AddTransient<IPermissionHandler, DefaultPermissionHandler>();
            context.Services.Configure<PermissionOptions>(options =>
            {
                options.DefaultDenyUnresolved = true;
                options.AllowUnresolvedActions = false;
            });

            // 日志
            context.Services.AddTransient<IOperLogStore, DefaultOperLogStore>();
            context.Services.Configure<OperLogOptions>(options =>
            {
                options.IsEnabled = true;
                options.LogReadOperations = false;
                options.SaveRequestData = true;
                options.SaveResponseData = false;
            });

            // 注册全局 Filter
            context.Services.AddTransient<PermissionAuthorizationFilter>();
            context.Services.AddTransient<OperLogActionFilter>();
        }
    }
}