using System.Linq;
using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Data;
using Volo.Abp.Modularity;
using Volo.Abp.Uow;
using Yi.Framework.Ddd.Application;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Enums;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Managers;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Yi.Module.TenantManagement.Application.Contracts;
using Yi.Module.TenantManagement.Application.Contracts.Dtos;
using Yi.Module.TenantManagement.Domain;
using Yi.Module.TenantManagement.Domain.Entities;

namespace Yi.Module.TenantManagement.Application
{
    /// <summary>
    /// 租户管理
    /// </summary>
    [PermissionResource("system", "tenant")]
    [OperLogEntity("租户")]
    public class TenantService :
        YiCrudAppService<TenantAggregateRoot, TenantGetOutputDto, TenantGetListOutputDto, Guid, TenantGetListInput,
            TenantCreateInput, TenantUpdateInput>, ITenantService
    {
        private ISqlSugarTenantRepository _repository;
        private IDataSeeder _dataSeeder;
        private readonly DbConnOptions _dbConnOptions;
        private readonly SqlSugarAndConfigurationTenantStore _tenantStore;
        private readonly UserManager _userManager;
        private readonly ISqlSugarRepository<TenantPackageMenuEntity> _tenantPackageMenuRepository;
        private readonly ISqlSugarRepository<RoleAggregateRoot, Guid> _roleRepository;
        private readonly ISqlSugarRepository<RoleMenuEntity> _roleMenuRepository;
        private readonly ISqlSugarRepository<MenuAggregateRoot, Guid> _menuRepository;

        public TenantService(ISqlSugarTenantRepository repository, IDataSeeder dataSeeder,
            IOptions<DbConnOptions> dbConnOptions, SqlSugarAndConfigurationTenantStore tenantStore, UserManager userManager,
            ISqlSugarRepository<TenantPackageMenuEntity> tenantPackageMenuRepository,
            ISqlSugarRepository<RoleAggregateRoot, Guid> roleRepository,
            ISqlSugarRepository<RoleMenuEntity> roleMenuRepository,
            ISqlSugarRepository<MenuAggregateRoot, Guid> menuRepository) :
            base(repository)
        {
            _repository = repository;
            _dataSeeder = dataSeeder;
            _dbConnOptions = dbConnOptions.Value;
            _tenantStore = tenantStore;
            _userManager = userManager;
            _tenantPackageMenuRepository = tenantPackageMenuRepository;
            _roleRepository = roleRepository;
            _roleMenuRepository = roleMenuRepository;
            _menuRepository = menuRepository;
        }

        /// <summary>
        /// 租户单查
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public override Task<TenantGetOutputDto> GetAsync(Guid id)
        {
            return base.GetAsync(id);
        }

        /// <summary>
        /// 租户多查
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<PagedResultDto<TenantGetListOutputDto>> GetListAsync(TenantGetListInput input)
        {
            RefAsync<int> total = 0;

            var entities = await _repository._DbQueryable
                .WhereIF(!string.IsNullOrEmpty(input.Name), x => x.Name.Contains(input.Name!))
                .WhereIF(!string.IsNullOrEmpty(input.ContactUserName), x => x.ContactUserName!.Contains(input.ContactUserName!))
                .WhereIF(!string.IsNullOrEmpty(input.ContactPhone), x => x.ContactPhone!.Contains(input.ContactPhone!))
                .WhereIF(input.StartTime is not null && input.EndTime is not null,
                    x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)
                .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
            return new PagedResultDto<TenantGetListOutputDto>(total, await MapToGetListOutputDtosAsync(entities));
        }

        /// <summary>
        /// 租户选项
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        public async Task<List<TenantSelectOutputDto>> GetSelectAsync()
        {
            using (CurrentTenant.Change(null))
            {
                var entites = await _repository._DbQueryable.ToListAsync();
                return entites.Select(x => new TenantSelectOutputDto { Id = x.Id, Name = x.Name }).ToList();
            }
        }


        /// <summary>
        /// 创建租户
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<TenantGetOutputDto> CreateAsync(TenantCreateInput input)
        {
            if (!_dbConnOptions.EnabledSaasMultiTenancy)
            {
                throw new UserFriendlyException("创建失败，系统未开启多租户功能，请在配置文件中启用");
            }

            if (await _repository.IsAnyAsync(x => x.Name == input.Name))
            {
                throw new UserFriendlyException("创建失败，当前租户已存在");
            }

            return await base.CreateAsync(input);
        }

        /// <summary>
        /// 更新租户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        public override async Task<TenantGetOutputDto> UpdateAsync(Guid id, TenantUpdateInput input)
        {
            var oldTenant = await _repository.GetByIdAsync(id);

            if (oldTenant.Name != input.Name && await _repository.IsAnyAsync(x => x.Name == input.Name))
            {
                throw new UserFriendlyException("租户名已经存在");
            }

            var oldPackageId = oldTenant.PackageId;

            var result = await base.UpdateAsync(id, input);

            await _tenantStore.RemoveCacheAsync(id, oldTenant.Name);

            if (input.PackageId.HasValue && input.PackageId != oldPackageId)
            {
                await SyncPackageAsync(id, input.PackageId.Value);
            }

            return result;
        }


        /// <summary>
        /// 租户删除
        /// </summary>
        /// <param name="ids"></param>
        /// <returns></returns>
        public override Task DeleteAsync(IEnumerable<Guid> ids)
        {
            return base.DeleteAsync(ids);
        }


        /// <summary>
        /// 初始化租户
        /// </summary>
        /// <param name="id"></param>
        /// <param name="input">初始化参数（包含管理员账号密码）</param>
        /// <returns></returns>
        [HttpPut("tenant/init/{id}")]
        [PermissionAction(PermissionActionEnum.Edit)]
        [OperLog("初始化租户", Yi.Framework.OperationRecord.Abstractions.Enums.OperEnum.Update)]
        public async Task<TenantInitOutputDto> InitAsync
            ([FromRoute] Guid id, [FromBody] TenantInitInput input)
        {
            var tenant = await _repository.GetByIdAsync(id);
            if (tenant is null)
            {
                throw new UserFriendlyException("租户不存在");
            }

            await CurrentUnitOfWork.SaveChangesAsync();

            bool databaseExists = false;
            int tableCount = 0;

            // 检查数据库状态（在租户上下文中）
            using (CurrentTenant.Change(id))
            {
                using (var checkUow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
                {
                    databaseExists = await _repository.DatabaseExistsAsync(tenant.Name);
                    if (databaseExists)
                    {
                        tableCount = await _repository.GetTableCountAsync();
                    }
                    await checkUow.CompleteAsync();
                }
            }

            // 如果数据库有数据且不是强制初始化，返回需要确认
            if (databaseExists && tableCount > 0 && !input.IsForce)
            {
                return new TenantInitOutputDto { NeedForce = true };
            }

            // 执行初始化（在新的租户上下文中）
            using (CurrentTenant.Change(id))
            {
                await CodeFirst(this.LazyServiceProvider, tenant.Name);
                await _dataSeeder.SeedAsync(id);

                // 创建租户管理员账号
                if (!string.IsNullOrEmpty(input.Username) && !string.IsNullOrEmpty(input.Password))
                {
                    await _userManager.CreateOrUpdateAdminAsync(input.Username, input.Password, "租户管理员");
                }
            }

            // 同步套餐菜单到租户（如果租户绑定了套餐）
            if (tenant.PackageId.HasValue)
            {
                await SyncPackageAsync(id, tenant.PackageId.Value);
            }
            else
            {
                await SyncAllTenantMenusToAdminRoleAsync(id);
            }

            return new TenantInitOutputDto { NeedForce = false };
        }

        /// <summary>
        /// 同步套餐菜单到租户
        /// </summary>
        /// <param name="tenantId">租户ID</param>
        /// <param name="packageId">套餐ID</param>
        [PermissionAction(PermissionActionEnum.Edit)]
        [OperLog("同步租户套餐", Yi.Framework.OperationRecord.Abstractions.Enums.OperEnum.Update)]
        public async Task SyncPackageAsync(Guid tenantId, Guid packageId)
        {
            // 查询套餐关联的宿主菜单 ID
            var packageMenuIds = await _tenantPackageMenuRepository._DbQueryable
                .Where(x => x.PackageId == packageId)
                .Select(x => x.MenuId)
                .ToListAsync();

            if (packageMenuIds.Count == 0)
            {
                throw new UserFriendlyException("同步失败，租户套餐未配置任何菜单");
            }

            // 获取宿主全量菜单（用于父级链查找）
            var allHostMenus = await _menuRepository._DbQueryable.ToListAsync();
            var allHostMenuMap = allHostMenus.ToDictionary(x => x.Id);

            var packageMenus = allHostMenus.Where(x => packageMenuIds.Contains(x.Id)).ToList();
            if (packageMenus.Count == 0)
            {
                throw new UserFriendlyException("同步失败，租户套餐菜单不存在");
            }

            // 套餐菜单 + 所有祖先节点
            var requiredHostMenuIds = CollectMenuIdsWithAncestors(packageMenuIds, allHostMenuMap);
            var requiredHostMenus = allHostMenus.Where(x => requiredHostMenuIds.Contains(x.Id)).ToList();

            // 在租户上下文中补全缺失菜单，再更新角色菜单关联
            using (CurrentTenant.Change(tenantId))
            {
                var tenantMenus = await _menuRepository._DbQueryable.ToListAsync();

                // 补全宿主有而租户缺失的菜单
                await EnsureMenusExistInTenantAsync(requiredHostMenus, allHostMenuMap, tenantMenus);

                // 补全后重新加载租户菜单
                tenantMenus = await _menuRepository._DbQueryable.ToListAsync();

                var tenantMenuIds = ResolveTenantMenuIds(packageMenus, tenantMenus);

                if (tenantMenuIds.Count == 0)
                {
                    throw new UserFriendlyException("同步失败，租户套餐未匹配到任何租户菜单");
                }

                // 查询租户下的所有角色
                var roles = await _roleRepository._DbQueryable.ToListAsync();

                foreach (var role in roles)
                {
                    // 管理员角色：全量替换
                    if (role.RoleCode == UserConst.AdminRolesCode)
                    {
                        await _roleMenuRepository.DeleteAsync(x => x.RoleId == role.Id);

                        var roleMenus = tenantMenuIds.Select(menuId => new RoleMenuEntity
                        {
                            RoleId = role.Id,
                            MenuId = menuId
                        }).ToList();

                        if (roleMenus.Count > 0)
                        {
                            await _roleMenuRepository.InsertRangeAsync(roleMenus);
                        }
                    }
                    else
                    {
                        // 其他角色：裁剪不在套餐中的菜单
                        await _roleMenuRepository._Db.Deleteable<RoleMenuEntity>()
                            .Where(x => x.RoleId == role.Id)
                            .Where(x => !tenantMenuIds.Contains(x.MenuId))
                            .ExecuteCommandAsync();
                    }
                }
            }
        }

        /// <summary>
        /// 收集指定菜单 ID 及其所有祖先 ID
        /// </summary>
        private static HashSet<Guid> CollectMenuIdsWithAncestors(
            List<Guid> menuIds,
            Dictionary<Guid, MenuAggregateRoot> allMenuMap)
        {
            var result = new HashSet<Guid>(menuIds);
            foreach (var id in menuIds)
            {
                var current = id;
                while (allMenuMap.TryGetValue(current, out var menu) && menu.ParentId != Guid.Empty)
                {
                    if (!result.Add(menu.ParentId)) break;
                    current = menu.ParentId;
                }
            }
            return result;
        }

        /// <summary>
        /// 将宿主有而租户缺失的菜单按拓扑顺序插入租户库（需在租户上下文中调用）
        /// </summary>
        private async Task EnsureMenusExistInTenantAsync(
            List<MenuAggregateRoot> requiredHostMenus,
            Dictionary<Guid, MenuAggregateRoot> allHostMenuMap,
            List<MenuAggregateRoot> currentTenantMenus)
        {
            var tenantByPermCode = currentTenantMenus
                .Where(x => !string.IsNullOrWhiteSpace(x.PermissionCode))
                .GroupBy(x => x.PermissionCode!, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);
            var tenantByStableKey = currentTenantMenus
                .Where(x => string.IsNullOrWhiteSpace(x.PermissionCode))
                .GroupBy(GetMenuStableKey, StringComparer.OrdinalIgnoreCase)
                .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

            // 找出租户缺失的宿主菜单
            var missingHostMenus = requiredHostMenus.Where(m =>
                string.IsNullOrWhiteSpace(m.PermissionCode)
                    ? !tenantByStableKey.ContainsKey(GetMenuStableKey(m))
                    : !tenantByPermCode.ContainsKey(m.PermissionCode!)).ToList();

            if (missingHostMenus.Count == 0) return;

            // 拓扑排序：父节点先于子节点
            var sortedMissing = TopologicalSort(missingHostMenus, allHostMenuMap);

            // 建立 宿主ID → 租户ID 映射（含已存在的菜单）
            var hostToTenantId = new Dictionary<Guid, Guid>();
            foreach (var tenantMenu in currentTenantMenus)
            {
                var matched = requiredHostMenus.FirstOrDefault(m =>
                    !string.IsNullOrWhiteSpace(m.PermissionCode)
                        ? string.Equals(m.PermissionCode, tenantMenu.PermissionCode, StringComparison.OrdinalIgnoreCase)
                        : GetMenuStableKey(m) == GetMenuStableKey(tenantMenu));
                if (matched != null)
                    hostToTenantId[matched.Id] = tenantMenu.Id;
            }

            var toInsert = new List<MenuAggregateRoot>();
            foreach (var hostMenu in sortedMissing)
            {
                var tenantParentId = Guid.Empty;
                if (hostMenu.ParentId != Guid.Empty)
                    hostToTenantId.TryGetValue(hostMenu.ParentId, out tenantParentId);

                var newId = Guid.NewGuid();
                var newMenu = new MenuAggregateRoot(newId, tenantParentId)
                {
                    MenuName = hostMenu.MenuName,
                    RouterName = hostMenu.RouterName,
                    MenuType = hostMenu.MenuType,
                    PermissionCode = hostMenu.PermissionCode,
                    MenuIcon = hostMenu.MenuIcon,
                    Router = hostMenu.Router,
                    IsLink = hostMenu.IsLink,
                    IsCache = hostMenu.IsCache,
                    IsShow = hostMenu.IsShow,
                    Remark = hostMenu.Remark,
                    Component = hostMenu.Component,
                    MenuSource = hostMenu.MenuSource,
                    Query = hostMenu.Query,
                    OrderNum = hostMenu.OrderNum,
                    State = hostMenu.State,
                };
                toInsert.Add(newMenu);
                hostToTenantId[hostMenu.Id] = newId;
            }

            if (toInsert.Count > 0)
                await _menuRepository.InsertRangeAsync(toInsert);
        }

        /// <summary>
        /// 对待插入菜单做拓扑排序，确保父节点先于子节点
        /// </summary>
        private static List<MenuAggregateRoot> TopologicalSort(
            List<MenuAggregateRoot> menus,
            Dictionary<Guid, MenuAggregateRoot> allMenuMap)
        {
            var ids = menus.Select(x => x.Id).ToHashSet();
            var result = new List<MenuAggregateRoot>();
            var visited = new HashSet<Guid>();

            void Visit(MenuAggregateRoot menu)
            {
                if (!visited.Add(menu.Id)) return;
                if (menu.ParentId != Guid.Empty && ids.Contains(menu.ParentId)
                    && allMenuMap.TryGetValue(menu.ParentId, out var parent))
                    Visit(parent);
                result.Add(menu);
            }

            foreach (var menu in menus)
                Visit(menu);

            return result;
        }

        private async Task SyncAllTenantMenusToAdminRoleAsync(Guid tenantId)
        {
            using (CurrentTenant.Change(tenantId))
            {
                var adminRole = await _roleRepository.GetFirstAsync(x => x.RoleCode == UserConst.AdminRolesCode);
                if (adminRole is null)
                {
                    throw new UserFriendlyException($"角色不存在：{UserConst.AdminRolesCode}");
                }

                var menuIds = await _menuRepository._DbQueryable.Select(x => x.Id).ToListAsync();
                await _roleMenuRepository.DeleteAsync(x => x.RoleId == adminRole.Id);

                var roleMenus = menuIds.Select(menuId => new RoleMenuEntity
                {
                    RoleId = adminRole.Id,
                    MenuId = menuId
                }).ToList();

                if (roleMenus.Count > 0)
                {
                    await _roleMenuRepository.InsertRangeAsync(roleMenus);
                }
            }
        }

        private static List<Guid> ResolveTenantMenuIds(List<MenuAggregateRoot> packageMenus, List<MenuAggregateRoot> tenantMenus)
        {
            var selectedIds = new HashSet<Guid>();
            var packagePermissionCodes = packageMenus
                .Select(x => x.PermissionCode)
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x!)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var packageMenuKeys = packageMenus
                .Where(x => string.IsNullOrWhiteSpace(x.PermissionCode))
                .Select(GetMenuStableKey)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var tenantMenu in tenantMenus)
            {
                if (!string.IsNullOrWhiteSpace(tenantMenu.PermissionCode) &&
                    packagePermissionCodes.Contains(tenantMenu.PermissionCode))
                {
                    selectedIds.Add(tenantMenu.Id);
                    continue;
                }

                if (string.IsNullOrWhiteSpace(tenantMenu.PermissionCode) &&
                    packageMenuKeys.Contains(GetMenuStableKey(tenantMenu)))
                {
                    selectedIds.Add(tenantMenu.Id);
                }
            }

            var menuMap = tenantMenus.ToDictionary(x => x.Id);
            foreach (var menuId in selectedIds.ToList())
            {
                AddParentMenuIds(menuId, menuMap, selectedIds);
            }

            return selectedIds.ToList();
        }

