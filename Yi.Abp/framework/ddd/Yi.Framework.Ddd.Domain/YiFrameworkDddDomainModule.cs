using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Yi.Framework.Ddd.Domain;

/// <summary>
/// Framework DDD domain module.
/// </summary>
[DependsOn(typeof(AbpDddDomainModule))]
public class YiFrameworkDddDomainModule : AbpModule
{
}
