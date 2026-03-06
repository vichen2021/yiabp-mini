using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.TenantManagement.Application.Contracts.Dtos;

namespace Yi.Framework.TenantManagement.Application.Contracts
{
    public interface ITenantService:IYiCrudAppService< TenantGetOutputDto, TenantGetListOutputDto, Guid, TenantGetListInput, TenantCreateInput, TenantUpdateInput>
    {
        /// <summary>
        /// 租户选项
        /// </summary>
        Task<List<TenantSelectOutputDto>> GetSelectAsync();

        /// <summary>
        /// 初始化租户
        /// </summary>
        Task<TenantInitOutputDto> InitAsync(Guid id, TenantInitInput input);
    }
}
