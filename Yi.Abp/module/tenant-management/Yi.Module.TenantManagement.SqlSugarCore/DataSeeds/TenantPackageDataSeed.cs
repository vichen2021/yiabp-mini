using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.TenantManagement.Domain.Entities;

namespace Yi.Module.TenantManagement.SqlSugarCore.DataSeeds;

public class TenantPackageDataSeed : IDataSeedContributor, ITransientDependency
{
    public const string DefaultPackageName = "标准套餐";

    private static readonly HashSet<string> ExcludedMenuNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "接口文档",
        "租户管理",
        "租户套餐",
    };

    private readonly ISqlSugarRepository<MenuAggregateRoot> _menuRepository;
    private readonly ISqlSugarRepository<TenantPackageAggregateRoot> _packageRepository;
    private readonly ISqlSugarRepository<TenantPackageMenuEntity> _packageMenuRepository;
    private readonly ISqlSugarRepository<TenantAggregateRoot, Guid> _tenantRepository;

    public TenantPackageDataSeed(
        ISqlSugarRepository<TenantPackageAggregateRoot> packageRepository,
        ISqlSugarRepository<TenantPackageMenuEntity> packageMenuRepository,
        ISqlSugarRepository<MenuAggregateRoot> menuRepository,
        ISqlSugarRepository<TenantAggregateRoot, Guid> tenantRepository)
    {
        _packageRepository = packageRepository;
        _packageMenuRepository = packageMenuRepository;
        _menuRepository = menuRepository;
        _tenantRepository = tenantRepository;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        if (context.TenantId is not null)
        {
            return;
        }

        var package = await _packageRepository.GetFirstAsync(x => x.PackageName == DefaultPackageName);
        if (package is null)
        {
            package = new TenantPackageAggregateRoot
            {
                PackageName = DefaultPackageName,
                MenuCheckStrictly = true,
                OrderNum = 999,
                Remark = "默认租户套餐，包含除租户管理、租户套餐、接口文档外的全部菜单",
                State = true,
                IsDeleted = false,
            };
            await _packageRepository.InsertAsync(package);
            package = await _packageRepository.GetFirstAsync(x => x.PackageName == DefaultPackageName);
        }

        if (package is null)
        {
            return;
        }

        await BindDefaultTenantAsync(package.Id);

        var menuIds = await GetDefaultPackageMenuIdsAsync();
        await _packageMenuRepository.DeleteAsync(x => x.PackageId == package.Id);
        if (menuIds.Count == 0)
        {
            return;
        }

        var packageMenus = menuIds.Select(menuId => new TenantPackageMenuEntity
        {
            PackageId = package.Id,
            MenuId = menuId,
        }).ToList();
        await _packageMenuRepository.InsertRangeAsync(packageMenus);
    }

    private async Task BindDefaultTenantAsync(Guid packageId)
    {
        var tenant = await _tenantRepository.GetFirstAsync(x => x.Name == TenantDataSeed.DefaultTenantName);
        if (tenant is null || tenant.PackageId == packageId)
        {
            return;
        }

        tenant.PackageId = packageId;
        await _tenantRepository.UpdateAsync(tenant);
    }

    private async Task<List<Guid>> GetDefaultPackageMenuIdsAsync()
    {
        var menus = await _menuRepository._DbQueryable
            .Where(x => x.IsDeleted == false)
            .ToListAsync();
        if (menus.Count == 0)
        {
            return [];
        }

        var childrenByParentId = menus
            .GroupBy(x => x.ParentId)
            .ToDictionary(x => x.Key, x => x.ToList());
        var excludedIds = new HashSet<Guid>();
        foreach (var menu in menus.Where(x => ExcludedMenuNames.Contains(x.MenuName)))
        {
            CollectMenuAndChildren(menu.Id, childrenByParentId, excludedIds);
        }

        return menus
            .Where(x => !excludedIds.Contains(x.Id))
            .Select(x => x.Id)
            .Distinct()
            .ToList();
    }

    private static void CollectMenuAndChildren(
        Guid menuId,
        Dictionary<Guid, List<MenuAggregateRoot>> childrenByParentId,
        HashSet<Guid> result)
    {
        if (!result.Add(menuId))
        {
            return;
        }

        if (!childrenByParentId.TryGetValue(menuId, out var children))
        {
            return;
        }

        foreach (var child in children)
        {
            CollectMenuAndChildren(child.Id, childrenByParentId, result);
        }
    }
}
