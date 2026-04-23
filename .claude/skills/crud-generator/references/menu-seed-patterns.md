# Menu and Dictionary Seed Data Patterns

This document provides detailed patterns for creating menu and dictionary seed data.

## Important Note

**Yi.Abp project uses a SINGLE file for all menu and dictionary seed data, NOT separate files per entity.**

- Menu Seed File: `Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed.cs`
- Dictionary Seed File: `Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryDataSeed.cs`
- Dictionary Type Seed File: `Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryTypeDataSeed.cs`

When adding a new entity, you need to **edit the existing files** to add new entries in the `GetSeedData()` method.

## Menu Seed Data

### File Structure

Location: `Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed.cs`

```csharp
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Guids;
using Yi.Framework.Rbac.Domain.Entities;
using Yi.Framework.Rbac.Domain.Shared.Enums;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.Rbac.SqlSugarCore.DataSeeds
{
    public class MenuDataSeed : IDataSeedContributor, ITransientDependency
    {
        private ISqlSugarRepository<MenuAggregateRoot> _repository;
        private IGuidGenerator _guidGenerator;

        public MenuDataSeed(ISqlSugarRepository<MenuAggregateRoot> repository, IGuidGenerator guidGenerator)
        {
            _repository = repository;
            _guidGenerator = guidGenerator;
        }

        public async Task SeedAsync(DataSeedContext context)
        {
            if (!await _repository.IsAnyAsync(x => x.MenuName == "系统管理"&&x.MenuSource==MenuSourceEnum.Ruoyi))
            {
                await _repository.InsertManyAsync(GetSeedData());
            }
        }

        public List<MenuAggregateRoot> GetSeedData()
        {
            List<MenuAggregateRoot> entities = new List<MenuAggregateRoot>();
            // Add your menu entries here...
            return entities;
        }
    }
}
```

### Menu Types

#### Top-Level Catalogue Menu

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
entities.Add({moduleName});
```

**Key properties:**
- `ParentId = Guid.Empty` - Top-level menu
- `MenuType = MenuTypeEnum.Catalogue` - Catalogue type for grouping
- `Router` - Route path (e.g., "/video", "/system")
- `OrderNum` - Display order (lower numbers appear first)

#### Entity Menu (List Page)

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
entities.Add({entityName});
```

**Key properties:**
- `ParentId = {moduleName}.Id` - Child of top-level menu
- `MenuType = MenuTypeEnum.Menu` - Menu type for navigation
- `PermissionCode` - Permission code for access control (format: `{module-name}:{entity-name}:list`)
- `Component` - Frontend component path (e.g., "video/vod/index")

### Permission Components (CRUD Operations)

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
entities.Add({entityName}Add);
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
entities.Add({entityName}Edit);
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
entities.Add({entityName}Remove);
```

### Menu Icons

Use icon names from the icon library (e.g., Material Symbols, Tabler Icons):
- `eos-icons:system-group` - System icon
- `material-symbols:list-alt-outline` - List icon
- `material-symbols:category-outline` - Category icon
- `tabler:code` - Code icon

### Icon Derivation Rules

When creating menu, derive icon from entity name:
- *User* → user-outlined
- *Role* → team-outlined
- *Order* → shopping-cart-outlined
- *Log* → file-text-outlined
- *Config* → setting-outlined
- *Category* → appstore-outlined
- *Payment* → pay-circle-outlined
- *Product* → shopping-outlined
- Default → appstore-outlined

## Dictionary Seed Data

### Dictionary Type Seed

Location: `Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryTypeDataSeed.cs`

Add dictionary type first:

```csharp
DictionaryTypeAggregateRoot dictType{EnumName} = new DictionaryTypeAggregateRoot()
{
    DictName = "{Enum Display Name}",
    DictType = "{module}_{enum_lower}",
    OrderNum = 100,
    Remark = "Description",
    IsDeleted = false,
    State = true
};
entities.Add(dictType{EnumName});
```

### Dictionary Data Seed

Location: `Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryDataSeed.cs`

Add dictionary entries:

```csharp
DictionaryEntity dict{EnumName}Value0 = new DictionaryEntity()
{
    DictLabel = "Label 1",  // From [Description] attribute
    DictValue = "0",        // Enum int value as string
    DictType = "{module}_{enum_lower}",
    OrderNum = 100,
    Remark = "Description",
    IsDeleted = false,
    State = true,
    ListClass = "default"  // success, danger, warning, info, primary, default
};
entities.Add(dict{EnumName}Value0);

DictionaryEntity dict{EnumName}Value1 = new DictionaryEntity()
{
    DictLabel = "Label 2",
    DictValue = "1",
    DictType = "{module}_{enum_lower}",
    OrderNum = 99,
    Remark = "Description",
    IsDeleted = false,
    State = true,
    ListClass = "primary"
};
entities.Add(dict{EnumName}Value1);
```

## Frontend Dict Enum Update

Location: `Yi.Vben5/packages/@core/base/shared/src/constants/dict-enum.ts`

Add constant for new dictionary type:

```typescript
export const DictEnum = {
  // ... existing entries
  {MODULE}_{ENUM_UPPER}: '{module}_{enum_lower}',  // e.g., POKEMON_STATUS: 'pokemon_status'
} as const;
```

## Permission Code Format

Use consistent permission code format:
- List: `{module-name}:{entity-name}:list`
- Add: `{module-name}:{entity-name}:add`
- Edit: `{module-name}:{entity-name}:edit`
- Remove: `{module-name}:{entity-name}:remove`

**Examples:**
- `system:user:list` - 用户列表查询权限
- `system:user:add` - 用户新增权限
- `workflow:category:edit` - 流程分类修改权限

## Order Numbers

- Top-level menus: Use values like 90-100 (lower = appears first)
- Entity menus: Use 100 as default
- Permission components: Use 100 as default

## How to Add New Entity

When adding a new entity, follow these steps:

1. **Read the existing seed files**:
   - Read `MenuDataSeed.cs` to understand the current structure
   - Read `DictionaryDataSeed.cs` and `DictionaryTypeDataSeed.cs` if you have enums

2. **Edit the existing files**:
   - Add new menu entries at the end of `GetSeedData()` method in `MenuDataSeed.cs`
   - Add new dictionary types in `DictionaryTypeDataSeed.cs` (if enums exist)
   - Add new dictionary entries in `DictionaryDataSeed.cs` (if enums exist)

3. **Update frontend dict-enum.ts**:
   - Add new constant in `Yi.Vben5/packages/@core/base/shared/src/constants/dict-enum.ts`

## Notes

- Yi.Abp project uses a single `MenuDataSeed.cs` file for all menu entries
- Yi.Abp project uses a single `DictionaryDataSeed.cs` file for all dictionary entries
- Edit existing files, do NOT create separate files per entity
- Check for existing entries to avoid duplicates
- All menus should be added to the `entities` list before returning in `GetSeedData()`