---
name: business-module-initializer
description: Initialize complete business module scaffolding for both backend (C# .NET with ABP framework) and frontend (Vue3 + Vben5 + Ant Design Vue). Creates entity classes, DTOs, service interfaces, service implementations, menu seed data, API files, and view components following the project's established patterns. Use when user needs to create a new business feature or CRUD module that requires full-stack implementation, such as (1) Creating a new entity with complete backend and frontend, (2) Adding a new business module to an existing system, (3) Generating scaffolding for department management, user management, product management, or similar business entities, (4) Setting up menu permissions and seed data for a new module. IMPORTANT: This skill should be used when the user explicitly requests to generate or scaffold a complete module, not for simple file creation or minor modifications.
---

# Business Module Initializer

Generates complete business modules following the project's ABP + SqlSugar + Vue3 + Vben5 architecture.

## Quick Checklist

When creating a module, you need:

**Backend (Cms.Abp/module/{module-name}):**
- [ ] Entity: `Domain/Entities/{EntityName}AggregateRoot.cs`
- [ ] DTOs (5 files): `Application.Contracts/Dtos/{EntityName}/`
- [ ] Service Interface: `Application.Contracts/IServices/I{EntityName}Service.cs`
- [ ] Service Implementation: `Application/Services/{EntityName}Service.cs`
- [ ] **Menu Seed Data**: `module/rbac/.../DataSeeds/MenuDataSeed/{ModuleName}MenuDataSeed.cs` ⚠️ CRITICAL
- [ ] Entity Seed Data (optional): `SqlSugarCore/DataSeeds/{EntityName}DataSeed.cs`

**Frontend (Cms.Vben5/apps/web-antd/src):**
- [ ] API Model: `api/{module-name}/{entity-name}/model.d.ts`
- [ ] API Functions: `api/{module-name}/{entity-name}/index.ts`
- [ ] View Index: `views/{module-name}/{entity-name}/index.vue`
- [ ] View Data: `views/{module-name}/{entity-name}/data.ts`
- [ ] View Drawer: `views/{module-name}/{entity-name}/{entity-name}-drawer.vue`

**Verification:**
- [ ] Run `dotnet build` in Cms.Abp directory
- [ ] Fix any compilation errors
- [ ] Document the module in `.docs/`

## Workflow

### Step 1: Understand Requirements

Ask the user to clarify:
- Entity name and properties
- Module name (if different from entity)
- Whether to add to existing module menu or create new module menu
- Special requirements (tree structure, relationships, etc.)

### Step 2: Create Backend Files

#### 2.1 Entity Class

Location: `Cms.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Domain/Entities/{EntityName}AggregateRoot.cs`

See `references/backend-patterns.md` for full template. Key points:
- Inherit `AggregateRoot<Guid>`
- Implement: `ISoftDelete`, `IAuditedObject`, `IOrderNum`, `IState`
- Use `[SugarTable]` and `[SugarColumn]` attributes

#### 2.2 DTO Classes (5 files)

Location: `Cms.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application.Contracts/Dtos/{EntityName}/`

Create:
1. `{EntityName}GetOutputDto.cs` - Single entity retrieval
2. `{EntityName}GetListOutputDto.cs` - List queries
3. `{EntityName}GetListInputVo.cs` - Query parameters (inherit `PagedAllResultRequestDto`)
4. `{EntityName}CreateInputVo.cs` - Creation
5. `{EntityName}UpdateInputVo.cs` - Updates

See `references/backend-patterns.md` for patterns.

#### 2.3 Service Interface

Location: `Cms.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application.Contracts/IServices/I{EntityName}Service.cs`

Inherit `IYiCrudAppService<...>` with all DTO types.

#### 2.4 Service Implementation

Location: `Cms.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application/Services/{EntityName}Service.cs`

Inherit `YiCrudAppService<...>` and implement interface. Override `GetListAsync()` for custom queries.

See `references/backend-patterns.md` for full pattern.

#### 2.5 Menu Seed Data ⚠️ CRITICAL - DO NOT SKIP

**IMPORTANT**: Menu seed data is REQUIRED for the module to appear in the UI and for permissions to work.

**Decision Point**: Ask user whether to:
- **Option A**: Add to existing module menu (e.g., add AppNav to existing APP配置 menu)
- **Option B**: Create new module menu file

**Option A - Update Existing Menu File:**

Location: `Cms.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed/{ExistingModule}MenuDataSeed.cs`

Add menu items for the new entity to the existing `GetSeedData()` method. See `references/menu-seed-patterns.md` for examples.

**Option B - Create New Module Menu File:**

Location: `Cms.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed/{ModuleName}MenuDataSeed.cs`

Create complete menu structure with:
- Top-level catalogue menu
- Entity menu with list permission
- CRUD permission menus (query, add, edit, remove)

See `references/menu-seed-patterns.md` for full template.

**Permission Code Format**: `{module-name}:{entity-name}:{action}`
- Example: `app:app-nav:list`, `app:app-nav:add`, `app:app-nav:edit`, `app:app-nav:remove`

### Step 3: Create Frontend Files

#### 3.1 API Files

Location: `Cms.Vben5/apps/web-antd/src/api/{module-name}/{entity-name}/`

Create:
- `model.d.ts` - TypeScript interface
- `index.ts` - API functions (list, info, add, update, remove)

See `references/frontend-patterns.md` for patterns.

#### 3.2 View Files

Location: `Cms.Vben5/apps/web-antd/src/views/{module-name}/{entity-name}/`

Create:
- `index.vue` - Main list view with table and CRUD operations
- `data.ts` - Form schemas (querySchema, columns, drawerSchema)
- `{entity-name}-drawer.vue` - Create/Edit drawer component

See `references/frontend-patterns.md` for patterns.

### Step 4: Build Verification ⚠️ REQUIRED

After creating all files:

1. Run `dotnet build` in `Cms.Abp` directory
2. If build fails:
   - Read error messages carefully
   - Fix compilation errors (common: wrong base class, missing using statements)
   - Re-run build until successful
3. Document the module in `.docs/{ModuleName}模块开发文档.md`

## Common Patterns

### Tree Structure Entities
- Add `ParentId` property (Guid, default `Guid.Empty`)
- Add `Children` property for tree building
- Use `TreeHelper.SetTree()` in backend
- Use `treeConfig` in frontend grid

### Custom Queries
- Override `GetListAsync()` for joins or complex filtering
- Use SqlSugar's `LeftJoin`, `WhereIF`, `Select`

### Validation
- Override `CheckCreateInputDtoAsync()` and `CheckUpdateInputDtoAsync()`

## Naming Conventions

- **Entity**: PascalCase (e.g., `AppNav`, `Dept`)
- **Files**: Match entity name (e.g., `AppNavAggregateRoot.cs`)
- **API paths**: kebab-case (e.g., `/app-nav/list`)
- **Frontend dirs**: kebab-case (e.g., `api/app/app-nav/`)
- **Functions**: camelCase (e.g., `appNavList`, `appNavInfo`)

## Reference Files

- `references/backend-patterns.md` - Entity, DTO, and service templates
- `references/menu-seed-patterns.md` - Menu seed data examples
- `references/frontend-patterns.md` - Frontend API and view templates
- `references/troubleshooting.md` - Common issues and solutions

## Examples

Reference existing modules:
- **AppChannel**: `Cms.Abp/module/app/` and `Cms.Vben5/.../views/app/app-channel/`
- **Config**: `Cms.Abp/module/rbac/` and `Cms.Vben5/.../views/system/config/`
