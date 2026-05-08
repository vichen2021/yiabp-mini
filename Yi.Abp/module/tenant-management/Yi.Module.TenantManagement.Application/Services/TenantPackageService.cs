using Mapster;
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Yi.Framework.Ddd.Application;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.TenantManagement.Application.Contracts.Dtos.TenantPackage;
using Yi.Module.TenantManagement.Application.Contracts.IServices;
using Yi.Module.TenantManagement.Domain;
using Yi.Module.TenantManagement.Domain.Entities;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.OperationRecord.Abstractions.Attributes;

namespace Yi.Module.TenantManagement.Application.Services
{
    /// <summary>
    /// 租户套餐服务实现
    /// </summary>
    [PermissionResource("system", "tenantPackage")]
    [OperLogEntity("租户套餐")]
    public class TenantPackageService : YiCrudAppService<TenantPackageAggregateRoot, TenantPackageGetOutputDto, TenantPackageGetListOutputDto, Guid,
        TenantPackageGetListInputVo, TenantPackageCreateInputVo, TenantPackageUpdateInputVo>, ITenantPackageService
    {
        private readonly ISqlSugarRepository<TenantPackageAggregateRoot, Guid> _repository;
        private readonly ISqlSugarRepository<TenantPackageMenuEntity> _tenantPackageMenuRepository;
        private readonly ISqlSugarRepository<TenantAggregateRoot, Guid> _tenantRepository;
        private readonly ISqlSugarRepository<MenuAggregateRoot, Guid> _menuRepository;

        public TenantPackageService(
            ISqlSugarRepository<TenantPackageAggregateRoot, Guid> repository,
            ISqlSugarRepository<TenantPackageMenuEntity> tenantPackageMenuRepository,
            ISqlSugarRepository<TenantAggregateRoot, Guid> tenantRepository,
            ISqlSugarRepository<MenuAggregateRoot, Guid> menuRepository) : base(repository) =>
            (_repository, _tenantPackageMenuRepository, _tenantRepository, _menuRepository) =
            (repository, tenantPackageMenuRepository, tenantRepository, menuRepository);

        public override async Task<PagedResultDto<TenantPackageGetListOutputDto>> GetListAsync(TenantPackageGetListInputVo input)
        {
            RefAsync<int> total = 0;
            var output = await _repository._DbQueryable
                .WhereIF(!string.IsNullOrEmpty(input.PackageName), x => x.PackageName.Contains(input.PackageName!))
                .WhereIF(input.State is not null, x => x.State == input.State)
                .WhereIF(input.StartTime is not null && input.EndTime is not null,
                    x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)
                .OrderByDescending(x => x.CreationTime)
                .Select(x => new TenantPackageGetListOutputDto
                {
                    Id = x.Id,
                    PackageName = x.PackageName,
                    MenuCheckStrictly = x.MenuCheckStrictly,
                    State = x.State,
                    OrderNum = x.OrderNum,
                    CreationTime = x.CreationTime,
                    CreatorId = x.CreatorId
                })
                .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);

            return new PagedResultDto<TenantPackageGetListOutputDto>(total, output);
        }

        public override async Task<TenantPackageGetOutputDto> GetAsync(Guid id)
        {
            var entity = await _repository.GetAsync(id);
            var dto = entity.Adapt<TenantPackageGetOutputDto>();

            // 查询套餐关联的菜单ID
            var menuIds = await _tenantPackageMenuRepository._DbQueryable
                .Where(x => x.PackageId == entity.Id)
                .Select(x => x.MenuId)
                .ToListAsync();

            dto.MenuIds = menuIds;

            return dto;
        }

        public override async Task<TenantPackageGetOutputDto> CreateAsync(TenantPackageCreateInputVo input)
        {
            await CheckCreateInputDtoAsync(input);

            var entity = input.Adapt<TenantPackageAggregateRoot>();
            entity = await _repository.InsertReturnEntityAsync(entity);

            // 处理套餐菜单关联
            if (input.MenuIds != null && input.MenuIds.Count > 0)
            {
                var packageMenus = input.MenuIds.Select(menuId => new TenantPackageMenuEntity
                {
                    PackageId = entity.Id,
                    MenuId = menuId
                }).ToList();
                await _tenantPackageMenuRepository.InsertRangeAsync(packageMenus);
            }

            return entity.Adapt<TenantPackageGetOutputDto>();
        }

