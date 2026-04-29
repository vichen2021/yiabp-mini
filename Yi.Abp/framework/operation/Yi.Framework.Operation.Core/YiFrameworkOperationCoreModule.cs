using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.AspNetCore.Mvc;
using Volo.Abp.Modularity;
using Yi.Framework.Operation.Abstractions;
using Yi.Framework.Operation.Abstractions.Metadata;
using Yi.Framework.Operation.Abstractions.Permissions;
using Yi.Framework.Operation.Core.Metadata;
using Yi.Framework.Operation.Core.Filters;
using Yi.Framework.Operation.Core.Permissions;

namespace Yi.Framework.Operation.Core
{
    [DependsOn(
        typeof(YiFrameworkOperationAbstractionsModule),
        typeof(AbpAspNetCoreMvcModule))]
    public class YiFrameworkOperationCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            // 元数据解析
            context.Services.AddSingleton<ActionMetadataCache>();
            context.Services.AddTransient<IActionMetadataResolver, DefaultActionMetadataResolver>();
            context.Services.AddTransient<IPermissionCodeGenerator, DefaultPermissionCodeGenerator>();

            // 权限
            context.Services.AddTransient<IPermissionHandler, DefaultPermissionHandler>();
            context.Services.Configure<PermissionOptions>(options =>
            {
                options.AllowUnresolvedActions = true; // 默认兼容模式
                options.AutoCheckResolvedActions = true; // 自动检查已解析权限
            });

            // 注册全局 Filter
            context.Services.AddTransient<PermissionAuthorizationFilter>();
        }
    }
}