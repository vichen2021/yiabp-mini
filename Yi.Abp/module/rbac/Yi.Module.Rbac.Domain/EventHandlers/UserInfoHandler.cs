using Mapster;
using Volo.Abp.Caching;
using Volo.Abp.DependencyInjection;
using Volo.Abp.EventBus;
using Yi.Module.Rbac.Domain.Managers;
using Yi.Module.Rbac.Domain.Repositories;
using Yi.Module.Rbac.Domain.Shared.Caches;
using Yi.Module.Rbac.Domain.Shared.Dtos;
using Yi.Module.Rbac.Domain.Shared.Etos;

namespace Yi.Module.Rbac.Domain.EventHandlers
{
    public class UserInfoHandler : ILocalEventHandler<UserRoleMenuQueryEventArgs>, ITransientDependency
    {
        private UserManager _userManager;
        public UserInfoHandler(UserManager userManager)
        {
            _userManager = userManager;
        }
        public async Task HandleEventAsync(UserRoleMenuQueryEventArgs eventData)
        {
            //数据库查询方式
            var result = await _userManager.GetInfoListAsync(eventData.UserIds);
            eventData.Result = result;
        }
    }
}
