using SqlSugar;
using Volo.Abp.Application.Dtos;
using Yi.Framework.Ddd.Application;
using Yi.Module.Rbac.Application.Contracts.Dtos.Menu;
using Yi.Module.Rbac.Application.Contracts.IServices;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Enums;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Framework.SqlSugarCore.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Yi.Module.Rbac.Domain.Shared.Dtos;
using Yi.Module.TenantManagement.Application.Contracts.IServices;
using Yi.Module.TenantManagement.Application.Contracts.Dtos.TenantPackage;

namespace Yi.Module.Rbac.Application.Services
{
    /// <summary>
    /// Menu服务实现
    /// </summary>
    [PermissionResource("system", "menu")]
    [OperLogEntity("菜单")]
    public class MenuService : YiCrudAppService<MenuAggregateRoot, MenuGetOutputDto, MenuGetListOutputDto, Guid, MenuGetListInputVo, MenuCreateInputVo, MenuUpdateInputVo>,
       IMenuService
    {
        private readonly ISqlSugarRepository<MenuAggregateRoot, Guid> _repository;
        private readonly ITenantPackageService _tenantPackageService;

        public MenuService(
            ISqlSugarRepository<MenuAggregateRoot, Guid> repository,
            ITenantPackageService tenantPackageService) : base(repository) =>
            (_repository, _tenantPackageService) = (repository, tenantPackageService);

        [Route("menu/list")]
        [PermissionAction(PermissionActionEnum.Query)]
        public new async Task<List<MenuGetListOutputDto>> GetListAsync(MenuGetListInputVo input)
        {
            var entities = await _repository._DbQueryable.WhereIF(!string.IsNullOrEmpty(input.MenuName), x => x.MenuName.Contains(input.MenuName!))
                        .WhereIF(input.State is not null, x => x.State == input.State)
                        .Where(x=>x.MenuSource==input.MenuSource)
                        .OrderByDescending(x => x.OrderNum)
                        .ToListAsync();
            return await MapToGetListOutputDtosAsync(entities);
        }

        /// <summary>
        /// 获取菜单树
        /// </summary>
        /// <returns></returns>
        [PermissionAction(PermissionActionEnum.Query)]
        public async Task<List<MenuTreeDto>> GetTreeAsync()
        {
            var menuList = await _repository._DbQueryable.ToListAsync();
            return menuList.TreeDtoBuild();
        }

        /// <summary>
        /// 获取租户套餐菜单树
        /// </summary>
        /// <param name="packageId">套餐ID，空Guid表示新增模式</param>
        /// <returns>包含 CheckedKeys 和 Menus 的结果</returns>
        [PermissionAction(PermissionActionEnum.Query)]
        public async Task<MenuTreeResultDto> GetTenantPackageMenuTreeAsync(Guid? packageId)
        {
            return await _tenantPackageService.GetMenuTreeAsync(packageId);
        }
    }
}
