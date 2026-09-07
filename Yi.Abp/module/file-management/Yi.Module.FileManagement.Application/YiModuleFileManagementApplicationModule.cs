using Volo.Abp.BlobStoring.Aliyun;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.Modularity;
using Yi.Framework.Authorization.Abstractions.Options;
using Yi.Framework.Ddd.Application;
using Yi.Module.FileManagement.Application.Contracts;
using Yi.Module.FileManagement.Domain;
using Yi.Module.SettingManagement.Domain.Shared;

namespace Yi.Module.FileManagement.Application
{
    [DependsOn(typeof(YiModuleFileManagementApplicationContractsModule),
        typeof(YiModuleFileManagementDomainModule),
        typeof(YiFrameworkDddApplicationModule),
        typeof(AbpBlobStoringFileSystemModule),
        typeof(AbpBlobStoringAliyunModule),
        typeof(YiModuleSettingManagementDomainSharedModule))]
    public class YiModuleFileManagementApplicationModule : AbpModule
    {
        /// <summary>
        /// 登记文件管理应用服务程序集供权限定义扫描
        /// </summary>
        /// <param name="context">服务配置上下文</param>
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            Configure<PermissionDefinitionOptions>(options =>
            {
                options.AddApplicationAssembly(typeof(YiModuleFileManagementApplicationModule).Assembly);
            });
        }
    }
}
