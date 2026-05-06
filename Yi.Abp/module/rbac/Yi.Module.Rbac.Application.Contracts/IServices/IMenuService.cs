using Volo.Abp.Application.Services;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Module.Rbac.Application.Contracts.Dtos.Menu;
using Yi.Module.Rbac.Domain.Shared.Dtos;
using Yi.Module.TenantManagement.Application.Contracts.Dtos.TenantPackage;

namespace Yi.Module.Rbac.Application.Contracts.IServices
{
    /// <summary>
    /// Menu服务抽象
    /// </summary>
    public interface IMenuService : IYiCrudAppService<MenuGetOutputDto, MenuGetListOutputDto, Guid, MenuGetListInputVo, MenuCreateInputVo, MenuUpdateInputVo>
    {

        /// <summary>
        /// 获取菜单列表
        /// </summary>
        /// <param name="input">查询参数</param>
        /// <returns>菜单列表</returns>
        Task<List<MenuGetListOutputDto>> GetListAsync(MenuGetListInputVo input);
        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <returns>菜单树结构列表</returns>
        Task<List<MenuTreeDto>> GetTreeAsync();
        /// <summary>
        /// 获取租户套餐菜单树
        /// </summary>
        /// <param name="packageId">套餐ID，空Guid表示新增模式</param>
        /// <returns>包含 CheckedKeys 和 Menus 的结果</returns>
        Task<MenuTreeResultDto> GetTenantPackageMenuTreeAsync(Guid? packageId);
    }
}