namespace Yi.Framework.Aliyun.Sms;

public interface IAliyunSmsManager
{
    Task SendSmsAsync(string phoneNumbers, string code);
}
