using Microsoft.Extensions.Configuration;
using Volo.Abp.BlobStoring;
using Volo.Abp.BlobStoring.Aliyun;
using Volo.Abp.BlobStoring.FileSystem;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Yi.Module.FileManagement.Domain.Shared.Settings;

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
    private readonly ISettingProvider _settingProvider;
    private readonly IConfiguration _configuration;

    public TenantBlobContainerConfigurationProvider(
        ISettingProvider settingProvider,
        IConfiguration configuration)
    {
        _settingProvider = settingProvider;
        _configuration = configuration;
    }

    /// <summary>
    /// 获取指定容器名的 BlobContainer 配置。
    /// ISettingProvider 已内置 Tenant → Global → Configuration → Default 的优先级解析。
    /// 若出现死锁，可将 GetAwaiter().GetResult() 改为 Task.Run(...).GetAwaiter().GetResult()。
    /// </summary>
    public BlobContainerConfiguration Get(string name)
    {
        var provider = _settingProvider
            .GetOrNullAsync(FileManagementSettingNames.Provider)
            .GetAwaiter().GetResult()
            ?? _configuration["BlobStoring:Provider"]
            ?? "FileSystem";

        var config = new BlobContainerConfiguration();

        if (string.Equals(provider, "Aliyun", StringComparison.OrdinalIgnoreCase))
        {
            config.UseAliyun(aliyun =>
            {
                aliyun.AccessKeyId = GetSetting(
                    FileManagementSettingNames.Aliyun.AccessKeyId,
                    _configuration["BlobStoring:Aliyun:AccessKeyId"]) ?? "";
                aliyun.AccessKeySecret = GetSetting(
                    FileManagementSettingNames.Aliyun.AccessKeySecret,
                    _configuration["BlobStoring:Aliyun:AccessKeySecret"]) ?? "";
                aliyun.Endpoint = GetSetting(
                    FileManagementSettingNames.Aliyun.Endpoint,
                    _configuration["BlobStoring:Aliyun:Endpoint"]) ?? "";
                aliyun.ContainerName = GetSetting(
                    FileManagementSettingNames.Aliyun.ContainerName,
                    _configuration["BlobStoring:Aliyun:ContainerName"]) ?? "";
                aliyun.CreateContainerIfNotExists = bool.Parse(
                    GetSetting(
                        FileManagementSettingNames.Aliyun.CreateContainerIfNotExists,
                        _configuration["BlobStoring:Aliyun:CreateContainerIfNotExists"]) ?? "false");
            });
        }
        else
        {
            config.UseFileSystem(fs =>
            {
                fs.BasePath = _configuration["BlobStoring:FileSystem:BasePath"] ?? "wwwroot/FileStorage";
            });
        }

        return config;
    }

    private string? GetSetting(string settingName, string? configFallback)
    {
        var value = _settingProvider.GetOrNullAsync(settingName).GetAwaiter().GetResult();
        return string.IsNullOrWhiteSpace(value) ? configFallback : value;
    }
}
