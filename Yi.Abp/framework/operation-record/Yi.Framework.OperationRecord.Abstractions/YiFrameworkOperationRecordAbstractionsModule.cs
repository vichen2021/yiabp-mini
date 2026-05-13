using Volo.Abp.Modularity;
using Yi.Framework.ActionMetadata.Abstractions;

namespace Yi.Framework.OperationRecord.Abstractions
{
    [DependsOn(typeof(YiFrameworkActionMetadataAbstractionsModule))]
    public class YiFrameworkOperationRecordAbstractionsModule : AbpModule
    {
    }
}
