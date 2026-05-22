using Microsoft.Extensions.Configuration;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Volo.Abp.Settings;
using Yi.Module.FileManagement.Application.Contracts.IServices;
using Yi.Module.FileManagement.Domain.File;
using Yi.Module.FileManagement.Domain.Shared.Settings;

namespace Yi.Module.FileManagement.Application.Services;

/// <summary>
/// 文件模块内部字节级读写服务
/// </summary>
[RemoteService(false)]
public class FileInternalService : ApplicationService, IFileInternalService
{
    private readonly FileManager _fileManager;
    private readonly IBlobContainer<FileManagementContainer> _blobContainer;
    private readonly IConfiguration _configuration;
    private readonly ISettingProvider _settingProvider;

    public FileInternalService(
        FileManager fileManager,
        IBlobContainer<FileManagementContainer> blobContainer,
        IConfiguration configuration,
        ISettingProvider settingProvider)
    {
        _fileManager = fileManager;
        _blobContainer = blobContainer;
        _configuration = configuration;
        _settingProvider = settingProvider;
    }

    /// <summary>
    /// 以字节数组方式写入文件，返回落库后的文件 ID
    /// </summary>
    public async Task<Guid> CreateFromBytesAsync(string fileName, string contentType, byte[] content)
    {
        var id = GuidGenerator.Create();
        await _fileManager.CreateAsync(
            id,
            fileName,
            content.Length,
            contentType,
            content,
            await CreateStorageKeyAsync(id),
            await GetCurrentProviderAsync(),
            overwrite: false);
        return id;
    }

    /// <summary>
    /// 读取文件字节内容
    /// </summary>
    public async Task<(byte[] Content, string ContentType, string FileName)> GetBytesAsync(Guid fileId)
    {
        var fileObject = await _fileManager.GetAsync(fileId);
        var stream = await _blobContainer.GetAsync(fileObject.StorageKey);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms);
        return (ms.ToArray(), fileObject.ContentType, fileObject.FileName);
    }

    private async Task<string> GetCurrentProviderAsync()
    {
        var v = await _settingProvider.GetOrNullAsync(FileManagementSettingNames.Provider);
        return string.IsNullOrWhiteSpace(v)
            ? (_configuration["BlobStoring:Provider"] ?? "FileSystem")
            : v;
    }

    private async Task<string> CreateStorageKeyAsync(Guid id)
    {
        var pathPrefix = await _settingProvider.GetOrNullAsync(FileManagementSettingNames.PathPrefix);
        if (string.IsNullOrWhiteSpace(pathPrefix))
            pathPrefix = _configuration["BlobStoring:PathPrefix"] ?? "default";
        pathPrefix = pathPrefix.Trim('/');
        return $"{pathPrefix}/{id}";
    }
}
