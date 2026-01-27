using Volo.Abp.Application.Dtos;
using Yi.Framework.FileManagement.Files;

namespace Yi.Framework.FileManagement.Application.Contracts.Dtos.File;

/// <summary>
/// 文件列表输出
/// </summary>
public class FileGetListOutputDto : EntityDto<Guid>
{
    /// <summary>
    /// 文件大小
    /// </summary>
    public long FileSize { get; set; }

    /// <summary>
    /// 可读文件大小
    /// </summary>
    public string BeautifySize => SizeHelper.BeautifySize(FileSize);

    /// <summary>
    /// 文件类型
    /// </summary>
    public string ContentType { get; set; } = string.Empty;

    /// <summary>
    /// 文件名称
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; }
}
