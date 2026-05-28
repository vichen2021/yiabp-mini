using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Yi.Module.FileManagement.Domain.Shared.Settings;

namespace Yi.Module.FileManagement.Application.BlobStoring;

public class FileStorageOptionsResolver : ITransientDependency
{
    private const string FileSystemProvider = "FileSystem";
    private const string AliyunProvider = "Aliyun";

    private readonly ISettingProvider _settingProvider;
    private readonly IConfiguration _configuration;
    private readonly ILogger<FileStorageOptionsResolver> _logger;

    public FileStorageOptionsResolver(
        ISettingProvider settingProvider,
        IConfiguration configuration,
        ILogger<FileStorageOptionsResolver> logger)
    {
        _settingProvider = settingProvider;
        _configuration = configuration;
        _logger = logger;
    }

    public string ResolveProvider()
    {
        var settingValue = _settingProvider.GetOrNullAsync(FileManagementSettingNames.Provider).GetAwaiter().GetResult();
        if (!string.IsNullOrWhiteSpace(settingValue))
        {
            return ValidateProvider(settingValue, "Setting");
        }

        var configValue = _configuration["BlobStoring:Provider"];
        if (!string.IsNullOrWhiteSpace(configValue))
        {
            return ValidateProvider(configValue, "Configuration");
        }

        _logger.LogError("文件存储 Provider 未配置。请配置 {SettingName} 或 BlobStoring:Provider", FileManagementSettingNames.Provider);
        throw new UserFriendlyException("文件存储 Provider 未配置，请先配置文件存储方式");
    }

    public string ResolvePathPrefix()
    {
        var settingValue = _settingProvider.GetOrNullAsync(FileManagementSettingNames.PathPrefix).GetAwaiter().GetResult();
        var value = string.IsNullOrWhiteSpace(settingValue)
            ? (_configuration[FileManagementSettingNames.PathPrefix] ?? _configuration["BlobStoring:PathPrefix"])
            : settingValue;

        if (string.IsNullOrWhiteSpace(value))
        {
            _logger.LogWarning("文件存储 PathPrefix 未配置，已回退到 default");
            value = "default";
        }

        return value.Trim('/');
    }

    public string ResolveFileSystemBasePath()
    {
        var basePath = _configuration["BlobStoring:FileSystem:BasePath"];
        if (!string.IsNullOrWhiteSpace(basePath))
        {
            return basePath;
        }

        _logger.LogWarning("文件系统存储 BasePath 未配置，已回退到 wwwroot/FileStorage");
        return "wwwroot/FileStorage";
    }

    public AliyunStorageOptions ResolveAliyun()
    {
        return new AliyunStorageOptions(
            GetRequiredAliyunValue(FileManagementSettingNames.Aliyun.AccessKeyId, "BlobStoring:Aliyun:AccessKeyId", "AccessKeyId"),
            GetRequiredAliyunValue(FileManagementSettingNames.Aliyun.AccessKeySecret, "BlobStoring:Aliyun:AccessKeySecret", "AccessKeySecret"),
            GetRequiredAliyunValue(FileManagementSettingNames.Aliyun.Endpoint, "BlobStoring:Aliyun:Endpoint", "Endpoint"),
            GetRequiredAliyunValue(FileManagementSettingNames.Aliyun.ContainerName, "BlobStoring:Aliyun:ContainerName", "ContainerName"),
            bool.Parse(GetAliyunValue(FileManagementSettingNames.Aliyun.CreateContainerIfNotExists, "BlobStoring:Aliyun:CreateContainerIfNotExists") ?? "false"));
    }

    public string CreateStorageKey(Guid id)
    {
        return $"{ResolvePathPrefix()}/{id}";
    }

    private string ValidateProvider(string provider, string source)
    {
        if (string.Equals(provider, FileSystemProvider, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("文件存储 Provider 解析为 FileSystem，来源：{Source}", source);
            return FileSystemProvider;
        }

        if (string.Equals(provider, AliyunProvider, StringComparison.OrdinalIgnoreCase))
        {
            _logger.LogInformation("文件存储 Provider 解析为 Aliyun，来源：{Source}", source);
            return AliyunProvider;
        }

        _logger.LogError("文件存储 Provider 配置无效：{Provider}，来源：{Source}", provider, source);
        throw new UserFriendlyException($"文件存储 Provider 配置无效：{provider}");
    }

    private string GetRequiredAliyunValue(string settingName, string configurationKey, string displayName)
    {
        var value = GetAliyunValue(settingName, configurationKey);
        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }

        _logger.LogError("阿里云 OSS 配置缺失：{DisplayName}。Setting：{SettingName}，Configuration：{ConfigurationKey}", displayName, settingName, configurationKey);
        throw new UserFriendlyException($"阿里云 OSS 配置缺失：{displayName}");
    }

    private string? GetAliyunValue(string settingName, string configurationKey)
    {
        var settingValue = _settingProvider.GetOrNullAsync(settingName).GetAwaiter().GetResult();
        return string.IsNullOrWhiteSpace(settingValue) ? _configuration[configurationKey] : settingValue;
    }
}

public sealed record AliyunStorageOptions(
    string AccessKeyId,
    string AccessKeySecret,
    string Endpoint,
    string ContainerName,
    bool CreateContainerIfNotExists);
