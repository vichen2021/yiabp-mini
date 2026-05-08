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
            // ActionIdentity 解析服务
            context.Services.AddSingleton<ActionIdentityCache>();
            context.Services.AddTransient<IActionIdentityResolver, DefaultActionIdentityResolver>();

            // 权限和日志决策
            context.Services.AddTransient<IPermissionRequirementResolver, DefaultPermissionRequirementResolver>();
            context.Services.AddTransient<IOperationLogRequirementResolver, DefaultOperationLogRequirementResolver>();

            // 权限
            context.Services.AddTransient<IPermissionHandler, DefaultPermissionHandler>();

            // 注册全局 Filter
            context.Services.AddTransient<PermissionAuthorizationFilter>();
        }
    }
}
