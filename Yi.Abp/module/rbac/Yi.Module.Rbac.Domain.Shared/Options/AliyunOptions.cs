namespace Yi.Module.Rbac.Domain.Shared.Options
{
    public class AliyunOptions
    {
        public string AccessKeyId { get; set; } = string.Empty;
        public string AccessKeySecret { get; set; } = string.Empty;
        public AliyunSmsOptions Sms { get; set; } = new();
        public AliyunOssOptions Oss { get; set; } = new();
    }

    public class AliyunSmsOptions
    {
        public string SignName { get; set; } = string.Empty;
        public string TemplateCode { get; set; } = string.Empty;
    }

    public class AliyunOssOptions
    {
        public string Endpoint { get; set; } = string.Empty;
        public string ContainerName { get; set; } = string.Empty;
        public bool CreateContainerIfNotExists { get; set; }
    }
}
