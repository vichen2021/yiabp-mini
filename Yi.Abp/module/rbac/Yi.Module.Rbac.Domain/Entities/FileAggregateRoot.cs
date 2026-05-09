using SqlSugar;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Yi.Framework.Core.Data;
using Yi.Framework.Core.Enums;
using Yi.Framework.Core.Helper;
using Yi.Module.Rbac.Domain.Shared.File;

namespace Yi.Module.Rbac.Domain.Entities;

/// <summary>
/// 文件
/// </summary>
[SugarTable("File")]
public class FileAggregateRoot : AggregateRoot<Guid>, IAuditedObject
{
    public FileAggregateRoot()
    {
    }

    public FileAggregateRoot(
        Guid id,
        string fileName,
        long fileSize,
        string contentType,
        string storageKey,
        string hash,
        string provider
    ) : base(id)
    {
        SetFileSize(fileSize);
        SetContentType(contentType);
        SetFileName(fileName);
        SetStorageKey(storageKey);
        SetExtension(fileName);
        SetFileType(fileName);
        SetHash(hash);
        SetProvider(provider);
        CreationTime = DateTime.Now;
    }

    [SugarColumn(IsPrimaryKey = true)]
    public override Guid Id { get; protected set; }


    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; private set; } = string.Empty;

    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; private set; }

    /// <summary>
    /// 文件类型
    /// </summary>
    public string ContentType { get; private set; } = string.Empty;

    [SugarColumn(Length = 256)]
    public string StorageKey { get; private set; } = string.Empty;

    [SugarColumn(Length = 32)]
    public string Extension { get; private set; } = string.Empty;

    public FileTypeEnum FileType { get; private set; }

    [SugarColumn(Length = 64)]
    public string Hash { get; private set; } = string.Empty;

    [SugarColumn(Length = 32)]
    public string Provider { get; private set; } = string.Empty;

    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }

    /// <summary>
    /// 设置文件大小
    /// </summary>
    public void SetFileSize(long fileSize)
    {
        FileSize = fileSize;
    }

    /// <summary>
    /// 设置文件类型
    /// </summary>
    private void SetContentType(string contentType)
    {
        if (string.IsNullOrWhiteSpace(contentType) || contentType.Length > 128)
            throw new ArgumentException("ContentType cannot be null, empty or exceed 128 characters.", nameof(contentType));
        ContentType = contentType;
    }

    /// <summary>
    /// 设置文件名称
    /// </summary>
    private void SetFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 256)
            throw new ArgumentException("FileName cannot be null, empty or exceed 256 characters.", nameof(fileName));
        FileName = fileName;
    }

    private void SetStorageKey(string storageKey)
    {
        if (string.IsNullOrWhiteSpace(storageKey) || storageKey.Length > 256)
            throw new ArgumentException("StorageKey cannot be null, empty or exceed 256 characters.", nameof(storageKey));
        StorageKey = storageKey;
    }

    private void SetExtension(string fileName)
    {
        var extension = System.IO.Path.GetExtension(fileName);
        if (extension.Length > 32)
            throw new ArgumentException("Extension cannot exceed 32 characters.", nameof(fileName));
        Extension = extension;
    }

    private void SetFileType(string fileName)
    {
        FileType = MimeHelper.GetFileType(fileName);
    }

    private void SetHash(string hash)
    {
        if (hash.Length > 64)
            throw new ArgumentException("Hash cannot exceed 64 characters.", nameof(hash));
        Hash = hash;
    }

    private void SetProvider(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider) || provider.Length > 32)
            throw new ArgumentException("Provider cannot be null, empty or exceed 32 characters.", nameof(provider));
        Provider = provider;
    }

    /// <summary>
    /// 更新文件
    /// </summary>
    public void Update(long fileSize, string contentType, string fileName, string hash, string provider)
    {
        SetFileSize(fileSize);
        SetContentType(contentType);
        SetFileName(fileName);
        SetExtension(fileName);
        SetFileType(fileName);
        SetHash(hash);
        SetProvider(provider);
    }

    /// <summary>
    /// 获取文件类型
    /// </summary>
    public FileTypeEnum GetFileType()
    {
        return MimeHelper.GetFileType(FileName);
    }

    /// <summary>
    /// 获取 MIME 类型映射
    /// </summary>
    public string GetMimeMapping()
    {
        return MimeHelper.GetMimeMapping(FileName);
    }
}
