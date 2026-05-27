using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Yi.Module.FileManagement.Application.Contracts.FileUrl;

namespace Yi.Module.FileManagement.Application.FileUrl;

public class FileUrlResolver : IFileUrlResolver, ITransientDependency
{
    private const string FileGetPath = "/api/file/get";

    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;

    public FileUrlResolver(IConfiguration configuration, ICurrentTenant currentTenant)
    {
        _configuration = configuration;
        _currentTenant = currentTenant;
    }

    public string? Resolve(Guid? fileId)
    {
        if (!fileId.HasValue || fileId.Value == Guid.Empty)
        {
            return null;
        }

        var baseUrl = GetPublicBaseUrl();
        var tenantId = _currentTenant.Id;
        var qs = tenantId.HasValue ? $"?tenant={tenantId.Value}" : string.Empty;
        var path = $"{FileGetPath}/{fileId.Value}{qs}";

        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return path;
        }

        return $"{baseUrl}{path}";
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