        public override async Task<TenantPackageGetOutputDto> UpdateAsync(Guid id, TenantPackageUpdateInputVo input)
        {
            var entity = await _repository.GetAsync(id);
            await CheckUpdateInputDtoAsync(entity, input);

            input.Adapt(entity);
            await _repository.UpdateAsync(entity);

            // 先删除旧的关联
            await _tenantPackageMenuRepository.DeleteAsync(x => x.PackageId == id);

            // 重新插入新的关联
            if (input.MenuIds != null && input.MenuIds.Count > 0)
            {
                var packageMenus = input.MenuIds.Select(menuId => new TenantPackageMenuEntity
                {
                    PackageId = id,
                    MenuId = menuId
                }).ToList();
                await _tenantPackageMenuRepository.InsertRangeAsync(packageMenus);
            }

            return entity.Adapt<TenantPackageGetOutputDto>();
        }

        /// <summary>
        /// 批量删除 - 检查租户引用保护
        /// </summary>
        public override async Task DeleteAsync(IEnumerable<Guid> ids)
        {
            // 检查是否有租户引用该套餐
            var hasReference = await _tenantRepository._DbQueryable
                .Where(x => x.PackageId.HasValue && ids.Contains(x.PackageId!.Value))
                .AnyAsync();

            if (hasReference)
            {
                throw new UserFriendlyException("租户套餐已被使用，无法删除");
            }

            // 删除套餐关联的菜单
            await _tenantPackageMenuRepository.DeleteAsync(x => ids.Contains(x.PackageId));

            // 删除套餐
            await base.DeleteAsync(ids);
        }

        protected override async Task CheckCreateInputDtoAsync(TenantPackageCreateInputVo input)
        {
            if (!string.IsNullOrEmpty(input.PackageName))
            {
                var isExist = await _repository.IsAnyAsync(x => x.PackageName == input.PackageName);
                if (isExist)
                {
                    throw new UserFriendlyException("套餐名称已存在");
                }
            }
        }

        protected override async Task CheckUpdateInputDtoAsync(TenantPackageAggregateRoot entity, TenantPackageUpdateInputVo input)
        {
            if (!string.IsNullOrEmpty(input.PackageName))
            {
                var isExist = await _repository._DbQueryable.Where(x => x.Id != entity.Id)
                    .AnyAsync(x => x.PackageName == input.PackageName);
                if (isExist)
                {
                    throw new UserFriendlyException("套餐名称已存在");
                }
            }
        }

        /// <summary>
        /// 租户套餐下拉列表（重写基类方法，只查询启用的套餐）
        /// </summary>
        public override async Task<List<TenantPackageGetListOutputDto>> GetSelectDataListAsync(string? keywords = null)
        {
            var query = _repository._DbQueryable.Where(x => x.State == true)
                .WhereIF(!string.IsNullOrEmpty(keywords), x => x.PackageName.Contains(keywords!))
                .Select(x => new TenantPackageGetListOutputDto
                {
                    Id = x.Id,
                    PackageName = x.PackageName,
                    MenuCheckStrictly = x.MenuCheckStrictly,
                });

            return await query.ToListAsync();
        }

        /// <summary>
        /// 获取套餐关联的菜单ID列表
        /// </summary>
        /// <param name="packageId">套餐ID</param>
        /// <returns>菜单ID列表</returns>
        [PermissionAction("query")]
        public async Task<List<Guid>> GetMenuIdsByPackageIdAsync(Guid packageId)
        {
            return await _tenantPackageMenuRepository._DbQueryable
                .Where(x => x.PackageId == packageId)
                .Select(x => x.MenuId)
                .ToListAsync();
        }

        /// <summary>
        /// 获取套餐菜单树
        /// </summary>
        /// <param name="packageId">套餐ID，空Guid表示新增模式</param>
        [PermissionAction("query")]
        public async Task<MenuTreeResultDto> GetMenuTreeAsync(Guid? packageId)
        {
            var result = new MenuTreeResultDto();

            // 查询全部菜单（排除租户管理菜单）
            var menus = await _menuRepository._DbQueryable
                .Where(x => x.MenuName != "租户管理")
                .ToListAsync();

            result.Menus = menus.TreeDtoBuild();

            // 如果是编辑模式，查询套餐已选的菜单
            if (packageId != Guid.Empty)
            {
                var checkedKeys = await _tenantPackageMenuRepository._DbQueryable
                    .Where(x => x.PackageId == packageId)
                    .Select(x => x.MenuId)
                    .ToListAsync();
                result.CheckedKeys = checkedKeys;
            }

            return result;
        }
    }
}
