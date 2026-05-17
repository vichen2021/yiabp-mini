using Microsoft.Extensions.Configuration;
using Volo.Abp.Application.Services;
using Volo.Abp.BlobStoring;
using Yi.Module.FileManagement.Application.Contracts.IServices;
using Yi.Module.FileManagement.Domain.File;

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

    public FileInternalService(
        FileManager fileManager,
        IBlobContainer<FileManagementContainer> blobContainer,
        IConfiguration configuration)
    {
        _fileManager = fileManager;
        _blobContainer = blobContainer;
        _configuration = configuration;
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
            CreateStorageKey(id),
            GetCurrentProvider(),
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

    private string GetCurrentProvider()
    {
        return _configuration["BlobStoring:Provider"] ?? "FileSystem";
    }

    private string CreateStorageKey(Guid id)
    {
        var pathPrefix = _configuration["BlobStoring:PathPrefix"] ?? "default";
        pathPrefix = string.IsNullOrWhiteSpace(pathPrefix) ? "default" : pathPrefix.Trim('/');
        return $"{pathPrefix}/{id}";
    }
}
