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
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Authorization.Abstractions.Enums;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Managers;
using Yi.Module.Rbac.Domain.Shared.Consts;
using Yi.Framework.SqlSugarCore.Abstractions;
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
        private ISqlSugarRepository<TenantAggregateRoot, Guid> _repository;
        private IDataSeeder _dataSeeder;
        private readonly DbConnOptions _dbConnOptions;
        private readonly SqlSugarAndConfigurationTenantStore _tenantStore;
        private readonly UserManager _userManager;
        private readonly ISqlSugarRepository<TenantPackageMenuEntity> _tenantPackageMenuRepository;
        private readonly ISqlSugarRepository<RoleAggregateRoot, Guid> _roleRepository;
        private readonly ISqlSugarRepository<RoleMenuEntity> _roleMenuRepository;
        private readonly ISqlSugarRepository<MenuAggregateRoot, Guid> _menuRepository;

        public TenantService(ISqlSugarRepository<TenantAggregateRoot, Guid> repository, IDataSeeder dataSeeder,
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
            var entites = await _repository._DbQueryable.ToListAsync();
            return entites.Select(x => new TenantSelectOutputDto { Id = x.Id, Name = x.Name }).ToList();
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

            var result = await base.UpdateAsync(id, input);

            await _tenantStore.RemoveCacheAsync(id, oldTenant.Name);

            return result;
        }


        /// <summary>
        /// 租户删除
        /// </summary>
        /// <param name="id"></param>
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
                    ISqlSugarClient db = await _repository.GetDbContextAsync();
                    try
                    {
                        var dbs = db.DbMaintenance.GetDataBaseList();
                        databaseExists = dbs.Any(x => x?.ToString() == tenant.Name);
                    }
                    catch
                    {
                        databaseExists = false;
                    }

                    if (databaseExists)
                    {
                        var tables = db.DbMaintenance.GetTableInfoList();
                        tableCount = tables.Count;
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
            // 查询套餐关联的宿主菜单，并映射为租户本地菜单。不同租户初始化时菜单 ID 会重新生成，不能跨库复用宿主菜单 ID。
            var packageMenuIds = await _tenantPackageMenuRepository._DbQueryable
                .Where(x => x.PackageId == packageId)
                .Select(x => x.MenuId)
                .ToListAsync();

            if (packageMenuIds.Count == 0)
            {
                throw new UserFriendlyException("同步失败，租户套餐未配置任何菜单");
            }

            var packageMenus = await _menuRepository._DbQueryable
                .Where(x => packageMenuIds.Contains(x.Id))
                .ToListAsync();

            if (packageMenus.Count == 0)
            {
                throw new UserFriendlyException("同步失败，租户套餐菜单不存在");
            }

            // 在租户上下文中查询角色并更新菜单关联
            using (CurrentTenant.Change(tenantId))
            {
                var tenantMenus = await _menuRepository._DbQueryable.ToListAsync();
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
                        // 删除该角色的所有菜单关联
                        await _roleMenuRepository.DeleteAsync(x => x.RoleId == role.Id);

                        // 插入套餐的菜单
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
                ISqlSugarClient db = await _repository.GetDbContextAsync();
                db.DbMaintenance.CreateDatabase(databaseName);

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
                    db.CodeFirst.InitTables(types.ToArray());
                }

                await uow.CompleteAsync();
            }
        }
    }
}
