using Yi.Framework.Core.Enums;
using Yi.Framework.Core.Helper;
using Yi.Framework.Rbac.Domain.Shared.File;

namespace Yi.Framework.Rbac.Domain.File;

/// <summary>
/// FileAggregateRoot 扩展方法
/// </summary>
public static class FileAggregateRootExtensions
{
    /// <summary>
    /// 获取文件类型
    /// </summary>
    public static FileTypeEnum GetFileType(this FileAggregateRoot file)
    {
        return MimeHelper.GetFileType(file.FileName);
    }

    /// <summary>
    /// 获取文件类型
    /// </summary>
    public static FileTypeEnum GetFileType(this FileDto file)
    {
        return MimeHelper.GetFileType(file.FileName);
    }

    /// <summary>
    /// 获取 MIME 类型映射
    /// </summary>
    public static string GetMimeMapping(this FileAggregateRoot file)
    {
        return MimeHelper.GetMimeMapping(file.FileName);
    }

    /// <summary>
    /// 获取 MIME 类型映射
    /// </summary>
    public static string GetMimeMapping(this FileDto file)
    {
        return MimeHelper.GetMimeMapping(file.FileName);
    }
}
