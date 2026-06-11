using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Volo.Abp.AspNetCore.SignalR;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Shared.Model;

namespace Yi.Module.Rbac.Application.SignalRHubs
{
    [HubRoute("/hub/main")]
    public class OnlineHub : AbpHub
    {
        public static ConcurrentDictionary<string, OnlineUserModel> ClientUsersDic { get; set; } = new();

        private readonly ILogger<OnlineHub> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public OnlineHub(ILogger<OnlineHub> logger, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext() ?? _httpContextAccessor?.HttpContext;
            
            if (httpContext is null)
            {
                return Task.CompletedTask;
            }

            var name = CurrentUser.UserName;
            var loginUser = LoginLogFactory.CreateFromHttpContext(httpContext);

            OnlineUserModel user = new(Context.ConnectionId)
            {
                Browser = loginUser?.Browser,
                LoginLocation = loginUser?.LoginLocation,
                Ipaddr = loginUser?.LoginIp,
                LoginTime = DateTime.Now,
                Os = loginUser?.Os,
                UserName = name ?? "Null",
                UserId = CurrentUser.Id ?? Guid.Empty
            };

            if (CurrentUser.IsAuthenticated)
            {
                ClientUsersDic.RemoveAll(u => u.Value.UserId == CurrentUser.Id);
            }

            ClientUsersDic.AddOrUpdate(Context.ConnectionId, user, (_, _) => user);

            if (CurrentUser.IsAuthenticated)
            {
                _logger.LogDebug(
                    "{Time}：{Name},{ConnectionId}连接服务端success，当前已连接{Count}个",
                    DateTime.Now, name, Context.ConnectionId, ClientUsersDic.Count);
            }
            Clients.All.SendAsync("onlineNum", ClientUsersDic.Count);

            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? exception)
        {
            if (CurrentUser.IsAuthenticated)
            {
                ClientUsersDic.RemoveAll(u => u.Value.UserId == CurrentUser.Id);
                _logger.LogDebug("用户{UserName}离开了，当前已连接{Count}个", CurrentUser?.UserName, ClientUsersDic.Count);
            }
            ClientUsersDic.Remove(Context.ConnectionId, out _);
            Clients.All.SendAsync("onlineNum", ClientUsersDic.Count);
            return base.OnDisconnectedAsync(exception);
        }
    }
}
