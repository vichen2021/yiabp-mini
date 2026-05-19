using IPTools.Core;
using Microsoft.AspNetCore.Http;
using UAParser;
using Yi.Framework.Core.Extensions;
using Yi.Module.Rbac.Domain.Entities;

namespace Yi.Module.Rbac.Application
{
    public static class LoginLogFactory
    {
        public static LoginLogAggregateRoot CreateFromHttpContext(HttpContext context)
        {
            ClientInfo GetClientInfo(HttpContext ctx)
            {
                var str = ctx.GetUserAgent();
                var uaParser = Parser.GetDefault();
                ClientInfo c;
                try
                {
                    c = uaParser.Parse(str);
                }
                catch
                {
                    c = new ClientInfo("null", new OS("null", "null", "null", "null", "null"), new Device("null", "null", "null"), new UserAgent("null", "null", "null", "null"));
                }
                return c;
            }

            var ipAddr = context.GetClientIp();
            IpInfo location;
            if (ipAddr == "127.0.0.1")
            {
                location = new IpInfo() { Province = "本地", City = "本机" };
            }
            else
            {
                try
                {
                    location = IpTool.Search(ipAddr);
                }
                catch
                {
                    location = new IpInfo() { Province = ipAddr, City = "未知地区" };
                }
            }

            ClientInfo clientInfo = GetClientInfo(context);
            return new LoginLogAggregateRoot
            {
                Browser = clientInfo.Device.Family,
                Os = clientInfo.OS.ToString(),
                LoginIp = ipAddr,
                LoginLocation = location.Province + "-" + location.City
            };
        }
    }
}
