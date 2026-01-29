using SqlSugar;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Yi.Framework.Core.Data;

namespace Yi.Framework.Rbac.Domain.File;

/// <summary>
/// 文件
/// </summary>
[SugarTable("File")]
public class FileAggregateRoot : AggregateRoot<Guid>,  IAuditedObject
{
    public FileAggregateRoot()
    {
    }

    public FileAggregateRoot(
        Guid id,
        string fileName,
        long fileSize,
        string contentType,
        Guid? tenantId = null
    ) : base(id)
    {
        SetFileSize(fileSize);
        SetContentType(contentType);
        SetFileName(fileName);
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
        if (string.IsNullOrWhiteSpace(fileName) || fileName.Length > 128)
            throw new ArgumentException("FileName cannot be null, empty or exceed 128 characters.", nameof(fileName));
        FileName = fileName;
    }

    /// <summary>
    /// 更新文件
    /// </summary>
    public void Update(long fileSize, string contentType, string fileName)
    {
        SetFileSize(fileSize);
        SetContentType(contentType);
        SetFileName(fileName);
    }
}
