using Microsoft.AspNetCore.Mvc;
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
        /// 获取角色菜单树
        /// </summary>
        /// <param name="roleId">角色ID</param>
        /// <returns>角色菜单树数据，包含已选中的菜单ID和菜单树结构</returns>
        Task<ActionResult> GetRoleMenuTreeAsync(Guid roleId);

        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <returns>菜单树结构列表</returns>
        Task<List<MenuTreeDto>> GetTreeAsync();
    }
}
