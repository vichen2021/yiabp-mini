using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Volo.Abp.Settings;
using Yi.Module.FileManagement.Domain.Shared.Settings;

namespace Yi.Module.FileManagement.Domain.Shared
{
    [DependsOn(typeof(AbpDddDomainSharedModule),
        typeof(AbpSettingsModule))]
    public class YiModuleFileManagementDomainSharedModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<AbpSettingOptions>(options =>
            {
                options.DefinitionProviders.Add<FileManagementSettingDefinitionProvider>();
            });
        }
    }
}
