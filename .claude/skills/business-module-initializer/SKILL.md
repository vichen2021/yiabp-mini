---
name: business-module-initializer
description: Initialize complete business module scaffolding for both backend (C# .NET with ABP framework) and frontend (Vue3 + Vben5 + Ant Design Vue). Creates entity classes, DTOs, service interfaces, service implementations, repositories, API files, and view components following the project's established patterns. Use when user needs to create a new business feature like department management, user management, or any CRUD module that requires full-stack implementation.
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
5. Repository interface in `Domain/Repositories/` (if custom methods needed)
6. Repository implementation in `SqlSugarCore/Repositories/` (if custom methods needed)

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
using Yi.Framework.{ModuleName}.Domain.Repositories;

namespace Yi.Framework.{ModuleName}.Application.Services
{
    /// <summary>
    /// {EntityName}服务实现
    /// </summary>
    public class {EntityName}Service : YiCrudAppService<{EntityName}AggregateRoot, {EntityName}GetOutputDto, {EntityName}GetListOutputDto, Guid,
        {EntityName}GetListInputVo, {EntityName}CreateInputVo, {EntityName}UpdateInputVo>, I{EntityName}Service
    {
        private I{EntityName}Repository _repository;

        public {EntityName}Service(I{EntityName}Repository repository) : base(repository)
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

#### 2.5 Create Repository (if custom methods needed)

**Interface:** `src/WebApi/module/{module-name}/Yi.Framework.{ModuleName}.Domain/Repositories/I{EntityName}Repository.cs`
```csharp
using Yi.Framework.{ModuleName}.Domain.Entities;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.{ModuleName}.Domain.Repositories
{
    public interface I{EntityName}Repository : ISqlSugarRepository<{EntityName}AggregateRoot, Guid>
    {
        // Add custom repository methods if needed
    }
}
```

**Implementation:** `src/WebApi/module/{module-name}/Yi.Framework.{ModuleName}.SqlSugarCore/Repositories/{EntityName}Repository.cs`
```csharp
using Volo.Abp.DependencyInjection;
using Yi.Framework.{ModuleName}.Domain.Entities;
using Yi.Framework.{ModuleName}.Domain.Repositories;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.SqlSugarCore.Repositories;

namespace Yi.Framework.{ModuleName}.SqlSugarCore.Repositories
{
    public class {EntityName}Repository : SqlSugarRepository<{EntityName}AggregateRoot, Guid>, I{EntityName}Repository, ITransientDependency
    {
        public {EntityName}Repository(ISugarDbContextProvider<ISqlSugarDbContext> sugarDbContextProvider) : base(sugarDbContextProvider)
        {
        }

        // Implement custom methods if needed
    }
}
```

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

**Key patterns from dept example:**
- Tree structure support with `treeConfig` in grid
- Parent-child relationships with TreeSelect
- Form validation and error handling
- Loading states and user feedback

## Naming Conventions

- **Entity name**: PascalCase (e.g., `Dept`, `User`, `Product`)
- **File names**: Match entity name (e.g., `DeptAggregateRoot.cs`, `DeptService.cs`)
- **API paths**: kebab-case (e.g., `/dept/list`, `/user/info`)
- **Frontend directories**: kebab-case (e.g., `api/system/dept/`, `views/system/dept/`)
- **TypeScript functions**: camelCase (e.g., `deptList`, `deptInfo`)

## Common Patterns

### Tree Structure Entities

If the entity has parent-child relationships (like Department):
- Add `ParentId` property (Guid, default to `Guid.Empty` for root)
- Add `Children` property (List<EntityType>?) for tree building
- Use `TreeHelper.SetTree()` for backend tree building
- Use `listToTree()` and `addFullName()` for frontend tree display
- Implement tree-specific methods in service (e.g., `GetTreeAsync()`, `GetListExcludeAsync()`)

### Validation

- Override `CheckCreateInputDtoAsync()` for create validation
- Override `CheckUpdateInputDtoAsync()` for update validation
- Check for duplicates, required fields, business rules

### Custom Queries

- Override `GetListAsync()` if you need joins or complex filtering
- Use SqlSugar's `LeftJoin`, `WhereIF`, `Select` for custom queries
- Map joined data to DTO properties (e.g., `LeaderName` from User table)

## Examples

Reference the `Dept` (Department) module implementation:
- Backend: `src/WebApi/module/rbac/Yi.Framework.Rbac.Domain/Entities/DeptAggregateRoot.cs`
- DTOs: `src/WebApi/module/rbac/Yi.Framework.Rbac.Application.Contracts/Dtos/Dept/`
- Service: `src/WebApi/module/rbac/Yi.Framework.Rbac.Application/Services/System/DeptService.cs`
- Frontend API: `src/Admin/apps/web-antd/src/api/system/dept/`
- Frontend Views: `src/Admin/apps/web-antd/src/views/system/dept/`

## Resources

See `references/backend-patterns.md` for detailed backend code patterns.
See `references/frontend-patterns.md` for detailed frontend code patterns.
See `assets/` for code templates if needed.
