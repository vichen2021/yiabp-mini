using Volo.Abp.Application.Dtos;
using Yi.Framework.FileManagement.Domain.Shared.Consts;

namespace Yi.Framework.FileManagement.Application.Contracts.Dtos;

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
    public string BeautifySize => BeautifyFileSize(FileSize);
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

    /// <summary>
    /// 美化文件大小（将字节数转换为可读格式）
    /// </summary>
    /// <param name="size">文件大小（字节）</param>
    /// <returns>格式化后的文件大小字符串</returns>
    private static string BeautifyFileSize(long size)
    {
        if (size == 0 || size == 1)
        {
            return $"{size} Byte";
        }

        if (size >= FileManagementConsts.Terabyte)
        {
            var fixedSize = ((float)size / (float)FileManagementConsts.Terabyte);
            return $"{FormatSize(fixedSize)} TB";
        }

        if (size >= FileManagementConsts.Gigabyte)
        {
            var fixedSize = ((float)size / (float)FileManagementConsts.Gigabyte);
            return $"{FormatSize(fixedSize)} GB";
        }

        if (size >= FileManagementConsts.Megabyte)
        {
            var fixedSize = ((float)size / (float)FileManagementConsts.Megabyte);
            return $"{FormatSize(fixedSize)} MB";
        }

        if (size >= FileManagementConsts.Kilobyte)
        {
            var fixedSize = ((float)size / (float)FileManagementConsts.Kilobyte);
            return $"{FormatSize(fixedSize)} KB";
        }

        return $"{size} B";
    }

    /// <summary>
    /// 格式化文件大小数字
    /// </summary>
    /// <param name="size">文件大小</param>
    /// <returns>格式化后的字符串</returns>
    private static string FormatSize(float size)
    {
        var s = $"{size:0.00}";
        return s.EndsWith("00") ? ((int)size).ToString() : s;
    }
}
