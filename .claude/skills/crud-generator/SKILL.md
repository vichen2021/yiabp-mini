---
name: crud-generator
description: Initialize complete business module scaffolding for both backend (C# .NET with ABP framework) and frontend (Vue3 + Vben5 + Ant Design Vue). Creates entity classes, DTOs, service interfaces, service implementations, menu seed data, API files, and view components following the project's established patterns. Use when user needs to create a new business feature or CRUD module that requires full-stack implementation, such as (1) Creating a new entity with complete backend and frontend, (2) Adding a new business module to an existing system, (3) Generating scaffolding for department management, user management, product management, or similar business entities, (4) Setting up menu permissions and seed data for a new module. IMPORTANT: This skill should be used when the user explicitly requests to generate or scaffold a complete module, not for simple file creation or minor modifications.
---

# Business Module Initializer

Generates complete business modules following the project's ABP + SqlSugar + Vue3 + Vben5 architecture.

## ⚠️ CRITICAL EXECUTION WARNING

**THIS SKILL REQUIRES FULL-STACK IMPLEMENTATION**

- ❌ **INCOMPLETE**: Creating only backend files
- ❌ **INCOMPLETE**: Creating only frontend files  
- ❌ **INCOMPLETE**: Skipping verification steps
- ✅ **COMPLETE**: Backend + Frontend + Verification all pass

**You MUST complete Steps 1-4 sequentially. Each step has verification checkpoints. Do NOT mark the task as done until ALL verification steps pass.**

## Quick Checklist

When creating a module, you need:

**Backend (Yi.Abp/module/{module-name}):** ⚠️ REQUIRED
- [ ] Entity: `Domain/Entities/{EntityName}AggregateRoot.cs`
- [ ] DTOs (5 files): `Application.Contracts/Dtos/{EntityName}/`
- [ ] Service Interface: `Application.Contracts/IServices/I{EntityName}Service.cs`
- [ ] Service Implementation: `Application/Services/{EntityName}Service.cs`
- [ ] **Menu Seed Data**: `module/rbac/.../DataSeeds/MenuDataSeed/{ModuleName}MenuDataSeed.cs` ⚠️ CRITICAL - DO NOT SKIP
- [ ] Entity Seed Data (optional): `SqlSugarCore/DataSeeds/{EntityName}DataSeed.cs`

**Frontend (Yi.Vben5/apps/web-antd/src):** ⚠️ REQUIRED - DO NOT SKIP
- [ ] API Model: `api/{module-name}/{entity-name}/model.d.ts` ⚠️ CRITICAL
- [ ] API Functions: `api/{module-name}/{entity-name}/index.ts` ⚠️ CRITICAL
- [ ] View Index: `views/{module-name}/{entity-name}/index.vue` ⚠️ CRITICAL
- [ ] View Data: `views/{module-name}/{entity-name}/data.ts` ⚠️ CRITICAL
- [ ] View Drawer: `views/{module-name}/{entity-name}/{entity-name}-drawer.vue` ⚠️ CRITICAL

**Verification:** ⚠️ REQUIRED - MUST PASS
- [ ] Backend: Run `dotnet build` in Yi.Abp directory - MUST SUCCEED
- [ ] Backend: Fix any compilation errors
- [ ] Frontend: Run TypeScript type check - MUST SUCCEED
- [ ] Frontend: Fix any type errors
- [ ] Frontend: Run lint check (recommended)
- [ ] Documentation: Document the module in `.docs/`（目录不存在则创建）

**Completion Check:** ⚠️ VERIFY BEFORE MARKING DONE
- [ ] All backend files created and verified
- [ ] All frontend files created and verified
- [ ] Backend build passes
- [ ] Frontend type check passes
- [ ] Documentation completed

## Execution Principles ⚠️ MANDATORY

**CRITICAL**: This workflow MUST be executed sequentially and completely. Do NOT skip any step.

1. **Sequential Execution**: Steps 1-4 must be completed in order. Do NOT proceed to next step until current step is verified complete.
2. **Full-Stack Requirement**: Both backend (Step 2) AND frontend (Step 3) are REQUIRED. Creating only backend or only frontend is INCOMPLETE.
3. **Verification Before Completion**: Step 4 verification MUST pass before considering the task complete.
4. **Completion Checklist**: Before marking task as done, verify ALL items in the "Quick Checklist" above are checked.

## Workflow

### Step 1: Understand Requirements ⚠️ REQUIRED

Ask the user to clarify:
- Entity name and properties
- Module name (if different from entity)
- Whether to add to existing module menu or create new module menu
- Special requirements (tree structure, relationships, etc.)

**Do NOT proceed to Step 2 until requirements are clear.**

### Step 2: Create Backend Files ⚠️ REQUIRED

#### 2.1 Entity Class

Location: `Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Domain/Entities/{EntityName}AggregateRoot.cs`

See `references/backend-patterns.md` for full template. Key points:
- Inherit `AggregateRoot<Guid>`
- Implement: `ISoftDelete`, `IAuditedObject`, `IOrderNum`, `IState`
- Use `[SugarTable]` and `[SugarColumn]` attributes

#### 2.2 DTO Classes (5 files)

Location: `Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application.Contracts/Dtos/{EntityName}/`

Create:
1. `{EntityName}GetOutputDto.cs` - Single entity retrieval
2. `{EntityName}GetListOutputDto.cs` - List queries
3. `{EntityName}GetListInputVo.cs` - Query parameters (inherit `PagedAllResultRequestDto`)
4. `{EntityName}CreateInputVo.cs` - Creation
5. `{EntityName}UpdateInputVo.cs` - Updates

See `references/backend-patterns.md` for patterns.

#### 2.3 Service Interface

Location: `Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application.Contracts/IServices/I{EntityName}Service.cs`

Inherit `IYiCrudAppService<...>` with all DTO types.

#### 2.4 Service Implementation

Location: `Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application/Services/{EntityName}Service.cs`

Inherit `YiCrudAppService<...>` and implement interface. Override `GetListAsync()` for custom queries.

See `references/backend-patterns.md` for full pattern.

#### 2.5 Menu Seed Data ⚠️ CRITICAL - DO NOT SKIP

**CHECKPOINT**: Before proceeding, verify:
- [ ] Entity file created
- [ ] All 5 DTO files created
- [ ] Service interface created
- [ ] Service implementation created
- [ ] Menu seed data is next (REQUIRED)

**IMPORTANT**: Menu seed data is REQUIRED for the module to appear in the UI and for permissions to work.

**Decision Point**: Ask user whether to:
- **Option A**: Add to existing module menu (e.g., add AppNav to existing APP配置 menu)
- **Option B**: Create new module menu file

**Option A - Update Existing Menu File:**

Location: `Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed/{ExistingModule}MenuDataSeed.cs`

Add menu items for the new entity to the existing `GetSeedData()` method. See `references/menu-seed-patterns.md` for examples.

**Option B - Create New Module Menu File:**

Location: `Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed/{ModuleName}MenuDataSeed.cs`

Create complete menu structure with:
- Top-level catalogue menu
- Entity menu with list permission
- CRUD permission menus (query, add, edit, remove)

See `references/menu-seed-patterns.md` for full template.

**Permission Code Format**: `{module-name}:{entity-name}:{action}`
- Example: `app:app-nav:list`, `app:app-nav:add`, `app:app-nav:edit`, `app:app-nav:remove`

**STEP 2 CHECKPOINT - VERIFY BEFORE PROCEEDING:**
- [ ] Entity file exists and is correct
- [ ] All 5 DTO files exist and are correct
- [ ] Service interface exists and is correct
- [ ] Service implementation exists and is correct
- [ ] Menu seed data created/updated (CRITICAL)

**⚠️ DO NOT proceed to Step 3 until ALL Step 2 items above are checked.**

### Step 3: Create Frontend Files ⚠️ REQUIRED - DO NOT SKIP

**IMPORTANT**: Frontend files are REQUIRED for the module to be functional. Creating only backend files results in an INCOMPLETE module.

#### 3.1 API Files ⚠️ CRITICAL

Location: `Yi.Vben5/apps/web-antd/src/api/{module-name}/{entity-name}/`

Create:
- `model.d.ts` - TypeScript interface (REQUIRED)
- `index.ts` - API functions (list, info, add, update, remove) (REQUIRED)

See `references/frontend-patterns.md` for patterns.

#### 3.2 View Files ⚠️ CRITICAL

Location: `Yi.Vben5/apps/web-antd/src/views/{module-name}/{entity-name}/`

Create:
- `index.vue` - Main list view with table and CRUD operations (REQUIRED)
- `data.ts` - Form schemas (querySchema, columns, drawerSchema) (REQUIRED)
- `{entity-name}-drawer.vue` - Create/Edit drawer component (REQUIRED)

See `references/frontend-patterns.md` for patterns.

**STEP 3 CHECKPOINT - VERIFY BEFORE PROCEEDING:**
- [ ] API model.d.ts file exists and is correct
- [ ] API index.ts file exists and is correct
- [ ] View index.vue file exists and is correct
- [ ] View data.ts file exists and is correct
- [ ] View drawer component exists and is correct

**⚠️ DO NOT proceed to Step 4 until ALL Step 3 items above are checked.**

### Step 4: Build Verification ⚠️ REQUIRED - MUST PASS

**CRITICAL**: This step MUST pass before the task is considered complete. Do NOT skip verification.

#### 4.1 Backend Verification (REQUIRED)

1. Run `dotnet build` in `Yi.Abp` directory
2. If build fails:
   - Read error messages carefully
   - Fix compilation errors (common: wrong base class, missing using statements)
   - Re-run build until successful
3. **DO NOT proceed until backend build succeeds**

#### 4.2 Frontend Verification (REQUIRED)

1. Navigate to `Yi.Vben5` directory
2. Run TypeScript type check: `pnpm run typecheck` (or equivalent command)
3. If type errors exist:
   - Fix TypeScript errors in API models and view files
   - Ensure all imports are correct
   - Re-run type check until successful
4. Run lint check: `pnpm run lint` (optional but recommended)
5. **DO NOT proceed until frontend verification passes**

#### 4.3 Documentation (REQUIRED)

Document the module in `.docs/{ModuleName}模块开发文档.md` with:
- Module overview
- Entity structure
- API endpoints
- Frontend routes and permissions

**Note**: If `.docs/` directory does not exist, create it first.

**FINAL CHECKPOINT - TASK COMPLETION VERIFICATION:**
- [ ] Backend build: `dotnet build` passes without errors
- [ ] Frontend type check: TypeScript compilation passes without errors
- [ ] Frontend lint: No critical linting errors (warnings acceptable)
- [ ] Documentation: Module documented in `.docs/`
- [ ] Quick Checklist: All items in the checklist above are checked

**⚠️ TASK IS ONLY COMPLETE WHEN ALL ITEMS ABOVE ARE CHECKED AND VERIFIED.**

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
- **Config**: `Yi.Abp/module/rbac/` and `Yi.Vben5/apps/web-antd/src/views/system/config/` and `Yi.Vben5\apps\web-antd\src\api\system\config`
- **User**: `Yi.Abp/module/rbac/` and `Yi.Vben5/apps/web-antd/src/views/system/user/` and `Yi.Vben5\apps\web-antd\src\api\system\user`
