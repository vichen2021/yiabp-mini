using Volo.Abp.Application.Services;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.Rbac.Application.Contracts.Dtos.Menu;
using Yi.Framework.Rbac.Domain.Shared.Dtos;

namespace Yi.Framework.Rbac.Application.Contracts.IServices
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
    }
}
