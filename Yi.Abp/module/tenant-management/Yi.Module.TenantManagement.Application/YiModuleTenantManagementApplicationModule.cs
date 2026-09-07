using Volo.Abp.Modularity;
using Yi.Framework.Authorization.Abstractions.Options;
using Yi.Framework.Ddd.Application;
using Yi.Module.TenantManagement.Application.Contracts;
using Yi.Module.TenantManagement.Domain;

namespace Yi.Module.TenantManagement.Application
{
    [DependsOn(
        typeof(YiModuleTenantManagementDomainModule),
        typeof(YiModuleTenantManagementApplicationContractsModule))]
    public class YiModuleTenantManagementApplicationModule : AbpModule
    {
        /// <summary>
        /// 登记租户管理应用服务程序集供权限定义扫描
        /// </summary>
        /// <param name="context">服务配置上下文</param>
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<PermissionDefinitionOptions>(options =>
            {
                options.AddApplicationAssembly(typeof(YiModuleTenantManagementApplicationModule).Assembly);
            });
        }
    }
}
