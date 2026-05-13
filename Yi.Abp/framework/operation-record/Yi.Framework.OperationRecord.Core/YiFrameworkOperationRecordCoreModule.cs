using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Modularity;
using Yi.Framework.ActionMetadata.Core;
using Yi.Framework.OperationRecord.Abstractions;
using Yi.Framework.OperationRecord.Abstractions.Metadata;
using Yi.Framework.OperationRecord.Core.Metadata;

namespace Yi.Framework.OperationRecord.Core
{
    [DependsOn(
        typeof(YiFrameworkOperationRecordAbstractionsModule),
        typeof(YiFrameworkActionMetadataCoreModule))]
    public class YiFrameworkOperationRecordCoreModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            context.Services.AddTransient<IOperationLogRequirementResolver, DefaultOperationLogRequirementResolver>();
        }
    }
}
