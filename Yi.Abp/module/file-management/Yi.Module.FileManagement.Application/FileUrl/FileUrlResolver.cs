using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Yi.Module.FileManagement.Application.BlobStoring;
using Yi.Module.FileManagement.Application.Contracts.FileUrl;

namespace Yi.Module.FileManagement.Application.FileUrl;

public class FileUrlResolver : IFileUrlResolver, ITransientDependency
{
    private const string FileGetPath = "/api/file/get";

    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;
    private readonly FileStorageOptionsResolver _optionsResolver;

    public FileUrlResolver(
        IConfiguration configuration,
        ICurrentTenant currentTenant,
        FileStorageOptionsResolver optionsResolver)
    {
        _configuration = configuration;
        _currentTenant = currentTenant;
        _optionsResolver = optionsResolver;
    }

    public string? Resolve(Guid? fileId)
    {
        if (!fileId.HasValue || fileId.Value == Guid.Empty)
        {
            return null;
        }

        var provider = _optionsResolver.ResolveProvider();
        return string.Equals(provider, "Aliyun", StringComparison.OrdinalIgnoreCase)
            ? BuildOssUrl(fileId.Value)
            : BuildProxyUrl(fileId.Value);
    }

    private string BuildOssUrl(Guid fileId)
    {
        var aliyunOptions = _optionsResolver.ResolveAliyun();
        var pathPrefix = _optionsResolver.ResolvePathPrefix();

        return $"https://{aliyunOptions.ContainerName}.{aliyunOptions.Endpoint}/{pathPrefix}/{fileId}";
    }

    private string BuildProxyUrl(Guid fileId)
    {
        var baseUrl = GetPublicBaseUrl();
        var tenantId = _currentTenant.Id;
        var qs = tenantId.HasValue ? $"?tenant={tenantId.Value}" : string.Empty;
        var path = $"{FileGetPath}/{fileId}{qs}";

        return string.IsNullOrWhiteSpace(baseUrl) ? path : $"{baseUrl}{path}";
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
