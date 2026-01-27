using Volo.Abp.BlobStoring;
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Yi.Framework.FileManagement.Domain.Shared;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.FileManagement.Domain
{
    [DependsOn(typeof(YiFrameworkFileManagementDomainSharedModule),
        typeof(AbpDddDomainModule),
        typeof(AbpBlobStoringModule),
        typeof(YiFrameworkSqlSugarCoreAbstractionsModule))]
    public class YiFrameworkFileManagementDomainModule : AbpModule
    {

    }
}
