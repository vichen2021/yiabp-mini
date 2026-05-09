namespace Yi.Framework.Aliyun.Sms;

public class AliyunOptions
{
    public string AccessKeyId { get; set; } = string.Empty;
    public string AccessKeySecret { get; set; } = string.Empty;
    public AliyunSmsOptions Sms { get; set; } = new();
}

public class AliyunSmsOptions
{
    public string SignName { get; set; } = string.Empty;
    public string TemplateCode { get; set; } = string.Empty;
}
