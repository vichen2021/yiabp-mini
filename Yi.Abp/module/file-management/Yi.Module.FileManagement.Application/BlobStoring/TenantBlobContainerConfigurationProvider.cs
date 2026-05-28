using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.Aliyun;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.DependencyInjection;

namespace Yi.Module.FileManagement.Application.BlobStoring;

/// <summary>
/// 运行时租户级 BlobContainer 配置提供者，替换 ABP 默认的静态 <see cref="DefaultBlobContainerConfigurationProvider"/>。
/// <para>
/// 每次请求调用 <see cref="Get"/> 时，按 ISettingProvider 的 fallback 链（Tenant → Global → Configuration → Default）
/// 动态解析当前租户的 OSS 配置，实现不同租户使用不同 Bucket / 凭证的隔离。
/// </para>
/// <para>
/// 若租户未配置任何 Setting，自动回退到 appsettings.json 中的 BlobStoring 配置。
/// </para>
/// </summary>
[Dependency(ReplaceServices = true)]
public class TenantBlobContainerConfigurationProvider
    : IBlobContainerConfigurationProvider, ITransientDependency
{
    private readonly FileStorageOptionsResolver _optionsResolver;

    public TenantBlobContainerConfigurationProvider(FileStorageOptionsResolver optionsResolver)
    {
        _optionsResolver = optionsResolver;
    }

    /// <summary>
    /// 获取指定容器名的 BlobContainer 配置。
    /// ISettingProvider 已内置 Tenant → Global → Configuration → Default 的优先级解析。
    /// 若出现死锁，可将 GetAwaiter().GetResult() 改为 Task.Run(...).GetAwaiter().GetResult()。
    /// </summary>
    public BlobContainerConfiguration Get(string name)
    {
        var provider = _optionsResolver.ResolveProvider();

        var config = new BlobContainerConfiguration();

        if (string.Equals(provider, "Aliyun", StringComparison.OrdinalIgnoreCase))
        {
            var aliyunOptions = _optionsResolver.ResolveAliyun();
            config.UseAliyun(aliyun =>
            {
                aliyun.AccessKeyId = aliyunOptions.AccessKeyId;
                aliyun.AccessKeySecret = aliyunOptions.AccessKeySecret;
                aliyun.Endpoint = aliyunOptions.Endpoint;
                aliyun.ContainerName = aliyunOptions.ContainerName;
                aliyun.CreateContainerIfNotExists = aliyunOptions.CreateContainerIfNotExists;
            });
        }
        else
        {
            config.UseFileSystem(fs =>
            {
                fs.BasePath = _optionsResolver.ResolveFileSystemBasePath();
            });
        }

        return config;
    }
}
