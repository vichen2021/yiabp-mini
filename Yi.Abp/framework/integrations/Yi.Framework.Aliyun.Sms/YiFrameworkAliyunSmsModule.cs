using Microsoft.Extensions.DependencyInjection;

namespace Yi.Framework.Aliyun.Sms;

public class YiFrameworkAliyunSmsModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        var configuration = context.Services.GetConfiguration();
        Configure<AliyunOptions>(configuration.GetSection(nameof(AliyunOptions)));
    }
}
