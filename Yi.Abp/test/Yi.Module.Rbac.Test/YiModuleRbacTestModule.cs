using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Auditing;
using Volo.Abp.Autofac;
using Volo.Abp.BackgroundWorkers;
using Yi.Module.Rbac.Application;
using Yi.Framework.Authorization.Abstractions.Options;
using Yi.Framework.Authorization.Core;
using Yi.Module.AuditLogging.Application;
using Yi.Module.AuditLogging.SqlSugarCore;
using Yi.Module.FileManagement.Application;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Managers;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Yi.Module.Rbac.SqlSugarCore;
using Yi.Module.TenantManagement.Application;

namespace Yi.Module.Rbac.Test
{
    [DependsOn(
        typeof(YiFrameworkAuthorizationCoreModule),
        typeof(YiModuleAuditLoggingApplicationModule),
        typeof(YiModuleAuditLoggingSqlSugarCoreModule),
        typeof(YiModuleRbacApplicationModule),
        typeof(YiModuleRbacSqlSugarCoreModule),

        typeof(AbpAutofacModule)
        )]
    public class YiModuleRbacTestModule : AbpModule
    {
        /// <summary>
        /// 配置测试宿主
        /// </summary>
        public override void ConfigureServices(ServiceConfigurationContext context)
        {

            // Configure<AbpBackgroundWorkerQuartzOptions>(options =>
            // {
            //     options.IsAutoRegisterEnabled = false;
            // });
            Configure<AbpBackgroundWorkerOptions> (options =>
            {
                options.IsEnabled = false; //禁用作业执行
            });

            // RBAC 菜单种子包含这些基础模块的菜单，测试宿主只登记程序集以完成权限码一致性校验
            Configure<PermissionDefinitionOptions>(options =>
            {
                options.AddApplicationAssembly(typeof(YiModuleFileManagementApplicationModule).Assembly);
                options.AddApplicationAssembly(typeof(YiModuleTenantManagementApplicationModule).Assembly);
            });
        }

    }
}