        private static void AddParentMenuIds(Guid menuId, Dictionary<Guid, MenuAggregateRoot> menuMap, HashSet<Guid> selectedIds)
        {
            var currentId = menuId;
            while (menuMap.TryGetValue(currentId, out var menu) && menu.ParentId != Guid.Empty)
            {
                if (!menuMap.ContainsKey(menu.ParentId))
                {
                    break;
                }

                if (!selectedIds.Add(menu.ParentId))
                {
                    currentId = menu.ParentId;
                    continue;
                }

                currentId = menu.ParentId;
            }
        }

        private static string GetMenuStableKey(MenuAggregateRoot menu)
        {
            return $"{menu.MenuType}|{menu.MenuName}|{menu.Router}|{menu.Component}";
        }

        private async Task CodeFirst(IServiceProvider service, string databaseName)
        {
            var moduleContainer = service.GetRequiredService<IModuleContainer>();

            //没有数据库，不能创工作单元，创建库，先关闭
            using (var uow = UnitOfWorkManager.Begin(requiresNew: true, isTransactional: false))
            {
                await _repository.CreateDatabaseAsync(databaseName);

                List<Type> types = new List<Type>();
                foreach (var module in moduleContainer.Modules)
                {
                    types.AddRange(module.Assembly.GetTypes()
                        .Where(x => x.GetCustomAttribute<IgnoreCodeFirstAttribute>() == null)
                        .Where(x => x.GetCustomAttribute<SugarTable>() != null)
                        .Where(x => x.GetCustomAttribute<DefaultTenantTableAttribute>() is null)
                        .Where(x => x.GetCustomAttribute<SplitTableAttribute>() is null));
                }

                if (types.Count > 0)
                {
                    await _repository.InitTablesAsync(types.ToArray());
                }

                await uow.CompleteAsync();
            }
        }
    }
}
