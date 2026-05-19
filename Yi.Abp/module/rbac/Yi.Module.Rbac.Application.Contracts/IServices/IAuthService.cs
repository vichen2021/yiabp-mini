using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Yi.Module.Rbac.Application.Contracts.Dtos.Account;

namespace Yi.Module.Rbac.Application.Contracts.IServices;

public interface IAuthService : IApplicationService
{
    Task<AuthOutputDto?> TryGetAuthInfoAsync(string? openId, string authType, Guid? userId = null);
    Task<AuthOutputDto> CreateAsync(AuthCreateOrUpdateInputDto input);
}