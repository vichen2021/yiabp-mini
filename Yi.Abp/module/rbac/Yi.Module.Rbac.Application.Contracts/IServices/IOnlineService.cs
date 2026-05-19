using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Yi.Module.Rbac.Domain.Shared.Model;

namespace Yi.Module.Rbac.Application.Contracts.IServices
{
    public interface IOnlineService : IApplicationService
    {
      Task< PagedResultDto<OnlineUserModel>> GetListAsync(OnlineUserModel online);
    }
}
