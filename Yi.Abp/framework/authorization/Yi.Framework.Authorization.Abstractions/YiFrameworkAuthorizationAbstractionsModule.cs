using Volo.Abp.Modularity;
using Yi.Framework.ActionMetadata.Abstractions;

namespace Yi.Framework.Authorization.Abstractions
{
    [DependsOn(typeof(YiFrameworkActionMetadataAbstractionsModule))]
    public class YiFrameworkAuthorizationAbstractionsModule : AbpModule
    {
    }
}
