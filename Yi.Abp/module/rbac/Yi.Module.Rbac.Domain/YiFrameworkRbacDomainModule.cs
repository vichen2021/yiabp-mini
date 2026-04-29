using Medallion.Threading;
using Medallion.Threading.Redis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using Volo.Abp.AspNetCore.SignalR;
using Volo.Abp.BlobStoring;
using Volo.Abp.Caching;
using Volo.Abp.DistributedLocking;
using Volo.Abp.Domain;
using Volo.Abp.Imaging;
using Volo.Abp.Modularity;
using Yi.Framework.Caching.FreeRedis;
using Yi.Module.Rbac.Domain.Shared;
using Yi.Module.Rbac.Domain.Shared.Options;
using Yi.Framework.Operation.Abstractions;

namespace Yi.Module.Rbac.Domain
{
    [DependsOn(
        typeof(YiModuleRbacDomainSharedModule),
        typeof(YiFrameworkCachingFreeRedisModule),
        typeof(YiFrameworkOperationAbstractionsModule),
        typeof(AbpAspNetCoreSignalRModule),
        typeof(AbpDddDomainModule),
        typeof(AbpCachingModule),
        typeof(AbpImagingImageSharpModule),
        typeof(AbpDistributedLockingModule),
        typeof(AbpBlobStoringModule)
        )]
    public class YiModuleRbacDomainModule : AbpModule
    {
        public override void ConfigureServices(ServiceConfigurationContext context)
        {
            var service = context.Services;
            var configuration = context.Services.GetConfiguration();

            // Filter 注册已迁移到 Yi.Abp.Web，此处移除

            //配置阿里云短信
            Configure<AliyunOptions>(configuration.GetSection(nameof(AliyunOptions)));

            //分布式锁,需要redis
            if (configuration.GetSection("Redis").GetValue<bool>("IsEnabled"))
            {
                context.Services.AddSingleton<IDistributedLockProvider>(sp =>
                {
                    var connection = ConnectionMultiplexer
                        .Connect(configuration["Redis:Configuration"]);
                    return new
                        RedisDistributedSynchronizationProvider(connection.GetDatabase());
                });
            }
        }
    }
}