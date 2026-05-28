using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Module.FileManagement.Application.BlobStoring;
using Yi.Module.FileManagement.Application.Contracts.FileUrl;
using Yi.Module.FileManagement.Domain.Entities;

namespace Yi.Module.FileManagement.Application.FileUrl;

public class FileUrlResolver : IFileUrlResolver, ITransientDependency
{
    private const string FileGetPath = "/api/file/get";

    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;
    private readonly ISqlSugarRepository<FileAggregateRoot, Guid> _repository;
    private readonly FileStorageOptionsResolver _optionsResolver;

    public FileUrlResolver(
        IConfiguration configuration,
        ICurrentTenant currentTenant,
        ISqlSugarRepository<FileAggregateRoot, Guid> repository,
        FileStorageOptionsResolver optionsResolver)
    {
        _configuration = configuration;
        _currentTenant = currentTenant;
        _repository = repository;
        _optionsResolver = optionsResolver;
    }

    public string? Resolve(Guid? fileId, string? storageKey = null)
    {
        if (!fileId.HasValue || fileId.Value == Guid.Empty)
        {
            return null;
        }

        var provider = _optionsResolver.ResolveProvider();
        return string.Equals(provider, "Aliyun", StringComparison.OrdinalIgnoreCase)
            ? BuildOssUrl(fileId.Value, storageKey)
            : BuildProxyUrl(fileId.Value);
    }

    private string BuildOssUrl(Guid fileId, string? storageKey)
    {
        var aliyunOptions = _optionsResolver.ResolveAliyun();
        var objectKey = string.IsNullOrWhiteSpace(storageKey)
            ? ResolveStorageKey(fileId)
            : storageKey.Trim('/');

        var tenantId = _currentTenant.Id;
        if (tenantId.HasValue && !objectKey.StartsWith("tenants/", StringComparison.OrdinalIgnoreCase))
        {
            objectKey = $"tenants/{tenantId.Value}/{objectKey}";
        }

        return $"https://{aliyunOptions.ContainerName}.{aliyunOptions.Endpoint}/{objectKey}";
    }

    private string ResolveStorageKey(Guid fileId)
    {
        var file = _repository.GetByIdAsync(fileId).GetAwaiter().GetResult();
        return string.IsNullOrWhiteSpace(file?.StorageKey)
            ? _optionsResolver.CreateStorageKey(fileId)
            : file.StorageKey.Trim('/');
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
