using Volo.Abp.Application.Dtos;

namespace Yi.Framework.FileManagement.Application.Contracts.Dtos.File;

/// <summary>
/// 文件列表查询参数
/// </summary>
public class FileGetListInputVo : PagedAndSortedResultRequestDto
{
    /// <summary>
    /// 文件名称
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// 开始创建时间
    /// </summary>
    public DateTime? StartTime { get; set; }

    /// <summary>
    /// 结束创建时间
    /// </summary>
    public DateTime? EndTime { get; set; }
}
