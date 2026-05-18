using AlibabaCloud.SDK.Dysmsapi20170525;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Services;

namespace Yi.Framework.Aliyun.Sms;

public class AliyunSmsManager : DomainService, IAliyunSmsManager, ITransientDependency
{
    private readonly ILogger<AliyunSmsManager> _logger;
    private AliyunOptions Options { get; }

    public AliyunSmsManager(ILogger<AliyunSmsManager> logger, IOptions<AliyunOptions> options)
    {
        Options = options.Value;
        _logger = logger;
    }

    private Client CreateClient()
    {
        var config = new AlibabaCloud.OpenApiClient.Models.Config
        {
            AccessKeyId = Options.AccessKeyId,
            AccessKeySecret = Options.AccessKeySecret,
            Endpoint = "dysmsapi.aliyuncs.com"
        };
        return new Client(config);
    }

    public async Task SendSmsAsync(string phoneNumbers, string code)
    {
        try
        {
            ValidateOptions();
            var aliyunClient = CreateClient();
            var sendSmsRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest
            {
                PhoneNumbers = phoneNumbers,
                SignName = Options.Sms.SignName,
                TemplateCode = Options.Sms.TemplateCode,
                TemplateParam = System.Text.Json.JsonSerializer.Serialize(new { code })
            };

            var response = await aliyunClient.SendSmsAsync(sendSmsRequest);
            if (!string.Equals(response.Body.Code, "OK", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("阿里云短信发送失败，Code:{Code}, Message:{Message}, RequestId:{RequestId}",
                    response.Body.Code,
                    response.Body.Message,
                    response.Body.RequestId);
                throw new UserFriendlyException($"短信发送失败：{response.Body.Message}");
            }
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "阿里云短信发送错误:{Message}", error.Message);
            throw new UserFriendlyException("阿里云短信发送错误:" + error.Message);
        }
    }

    public async Task SendSmsAsync(string phoneNumbers, string templateCode, Dictionary<string, string> templateParams)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(Options.AccessKeyId)
                || string.IsNullOrWhiteSpace(Options.AccessKeySecret)
                || string.IsNullOrWhiteSpace(Options.Sms.SignName)
                || string.IsNullOrWhiteSpace(templateCode))
            {
                throw new UserFriendlyException("阿里云短信配置不完整，请检查 AliyunOptions 配置节点");
            }

            var aliyunClient = CreateClient();
            var sendSmsRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest
            {
                PhoneNumbers = phoneNumbers,
                SignName = Options.Sms.SignName,
                TemplateCode = templateCode,
                TemplateParam = System.Text.Json.JsonSerializer.Serialize(templateParams)
            };

            var response = await aliyunClient.SendSmsAsync(sendSmsRequest);
            if (!string.Equals(response.Body.Code, "OK", StringComparison.OrdinalIgnoreCase))
            {
                _logger.LogWarning("阿里云短信发送失败，Code:{Code}, Message:{Message}, RequestId:{RequestId}",
                    response.Body.Code,
                    response.Body.Message,
                    response.Body.RequestId);
                throw new UserFriendlyException($"短信发送失败：{response.Body.Message}");
            }
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception error)
        {
            _logger.LogError(error, "阿里云短信发送错误:{Message}", error.Message);
            throw new UserFriendlyException("阿里云短信发送错误:" + error.Message);
        }
    }

    private void ValidateOptions()
    {
        if (string.IsNullOrWhiteSpace(Options.AccessKeyId)
            || string.IsNullOrWhiteSpace(Options.AccessKeySecret)
            || string.IsNullOrWhiteSpace(Options.Sms.SignName)
            || string.IsNullOrWhiteSpace(Options.Sms.TemplateCode))
        {
            throw new UserFriendlyException("阿里云短信配置不完整，请检查 AliyunOptions 配置节点");
        }
    }
}
