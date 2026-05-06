using Yi.Framework.Ddd.Application.Contracts;
using Yi.Module.TenantManagement.Application.Contracts.Dtos;

namespace Yi.Module.TenantManagement.Application.Contracts
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

        /// <summary>
        /// 同步套餐菜单到租户
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="packageId">套餐ID</param>
        Task SyncPackageAsync(Guid tenantId, Guid packageId);
    }
}
