using Yi.Framework.Ddd.Application.Contracts;
using Yi.Module.TenantManagement.Application.Contracts.Dtos.TenantPackage;

namespace Yi.Module.TenantManagement.Application.Contracts.IServices
{
    /// <summary>
    /// 租户套餐服务接口
    /// </summary>
    public interface ITenantPackageService : IYiCrudAppService<TenantPackageGetOutputDto, TenantPackageGetListOutputDto, Guid,
        TenantPackageGetListInputVo, TenantPackageCreateInputVo, TenantPackageUpdateInputVo>
    {
        /// <summary>
        /// 获取套餐关联的菜单ID列表
        /// </summary>
        /// <param name="packageId">套餐ID</param>
        /// <returns>菜单ID列表</returns>
        Task<List<Guid>> GetMenuIdsByPackageIdAsync(Guid packageId);

        /// <summary>
        /// 获取套餐菜单树
        /// </summary>
        /// <param name="packageId">套餐ID，空Guid表示新增模式</param>
        Task<MenuTreeResultDto> GetMenuTreeAsync(Guid? packageId);
    }
}
