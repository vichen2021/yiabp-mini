namespace Yi.Module.FileManagement.Application.Contracts.Dtos;

/// <summary>
/// 租户 OSS 设置读写 DTO。
/// <para>读取时 <see cref="AccessKeySecret"/> 不回显（返回空字符串）；写入时传明文，由框架加密落库。</para>
/// </summary>
public class TenantOssSettingDto
{
    /// <summary>OSS Provider，可选值：FileSystem / Aliyun。</summary>
    public string? Provider { get; set; }

    /// <summary>文件存储路径前缀，用于构建 StorageKey。</summary>
    public string? PathPrefix { get; set; }

    /// <summary>阿里云 AccessKeyId。</summary>
    public string? AccessKeyId { get; set; }

    /// <summary>阿里云 AccessKeySecret。写入时传明文，读取时始终返回空（不回显）。</summary>
    public string? AccessKeySecret { get; set; }

    /// <summary>阿里云 OSS Endpoint。</summary>
    public string? Endpoint { get; set; }

    /// <summary>阿里云 OSS Bucket 名称。</summary>
    public string? ContainerName { get; set; }

    /// <summary>Bucket 不存在时是否自动创建。</summary>
    public bool CreateContainerIfNotExists { get; set; }
}
