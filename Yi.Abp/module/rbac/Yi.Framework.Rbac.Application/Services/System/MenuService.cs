using SqlSugar;
using Volo.Abp.Application.Dtos;
using Yi.Framework.Ddd.Application;
using Yi.Framework.Rbac.Application.Contracts.Dtos.Menu;
using Yi.Framework.Rbac.Application.Contracts.IServices;
using Yi.Framework.Rbac.Domain.Entities;
using Yi.Framework.Rbac.Domain.Shared.Consts;
using Yi.Framework.SqlSugarCore.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Yi.Framework.Rbac.Domain.Shared.Dtos;

namespace Yi.Framework.Rbac.Application.Services.System
{
    /// <summary>
    /// Menu服务实现
    /// </summary>
    public class MenuService : YiCrudAppService<MenuAggregateRoot, MenuGetOutputDto, MenuGetListOutputDto, Guid, MenuGetListInputVo, MenuCreateInputVo, MenuUpdateInputVo>,
       IMenuService
    {
        private readonly ISqlSugarRepository<MenuAggregateRoot, Guid> _repository;
        public MenuService(ISqlSugarRepository<MenuAggregateRoot, Guid> repository) : base(repository)
        {
            _repository = repository;
        }

        [Route("menu/list")]
        public async Task<List<MenuGetListOutputDto>> GetListAsync(MenuGetListInputVo input)
        {
            var entities = await _repository._DbQueryable.WhereIF(!string.IsNullOrEmpty(input.MenuName), x => x.MenuName.Contains(input.MenuName!))
                        .WhereIF(input.State is not null, x => x.State == input.State)
                        .Where(x=>x.MenuSource==input.MenuSource)
                        .OrderByDescending(x => x.OrderNum)
                        .ToListAsync();
            return await MapToGetListOutputDtosAsync(entities);
        }
        /// <summary>
        /// 获取角色菜单树
        /// </summary>
        /// <param name="roleId"></param>
        /// <returns></returns>
        public async Task<ActionResult> GetRoleMenuTree(Guid roleId)
        {
            // 如果是超管返回全部id
            if (CurrentUser.UserName.Equals(UserConst.Admin) || CurrentUser.Roles.Any(f => f.Equals(UserConst.AdminRolesCode))) 
            {
                var menuList = await _repository._DbQueryable.ToListAsync();
                return new JsonResult(new
                {
                    checkedKeys = menuList.Select(x => x.Id),
                    menus = menuList.TreeDtoBuild()
                });
            }
            var checkedKeys = await _repository._DbQueryable
                .Where(m => SqlFunc.Subqueryable<RoleMenuEntity>().Where(rm => rm.RoleId == roleId && rm.MenuId == m.Id).Any())
                .Select(x => x.Id).ToListAsync();
            var roles = await _repository._DbQueryable.ToListAsync();
            var menus = roles.TreeDtoBuild();
            return new JsonResult(new
            {
                checkedKeys,
                menus
            });
        }
        
        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <returns></returns>
        public async Task<List<MenuTreeDto>> GetTree()
        {
            var menuList = await _repository._DbQueryable.ToListAsync();
            return menuList.TreeDtoBuild();
        }
        
        
    }
}
