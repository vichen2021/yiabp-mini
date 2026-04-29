using Volo.Abp.Application.Dtos;
using Yi.Module.Rbac.Domain.Shared.Model;

namespace Yi.Module.Rbac.Application.Contracts.IServices
{
    public interface IOnlineService
    {
      Task< PagedResultDto<OnlineUserModel>> GetListAsync(OnlineUserModel online);
    }
}
