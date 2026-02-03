---
name: business-module-initializer
description: Initialize complete business module scaffolding for both backend (C# .NET with ABP framework) and frontend (Vue3 + Vben5 + Ant Design Vue). Creates entity classes, DTOs, service interfaces, service implementations, menu seed data, API files, and view components following the project's established patterns. Use when user needs to create a new business feature or CRUD module that requires full-stack implementation, such as: (1) Creating a new entity with complete backend and frontend, (2) Adding a new business module to an existing system, (3) Generating scaffolding for department management, user management, product management, or similar business entities, (4) Setting up menu permissions and seed data for a new module.
---

# Business Module Initializer

This skill guides the creation of complete business modules following the project's established architecture patterns. It generates all necessary files for both backend (ABP framework with SqlSugar) and frontend (Vue3 + Vben5).

## Overview

When initializing a business module (e.g., "Department", "Product", "Order"), you need to create:

**Backend (@src/WebApi):**
1. Entity class in `Domain/Entities/`
2. DTO classes in `Application.Contracts/Dtos/{EntityName}/`
3. Service interface in `Application.Contracts/IServices/`
4. Service implementation in `Application/Services/`

**Frontend (@src/Admin/apps/web-antd):**
1. API files in `api/system/{entity-name}/`
2. View files in `views/system/{entity-name}/`

## Workflow

### Step 1: Understand the Entity Requirements

Before starting, clarify:
- Entity name (e.g., "Department", "Product", "Order")
- Required properties/fields
- Relationships with other entities
- Special business logic requirements

### Step 2: Backend Implementation

#### 2.1 Create Entity Class

Location: `src/WebApi/module/{module-name}/Yi.Framework.{ModuleName}.Domain/Entities/{EntityName}AggregateRoot.cs`

**Template pattern:**
```csharp
using SqlSugar;
using Volo.Abp;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Yi.Framework.Core.Data;

namespace Yi.Framework.{ModuleName}.Domain.Entities
{
    /// <summary>
    /// {Entity description}
    /// </summary>
    [SugarTable("{TableName}")]
    public class {EntityName}AggregateRoot : AggregateRoot<Guid>, ISoftDelete, IAuditedObject, IOrderNum, IState
    {
        public {EntityName}AggregateRoot()
        {
        }

        /// <summary>
        /// 主键
        /// </summary>
        [SugarColumn(IsPrimaryKey = true)]
        public override Guid Id { get; protected set; }

        /// <summary>
        /// 逻辑删除
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreationTime { get; set; } = DateTime.Now;

        /// <summary>
        /// 创建者
        /// </summary>
        public Guid? CreatorId { get; set; }

        /// <summary>
        /// 最后修改者
        /// </summary>
        public Guid? LastModifierId { get; set; }

        /// <summary>
        /// 最后修改时间
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int OrderNum { get; set; } = 0;

        /// <summary>
        /// 状态
        /// </summary>
        public bool State { get; set; } = true;

        // Add entity-specific properties here
    }
}
```

**Key points:**
- Inherit from `AggregateRoot<Guid>` for aggregate roots
- Implement interfaces: `ISoftDelete`, `IAuditedObject`, `IOrderNum`, `IState` (if needed)
- Use `[SugarTable("TableName")]` attribute for table mapping
- Use `[SugarColumn]` for column mapping when needed

#### 2.2 Create DTO Classes

Location: `src/WebApi/module/{module-name}/Yi.Framework.{ModuleName}.Application.Contracts/Dtos/{EntityName}/`

Create three DTO files:

**1. {EntityName}GetOutputDto.cs** - For single entity retrieval:
```csharp
using Volo.Abp.Application.Dtos;

namespace Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName}
{
    public class {EntityName}GetOutputDto : EntityDto<Guid>
    {
        public bool State { get; set; }
        // Add properties that match entity fields
    }
}
```

**2. {EntityName}GetListOutputDto.cs** - For list queries:
```csharp
using Volo.Abp.Application.Dtos;

namespace Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName}
{
    public class {EntityName}GetListOutputDto : EntityDto<Guid>
    {
        public DateTime CreationTime { get; set; } = DateTime.Now;
        public Guid? CreatorId { get; set; }
        public bool State { get; set; }
        // Add properties including any joined data (e.g., LeaderName from User table)
    }
}
```

**3. {EntityName}GetListInputVo.cs** - For query parameters:
```csharp
using Yi.Framework.Ddd;
using Yi.Framework.Ddd.Application.Contracts;

namespace Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName}
{
    public class {EntityName}GetListInputVo : PagedAllResultRequestDto
    {
        public Guid Id { get; set; }
        public bool? State { get; set; }
        // Add filter properties
    }
}
```

**4. {EntityName}CreateInputVo.cs** - For creation:
```csharp
namespace Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName}
{
    public class {EntityName}CreateInputVo
    {
        public bool State { get; set; }
        // Add required properties for creation
        public Guid? ParentId { get; set; } = Guid.Empty; // If tree structure
    }
}
```

**5. {EntityName}UpdateInputVo.cs** - For updates:
```csharp
namespace Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName}
{
    public class {EntityName}UpdateInputVo
    {
        public bool State { get; set; }
        // Add properties for update
        public Guid? ParentId { get; set; } = Guid.Empty; // If tree structure
    }
}
```

#### 2.3 Create Service Interface

Location: `src/WebApi/module/{module-name}/Yi.Framework.{ModuleName}.Application.Contracts/IServices/I{EntityName}Service.cs`

```csharp
using Volo.Abp.Application.Services;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName};

namespace Yi.Framework.{ModuleName}.Application.Contracts.IServices
{
    /// <summary>
    /// {EntityName}服务抽象
    /// </summary>
    public interface I{EntityName}Service : IYiCrudAppService<{EntityName}GetOutputDto, {EntityName}GetListOutputDto, Guid, {EntityName}GetListInputVo, {EntityName}CreateInputVo, {EntityName}UpdateInputVo>
    {
        // Add custom methods if needed
    }
}
```

#### 2.4 Create Service Implementation

Location: `src/WebApi/module/{module-name}/Yi.Framework.{ModuleName}.Application/Services/{EntityName}Service.cs`

```csharp
using Yi.Framework.Ddd.Application;
using Yi.Framework.{ModuleName}.Application.Contracts.Dtos.{EntityName};
using Yi.Framework.{ModuleName}.Application.Contracts.IServices;
using Yi.Framework.{ModuleName}.Domain.Entities;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.{ModuleName}.Application.Services
{
    /// <summary>
    /// {EntityName}服务实现
    /// </summary>
    public class {EntityName}Service : YiCrudAppService<{EntityName}AggregateRoot, {EntityName}GetOutputDto, {EntityName}GetListOutputDto, Guid,
        {EntityName}GetListInputVo, {EntityName}CreateInputVo, {EntityName}UpdateInputVo>, I{EntityName}Service
    {
        private ISqlSugarRepository<{EntityName}AggregateRoot, Guid> _repository;

        public {EntityName}Service(ISqlSugarRepository<{EntityName}AggregateRoot, Guid> repository) : base(repository)
        {
            _repository = repository;
        }

        // Override GetListAsync if custom query logic needed
        // Override CheckCreateInputDtoAsync for validation
        // Override CheckUpdateInputDtoAsync for validation
        // Add custom methods as needed
    }
}
```

#### 2.5 Create Menu Seed Data

After creating the service, you need to add menu seed data for the new module.

**Location:** `src/WebApi/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed/{ModuleName}MenuDataSeed.cs`

**Template pattern:**
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
            List<MenuAggregateRoot> entities = new List<MenuAggregateRoot>();

            // {Module Display Name}（顶级菜单，和系统管理平级）
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

            // {Entity Name}管理
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

            // {Entity Name}查询
            MenuAggregateRoot {entityName}Query = new MenuAggregateRoot(_guidGenerator.Create())
            {
                MenuName = "{Entity Display Name}查询",
                PermissionCode = "{module-name}:{entity-name}:query",
                MenuType = MenuTypeEnum.Component,
                OrderNum = 100,
                ParentId = {entityName}.Id,
                IsDeleted = false
            };
            entities.Add({entityName}Query);

            // {Entity Name}新增
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

            // {Entity Name}修改
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

            // {Entity Name}删除
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

            // 统一设置菜单属性
            entities.ForEach(m =>
            {
                m.IsDeleted = false;
                m.State = true;
                m.MenuSource = MenuSourceEnum.Ruoyi;
                m.IsShow = true;
            });

            return entities;
        }
    }
}
```

**Key points:**
- Each module should have its own menu seed data file (e.g., `VideoMenuDataSeed.cs` for video module)
- Create a top-level catalogue menu (ParentId = Guid.Empty)
- Create menu items for each entity with CRUD permissions (query, add, edit, remove)
- Use consistent permission code format: `{module-name}:{entity-name}:{action}`
- Set default properties at the end for all menus

**Reference example:**
- Video module: `src/WebApi/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed/VideoMenuDataSeed.cs`

### Step 3: Frontend Implementation

#### 3.1 Create API Files

Location: `src/Admin/apps/web-antd/src/api/system/{entity-name}/`

**index.ts** - API functions:
```typescript
import type { {EntityName} } from './model';

