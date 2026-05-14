using Microsoft.Extensions.Configuration;
using Volo.Abp.DependencyInjection;
using Yi.Module.FileManagement.Application.Contracts.FileUrl;

namespace Yi.Module.FileManagement.Application.FileUrl;

public class FileUrlResolver : IFileUrlResolver, ITransientDependency
{
    private const string FileGetPath = "/api/file/get";

    private readonly IConfiguration _configuration;

    public FileUrlResolver(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string? Resolve(Guid? fileId)
    {
        if (!fileId.HasValue || fileId.Value == Guid.Empty)
        {
            return null;
        }

        var baseUrl = GetPublicBaseUrl();
        var path = $"{FileGetPath}/{fileId.Value}";

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
