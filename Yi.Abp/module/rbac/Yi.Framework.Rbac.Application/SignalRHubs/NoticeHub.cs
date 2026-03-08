using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Volo.Abp.AspNetCore.SignalR;

namespace Yi.Framework.Rbac.Application.SignalRHubs
{
    [HubRoute("/hub/notice")]
    public class NoticeHub : AbpHub
    {
        private readonly IHubContext<NoticeHub> _hubContext;

        public NoticeHub(IHubContext<NoticeHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public override async Task OnConnectedAsync()
        {
            if (CurrentUser.IsAuthenticated && CurrentUser.Id.HasValue)
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"User_{CurrentUser.Id}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (CurrentUser.IsAuthenticated && CurrentUser.Id.HasValue)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User_{CurrentUser.Id}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public async Task SendNoticeToUser(string userId, string type, string title, string content)
        {
            await Clients.Group($"User_{userId}").SendAsync("ReceiveNotice", type, title, content);
        }

        public async Task SendNoticeToAll(string type, string title, string content)
        {
            await Clients.All.SendAsync("ReceiveNotice", type, title, content);
        }
    }
}
