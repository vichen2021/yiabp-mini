using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Shared.Enums;

namespace Yi.Module.Rbac.SqlSugarCore.DataSeeds.TenantDataSeeds;

public class TenantMenuDataSeed : ITransientDependency
{
    private readonly IGuidGenerator _guidGenerator;

    public TenantMenuDataSeed(IGuidGenerator guidGenerator)
    {
        _guidGenerator = guidGenerator;
    }

    public List<MenuAggregateRoot> GetSeedData(Guid systemMenuId)
    {
        var entities = new List<MenuAggregateRoot>();

        MenuAggregateRoot tenant = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "租户管理",
            PermissionCode = "system:tenant:query",
            MenuType = MenuTypeEnum.Menu,
            Router = "tenant",
            IsShow = true,
            IsLink = false,
            IsCache = true,
            Component = "system/tenant/index",
            MenuIcon = "tabler:users",
            OrderNum = 101,
            ParentId = systemMenuId,
            IsDeleted = false
        };
        entities.Add(tenant);

        MenuAggregateRoot tenantQuery = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "租户查询",
            PermissionCode = "system:tenant:query",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 100,
            ParentId = tenant.Id,
            IsDeleted = false
        };
        entities.Add(tenantQuery);

        MenuAggregateRoot tenantAdd = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "租户新增",
            PermissionCode = "system:tenant:add",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 100,
            ParentId = tenant.Id,
            IsDeleted = false
        };
        entities.Add(tenantAdd);

        MenuAggregateRoot tenantEdit = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "租户修改",
            PermissionCode = "system:tenant:edit",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 100,
            ParentId = tenant.Id,
            IsDeleted = false
        };
        entities.Add(tenantEdit);

        MenuAggregateRoot tenantRemove = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "租户删除",
            PermissionCode = "system:tenant:remove",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 100,
            ParentId = tenant.Id,
            IsDeleted = false
        };
        entities.Add(tenantRemove);

        MenuAggregateRoot tenantPackage = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "租户套餐",
            PermissionCode = "system:tenantPackage:query",
            MenuType = MenuTypeEnum.Menu,
            Router = "tenant-package",
            IsShow = true,
            IsLink = false,
            IsCache = true,
            Component = "system/tenant-package/index",
            MenuIcon = "tabler:package",
            OrderNum = 102,
            ParentId = systemMenuId,
            IsDeleted = false
        };
        entities.Add(tenantPackage);

        MenuAggregateRoot tenantPackageQuery = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "套餐查询",
            PermissionCode = "system:tenantPackage:query",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 100,
            ParentId = tenantPackage.Id,
            IsDeleted = false
        };
        entities.Add(tenantPackageQuery);

        MenuAggregateRoot tenantPackageAdd = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "套餐新增",
            PermissionCode = "system:tenantPackage:add",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 99,
            ParentId = tenantPackage.Id,
            IsDeleted = false
        };
        entities.Add(tenantPackageAdd);

        MenuAggregateRoot tenantPackageEdit = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "套餐修改",
            PermissionCode = "system:tenantPackage:edit",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 98,
            ParentId = tenantPackage.Id,
            IsDeleted = false
        };
        entities.Add(tenantPackageEdit);

        MenuAggregateRoot tenantPackageRemove = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "套餐删除",
            PermissionCode = "system:tenantPackage:remove",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 97,
            ParentId = tenantPackage.Id,
            IsDeleted = false
        };
        entities.Add(tenantPackageRemove);

        return entities;
    }
}