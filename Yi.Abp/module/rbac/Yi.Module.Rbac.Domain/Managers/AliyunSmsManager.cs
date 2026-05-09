using AlibabaCloud.SDK.Dysmsapi20170525;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp;
using Volo.Abp.Domain.Services;
using Yi.Module.Rbac.Domain.Shared.Options;

namespace Yi.Module.Rbac.Domain.Managers
{
    public class AliyunSmsManager : DomainService, IAliyunSmsManager
    {
        private ILogger<AliyunSmsManager> _logger;
        private AliyunOptions Options { get; set; }
        public AliyunSmsManager(ILogger<AliyunSmsManager> logger, IOptions<AliyunOptions> options)
        {
            Options = options.Value;
            _logger = logger;
        }

        private Client CreateClient()
        {
            AlibabaCloud.OpenApiClient.Models.Config config = new AlibabaCloud.OpenApiClient.Models.Config
            {
                // 必填，您的 AccessKey ID
                AccessKeyId = Options.AccessKeyId,
                // 必填，您的 AccessKey Secret
                AccessKeySecret = Options.AccessKeySecret,
            };
            // 访问的域名
            config.Endpoint = "dysmsapi.aliyuncs.com";
            return new Client(config);
        }


        /// <summary>
        /// 发送短信
        /// </summary>
        /// <param name="phoneNumbers"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        public async Task SendSmsAsync(string phoneNumbers, string code)
        {
            try
            {
                ValidateOptions();
                var _aliyunClient = CreateClient();
                AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest sendSmsRequest = new AlibabaCloud.SDK.Dysmsapi20170525.Models.SendSmsRequest
                {
                    PhoneNumbers = phoneNumbers,
                    SignName = Options.Sms.SignName,
                    TemplateCode = Options.Sms.TemplateCode,
                    TemplateParam = System.Text.Json.JsonSerializer.Serialize(new { code })
                };

                var response = await _aliyunClient.SendSmsAsync(sendSmsRequest);
                if (!string.Equals(response.Body.Code, "OK", StringComparison.OrdinalIgnoreCase))
                {
                    _logger.LogWarning("阿里云短信发送失败，Code:{Code}, Message:{Message}, RequestId:{RequestId}",
                        response.Body.Code,
                        response.Body.Message,
                        response.Body.RequestId);
                    throw new UserFriendlyException($"短信发送失败：{response.Body.Message}");
                }
            }

            catch (Exception _error)
            {
                _logger.LogError(_error, "阿里云短信发送错误:" + _error.Message);
                throw new UserFriendlyException("阿里云短信发送错误:" + _error.Message);
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

    public interface IAliyunSmsManager
    {
        Task SendSmsAsync(string phoneNumbers, string code);
    }
}
