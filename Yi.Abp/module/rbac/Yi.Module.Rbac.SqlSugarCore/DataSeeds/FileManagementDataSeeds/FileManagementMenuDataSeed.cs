using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Shared.Enums;

namespace Yi.Module.Rbac.SqlSugarCore.DataSeeds.FileManagementDataSeeds;

public class FileManagementMenuDataSeed : ITransientDependency
{
    private readonly IGuidGenerator _guidGenerator;

    public FileManagementMenuDataSeed(IGuidGenerator guidGenerator)
    {
        _guidGenerator = guidGenerator;
    }

    public List<MenuAggregateRoot> GetSeedData(Guid parentId)
    {
        var entities = new List<MenuAggregateRoot>();

        MenuAggregateRoot file = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "文件管理",
            PermissionCode = "system:file:query",
            MenuType = MenuTypeEnum.Menu,
            Router = "file",
            IsShow = true,
            IsLink = false,
            IsCache = true,
            Component = "system/file/index",
            MenuIcon = "tabler:file",
            OrderNum = 100,
            ParentId = parentId,
            IsDeleted = false
        };
        entities.Add(file);

        MenuAggregateRoot fileQuery = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "文件查询",
            PermissionCode = "system:file:query",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 100,
            ParentId = file.Id,
            IsDeleted = false
        };
        entities.Add(fileQuery);

        MenuAggregateRoot fileAdd = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "文件新增",
            PermissionCode = "system:file:add",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 100,
            ParentId = file.Id,
            IsDeleted = false
        };
        entities.Add(fileAdd);

        MenuAggregateRoot fileRemove = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "文件删除",
            PermissionCode = "system:file:remove",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 100,
            ParentId = file.Id,
            IsDeleted = false
        };
        entities.Add(fileRemove);

        MenuAggregateRoot ossSettingsQuery = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "OSS设置查询",
            PermissionCode = "system:tenantOSSSettings:query",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 100,
            ParentId = file.Id,
            IsDeleted = false
        };
        entities.Add(ossSettingsQuery);

        MenuAggregateRoot ossSettingsEdit = new MenuAggregateRoot(_guidGenerator.Create())
        {
            MenuName = "OSS设置编辑",
            PermissionCode = "system:tenantOSSSettings:edit",
            MenuType = MenuTypeEnum.Component,
            OrderNum = 100,
            ParentId = file.Id,
            IsDeleted = false
        };
        entities.Add(ossSettingsEdit);

        return entities;
    }
}
