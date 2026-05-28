namespace Yi.Module.FileManagement.Domain.Shared.Settings;

/// <summary>
/// 文件管理模块 OSS 相关 Setting 键名常量。
/// 对应 Setting 表中的 Name 字段，供 ISettingProvider / ISettingManager 读写使用。
/// </summary>
public static class FileManagementSettingNames
{
    private const string Prefix = "FileManagement.Oss";

    /// <summary>当前使用的 OSS Provider，可选值：FileSystem / Aliyun。</summary>
    public const string Provider = Prefix + ".Provider";

    /// <summary>文件存储路径前缀，用于构建 StorageKey。</summary>
    public const string PathPrefix = Prefix + ".PathPrefix";

    /// <summary>阿里云 OSS 相关配置键。</summary>
    public static class Aliyun
    {
        private const string AliyunPrefix = Prefix + ".Aliyun";

        /// <summary>阿里云 AccessKeyId。</summary>
        public const string AccessKeyId = AliyunPrefix + ".AccessKeyId";

        /// <summary>阿里云 AccessKeySecret（加密存储）。</summary>
        public const string AccessKeySecret = AliyunPrefix + ".AccessKeySecret";

        /// <summary>阿里云 OSS Endpoint。</summary>
        public const string Endpoint = AliyunPrefix + ".Endpoint";

        /// <summary>阿里云 OSS Bucket 名称。</summary>
        public const string ContainerName = AliyunPrefix + ".ContainerName";

        /// <summary>阿里云 OSS 自定义访问域名。</summary>
        public const string CustomDomain = AliyunPrefix + ".CustomDomain";

        /// <summary>Bucket 不存在时是否自动创建。</summary>
        public const string CreateContainerIfNotExists = AliyunPrefix + ".CreateContainerIfNotExists";
    }
}
