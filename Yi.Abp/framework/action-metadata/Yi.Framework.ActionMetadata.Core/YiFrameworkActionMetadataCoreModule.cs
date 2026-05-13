using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Yi.Framework.ActionMetadata.Abstractions;
using Yi.Framework.ActionMetadata.Abstractions.Metadata;
using Yi.Framework.ActionMetadata.Core.Metadata;

namespace Yi.Framework.ActionMetadata.Core
{
    [DependsOn(typeof(YiFrameworkActionMetadataAbstractionsModule))]
    public class YiFrameworkActionMetadataCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddSingleton<ActionIdentityCache>();
            context.Services.AddTransient<IActionIdentityResolver, DefaultActionIdentityResolver>();
        }
    }
}
