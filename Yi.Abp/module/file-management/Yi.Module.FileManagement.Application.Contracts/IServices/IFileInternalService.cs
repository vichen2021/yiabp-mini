namespace Yi.Module.FileManagement.Application.Contracts.IServices;

/// <summary>
/// 文件内部存储服务
/// </summary>
public interface IFileInternalService
{
    /// <summary>
    /// 以字节数组方式写入文件，返回落库后的文件 ID
    /// </summary>
    Task<Guid> CreateFromBytesAsync(string fileName, string contentType, byte[] content);

    /// <summary>
    /// 读取文件字节内容
    /// </summary>
    Task<(byte[] Content, string ContentType, string FileName)> GetBytesAsync(Guid fileId);
}
