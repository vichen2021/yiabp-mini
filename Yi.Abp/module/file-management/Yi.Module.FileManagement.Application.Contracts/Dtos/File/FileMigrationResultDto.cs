namespace Yi.Module.FileManagement.Application.Contracts.Dtos.File;

/// <summary>
/// 文件迁移结果
/// </summary>
public class FileMigrationResultDto
{
    /// <summary>总数</summary>
    public int Total { get; set; }

    /// <summary>成功数</summary>
    public int Success { get; set; }

    /// <summary>失败数</summary>
    public int Failed { get; set; }

    /// <summary>错误详情</summary>
    public List<string> Errors { get; set; } = new();
}
