namespace Yi.Framework.FileManagement.Domain.Shared.Consts;

public static class FileManagementConsts
{
    /// <summary>名称空间</summary>

    public const int Kilobyte = 1024;
    public const int Megabyte = Kilobyte * 1024;
    public const long Gigabyte = Megabyte * 1024;
    public const long Terabyte = Gigabyte * 1024;
    
    public const string FileAlreadyExist = "文件已存在";
    public const string FileNotFound = "文件未找到";
}
