using Volo.Abp.Domain;
using Volo.Abp.BlobStoring;
using Volo.Abp.Imaging;
using Volo.Abp.Modularity;
using Yi.Module.FileManagement.Domain.Shared;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.FileManagement.Domain
{
    [DependsOn(typeof(YiModuleFileManagementDomainSharedModule),
        typeof(AbpDddDomainModule),
        typeof(AbpBlobStoringModule),
        typeof(AbpImagingImageSharpModule),
        typeof(YiFrameworkSqlSugarCoreAbstractionsModule))]
    public class YiModuleFileManagementDomainModule : AbpModule
    {

    }
}
