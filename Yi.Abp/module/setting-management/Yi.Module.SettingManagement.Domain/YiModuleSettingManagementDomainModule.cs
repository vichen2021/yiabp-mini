using Volo.Abp.Caching;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.Abp.SettingManagement;
using Volo.Abp.Settings;
using Yi.Module.SettingManagement.Domain.Shared;

namespace Yi.Module.SettingManagement.Domain
{
    [DependsOn(
        typeof(AbpSettingsModule),
        typeof(AbpDddDomainModule),
        typeof(AbpSettingManagementDomainSharedModule),
        typeof(AbpCachingModule),
        typeof(YiModuleSettingManagementDomainSharedModule)
        )]
    public class YiModuleSettingManagementDomainModule : AbpModule
    {
        /// <summary>
        /// 配置 <see cref="SettingManagementOptions"/>，按优先级从低到高注册 Provider。
        /// </summary>
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<SettingManagementOptions>(options =>
            {
                options.Providers.Add<DefaultValueSettingManagementProvider>();
                options.Providers.Add<ConfigurationSettingManagementProvider>();
                options.Providers.Add<GlobalSettingManagementProvider>();
                options.Providers.Add<TenantSettingManagementProvider>();
                options.Providers.Add<UserSettingManagementProvider>();
            });
        }
    }

}