import type { ID } from '#/api/common';

import { requestClient } from '#/api/request';

enum Api {
  {entityName}List = '/{entity-name}/list',
  root = '/{entity-name}',
}

/**
 * {Entity name}列表
 * @returns list
 */
export function {entityName}List(params?: { /* filter params */ }) {
  return requestClient.get<{EntityName}[]>(Api.{entityName}List, { params });
}

/**
 * {Entity name}详情
 * @param {entityName}Id id
 * @returns {Entity name}信息
 */
export function {entityName}Info({entityName}Id: ID) {
  return requestClient.get<{EntityName}>(`${Api.root}/${{entityName}Id}`);
}

/**
 * {Entity name}新增
 * @param data 参数
 */
export function {entityName}Add(data: Partial<{EntityName}>) {
  return requestClient.postWithMsg<void>(Api.root, data);
}

/**
 * {Entity name}更新
 * @param data 参数
 */
export function {entityName}Update(data: Partial<{EntityName}>) {
  return requestClient.putWithMsg<void>(`${Api.root}/${data.id}`, data);
}

/**
 * {Entity name}删除
 * @param {entityName}Id ID
 * @returns void
 */
export function {entityName}Remove({entityName}Id: ID) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: {entityName}Id },
  });
}
```

**model.d.ts** - TypeScript interfaces:
```typescript
export interface {EntityName} {
  creationTime: string;
  creatorId?: string | null;
  state: boolean;
  // Add entity properties matching backend DTO
  id: string;
  children?: {EntityName}[]; // If tree structure
}
```

#### 3.2 Create View Files

Location: `src/Admin/apps/web-antd/src/views/system/{entity-name}/`

**index.vue** - Main list view:
- Uses `useVbenVxeGrid` for table
- Implements CRUD operations
- Handles tree structure if needed (like dept)

**data.ts** - Form schemas and table columns:
- `querySchema` - Search form schema
- `columns` - Table column definitions
- `drawerSchema` - Create/Edit form schema

**{entity-name}-drawer.vue** - Create/Edit drawer:
- Uses `useVbenForm` for form
- Handles create and update operations
- Manages form state and validation

**Key patterns from config example:**
- Pagination support with `pagerConfig` in grid
- Form validation and error handling
- Loading states and user feedback
- Date range filtering with `fieldMappingTime`

## Naming Conventions

- **Entity name**: PascalCase (e.g., `Dept`, `User`, `Product`)
- **File names**: Match entity name (e.g., `DeptAggregateRoot.cs`, `DeptService.cs`)
- **API paths**: kebab-case (e.g., `/dept/list`, `/user/info`)
- **Frontend directories**: kebab-case (e.g., `api/system/dept/`, `views/system/dept/`)
- **TypeScript functions**: camelCase (e.g., `deptList`, `deptInfo`)

## Common Patterns

### Tree Structure Entities

If the entity has parent-child relationships:
- Add `ParentId` property (Guid, default to `Guid.Empty` for root)
- Add `Children` property (List<EntityType>?) for tree building
- Use `TreeHelper.SetTree()` for backend tree building
- Use `listToTree()` and `addFullName()` for frontend tree display
- Implement tree-specific methods in service (e.g., `GetTreeAsync()`, `GetListExcludeAsync()`)
- Add `treeConfig` in frontend grid options

### Validation

- Override `CheckCreateInputDtoAsync()` for create validation
- Override `CheckUpdateInputDtoAsync()` for update validation
- Check for duplicates, required fields, business rules

### Custom Queries

- Override `GetListAsync()` if you need joins or complex filtering
- Use SqlSugar's `LeftJoin`, `WhereIF`, `Select` for custom queries
- Map joined data to DTO properties (e.g., `LeaderName` from User table)

## Examples

Reference the `Config` (Configuration) module implementation:
- Backend: `src/WebApi/module/rbac/Yi.Framework.Rbac.Domain/Entities/ConfigAggregateRoot.cs`
- DTOs: `src/WebApi/module/rbac/Yi.Framework.Rbac.Application.Contracts/Dtos/Config/`
- Service: `src/WebApi/module/rbac/Yi.Framework.Rbac.Application/Services/System/ConfigService.cs`
- Frontend API: `src/Admin/apps/web-antd/src/api/system/config/`
- Frontend Views: `src/Admin/apps/web-antd/src/views/system/config/`

## Resources

See `references/backend-patterns.md` for detailed backend code patterns.
See `references/frontend-patterns.md` for detailed frontend code patterns.
See `references/menu-seed-patterns.md` for menu seed data patterns.
See `assets/` for code templates if needed.
