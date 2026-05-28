using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;
using Yi.Module.FileManagement.Application.Contracts.FileUrl;
using Yi.Module.FileManagement.Domain.Shared.Settings;

namespace Yi.Module.FileManagement.Application.FileUrl;

public class FileUrlResolver : IFileUrlResolver, ITransientDependency
{
    private const string FileGetPath = "/api/file/get";

    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;
    private readonly ISettingProvider _settingProvider;

    public FileUrlResolver(IConfiguration configuration, ICurrentTenant currentTenant, ISettingProvider settingProvider)
    {
        _configuration = configuration;
        _currentTenant = currentTenant;
        _settingProvider = settingProvider;
    }

    public string? Resolve(Guid? fileId)
    {
        if (!fileId.HasValue || fileId.Value == Guid.Empty)
        {
            return null;
        }

        return IsAliyunProvider()
            ? BuildOssUrl(fileId.Value)
            : BuildProxyUrl(fileId.Value);
    }

    private bool IsAliyunProvider()
    {
        var provider = GetSetting(FileManagementSettingNames.Provider,
            _configuration["BlobStoring:Provider"]);
        return string.Equals(provider, "Aliyun", StringComparison.OrdinalIgnoreCase);
    }

    private string BuildOssUrl(Guid fileId)
    {
        var endpoint = GetSetting(FileManagementSettingNames.Aliyun.Endpoint,
            _configuration["BlobStoring:Aliyun:Endpoint"]) ?? "";
        var containerName = GetSetting(FileManagementSettingNames.Aliyun.ContainerName,
            _configuration["BlobStoring:Aliyun:ContainerName"]) ?? "";
        var pathPrefix = GetSetting(FileManagementSettingNames.PathPrefix,
            _configuration["BlobStoring:PathPrefix"]) ?? "default";
        pathPrefix = pathPrefix.Trim('/');

        return $"https://{containerName}.{endpoint}/{pathPrefix}/{fileId}";
    }

    private string BuildProxyUrl(Guid fileId)
    {
        var baseUrl = GetPublicBaseUrl();
        var tenantId = _currentTenant.Id;
        var qs = tenantId.HasValue ? $"?tenant={tenantId.Value}" : string.Empty;
        var path = $"{FileGetPath}/{fileId}{qs}";

        return string.IsNullOrWhiteSpace(baseUrl) ? path : $"{baseUrl}{path}";
    }

    private string? GetSetting(string settingName, string? configFallback)
    {
        var value = _settingProvider.GetOrNullAsync(settingName).GetAwaiter().GetResult();
        return string.IsNullOrWhiteSpace(value) ? configFallback : value;
    }

    private string? GetPublicBaseUrl()
    {
        var configuredUrl = _configuration["App:PublicUrl"] ?? _configuration["App:SelfUrl"];
        if (string.IsNullOrWhiteSpace(configuredUrl) || configuredUrl.Contains('*'))
        {
            return null;
        }

        return configuredUrl.TrimEnd('/');
    }
}
