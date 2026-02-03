# Menu Seed Data Patterns

This document provides detailed patterns for creating menu seed data files.

## File Structure

Each module should have its own menu seed data file located at:
`src/WebApi/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed/{ModuleName}MenuDataSeed.cs`

## Basic Structure

```csharp
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Yi.Framework.Rbac.Domain.Entities;
using Yi.Framework.Rbac.Domain.Shared.Enums;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.Rbac.SqlSugarCore.DataSeeds
{
    public class {ModuleName}MenuDataSeed : IDataSeedContributor, ITransientDependency
    {
        private ISqlSugarRepository<MenuAggregateRoot> _repository;
        private IGuidGenerator _guidGenerator;

        public {ModuleName}MenuDataSeed(ISqlSugarRepository<MenuAggregateRoot> repository, IGuidGenerator guidGenerator)
        {
            _repository = repository;
            _guidGenerator = guidGenerator;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (!await _repository.IsAnyAsync(x => x.MenuName == "{Module Display Name}"))
            {
                await _repository.InsertManyAsync(GetSeedData());
            }
        }

        public List<MenuAggregateRoot> GetSeedData()
        {
            // Implementation here
        }
    }
}
```

## Menu Types

### Top-Level Catalogue Menu

```csharp
MenuAggregateRoot {moduleName} = new MenuAggregateRoot(_guidGenerator.Create(), Guid.Empty)
{
    MenuName = "{Module Display Name}",
    MenuType = MenuTypeEnum.Catalogue,
    Router = "/{module-name}",
    IsShow = true,
    IsLink = false,
    MenuIcon = "{icon-name}",
    OrderNum = 95,
    IsDeleted = false
};
```

**Key properties:**
- `ParentId = Guid.Empty` - Top-level menu
- `MenuType = MenuTypeEnum.Catalogue` - Catalogue type for grouping
- `Router` - Route path (e.g., "/video", "/system")
- `OrderNum` - Display order (lower numbers appear first)

### Entity Menu (List Page)

```csharp
MenuAggregateRoot {entityName} = new MenuAggregateRoot(_guidGenerator.Create(), {moduleName}.Id)
{
    MenuName = "{Entity Display Name}",
    PermissionCode = "{module-name}:{entity-name}:list",
    MenuType = MenuTypeEnum.Menu,
    Router = "{entity-name}",
    IsShow = true,
    IsLink = false,
    IsCache = true,
    Component = "{module-name}/{entity-name}/index",
    MenuIcon = "{icon-name}",
    OrderNum = 100,
    IsDeleted = false
};
```

**Key properties:**
- `ParentId = {moduleName}.Id` - Child of top-level menu
- `MenuType = MenuTypeEnum.Menu` - Menu type for navigation
- `PermissionCode` - Permission code for access control (format: `{module-name}:{entity-name}:list`)
- `Component` - Frontend component path (e.g., "video/vod/index")

### Permission Components (CRUD Operations)

#### Query Permission

```csharp
MenuAggregateRoot {entityName}Query = new MenuAggregateRoot(_guidGenerator.Create())
{
    MenuName = "{Entity Display Name}查询",
    PermissionCode = "{module-name}:{entity-name}:query",
    MenuType = MenuTypeEnum.Component,
    OrderNum = 100,
    ParentId = {entityName}.Id,
    IsDeleted = false
};
```

#### Add Permission

```csharp
MenuAggregateRoot {entityName}Add = new MenuAggregateRoot(_guidGenerator.Create())
{
    MenuName = "{Entity Display Name}新增",
    PermissionCode = "{module-name}:{entity-name}:add",
    MenuType = MenuTypeEnum.Component,
    OrderNum = 100,
    ParentId = {entityName}.Id,
    IsDeleted = false
};
```

#### Edit Permission

```csharp
MenuAggregateRoot {entityName}Edit = new MenuAggregateRoot(_guidGenerator.Create())
{
    MenuName = "{Entity Display Name}修改",
    PermissionCode = "{module-name}:{entity-name}:edit",
    MenuType = MenuTypeEnum.Component,
    OrderNum = 100,
    ParentId = {entityName}.Id,
    IsDeleted = false
};
```

#### Remove Permission

```csharp
MenuAggregateRoot {entityName}Remove = new MenuAggregateRoot(_guidGenerator.Create())
{
    MenuName = "{Entity Display Name}删除",
    PermissionCode = "{module-name}:{entity-name}:remove",
    MenuType = MenuTypeEnum.Component,
    OrderNum = 100,
    ParentId = {entityName}.Id,
    IsDeleted = false
};
```

## Default Properties

At the end of `GetSeedData()`, set default properties for all menus:

```csharp
entities.ForEach(m =>
{
    m.IsDeleted = false;
    m.State = true;
    m.MenuSource = MenuSourceEnum.Ruoyi;
    m.IsShow = true;
});
```

## Permission Code Format

Use consistent permission code format:
- List: `{module-name}:{entity-name}:list`
- Query: `{module-name}:{entity-name}:query`
- Add: `{module-name}:{entity-name}:add`
- Edit: `{module-name}:{entity-name}:edit`
- Remove: `{module-name}:{entity-name}:remove`

**Examples:**
- `video:vod:list` - Video list permission
- `video:category:add` - Category add permission
- `system:user:edit` - User edit permission

## Menu Icons

Use icon names from the icon library (e.g., Material Symbols, Tabler Icons):
- `material-symbols:video-library-outline` - Video library icon
- `material-symbols:category-outline` - Category icon
- `material-symbols:movie-outline` - Movie icon
- `eos-icons:system-group` - System group icon
- `tabler:code` - Code icon

## Order Numbers

- Top-level menus: Use values like 90-100 (lower = appears first)
- Entity menus: Use 100 as default
- Permission components: Use 100 as default

## Examples

Reference the Video module menu seed:
- File: `src/WebApi/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed/VideoMenuDataSeed.cs`
- Contains: Video management catalogue, Category menu, Vod menu, and their CRUD permissions

## Notes

- Each module should have exactly one menu seed data file
- The file name should follow the pattern: `{ModuleName}MenuDataSeed.cs`
- Check for existing menu in `SeedAsync` to avoid duplicates
- All menus should be added to the `entities` list before returning

