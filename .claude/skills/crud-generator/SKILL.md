---
name: crud-generator
description: Initialize complete business module scaffolding for both backend (C# .NET with ABP framework) and frontend (Vue3 + Vben5 + Ant Design Vue) with PARALLEL agent execution. Creates entity classes, DTOs, service interfaces, service implementations, menu/dictionary seed data, API files, and view components following the project's established patterns. Use when user needs to quickly create a new business feature or CRUD module that requires full-stack implementation. IMPORTANT: Use this skill when user requests "生成CRUD", "创建模块", "生成基础代码" — prioritizes speed through parallel execution.
---

# CRUD Generator

Generates complete business modules following the project's ABP + SqlSugar + Vue3 + Vben5 architecture. Uses **parallel agents** for faster execution.

## ⚠️ CRITICAL EXECUTION WARNING

**THIS SKILL REQUIRES FULL-STACK IMPLEMENTATION**

- ❌ **INCOMPLETE**: Creating only backend files
- ❌ **INCOMPLETE**: Creating only frontend files
- ❌ **INCOMPLETE**: Skipping verification steps
- ✅ **COMPLETE**: Backend + Frontend + Verification all pass

**You MUST complete Steps 1-4. Step 3 uses parallel agents. Do NOT mark the task as done until ALL verification steps pass.**

## Quick Checklist

When creating a module, you need:

**Backend (Yi.Abp/module/{module-name}):** ⚠️ REQUIRED
- [ ] Entity: `Domain/Entities/{EntityName}AggregateRoot.cs`
- [ ] Enum (if any): `Domain.Shared/Enums/{EnumName}.cs`
- [ ] DTOs (5 files): `Application.Contracts/Dtos/{EntityName}/`
- [ ] Service Interface: `Application.Contracts/IServices/I{EntityName}Service.cs`
- [ ] Service Implementation: `Application/Services/{EntityName}Service.cs`
- [ ] **Menu Seed Data**: Update `module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed.cs` ⚠️ CRITICAL - DO NOT SKIP
- [ ] **Dictionary Seed Data**: Update `module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryDataSeed.cs` (if enums exist)
- [ ] Entity Seed Data (optional): `SqlSugarCore/DataSeeds/{EntityName}DataSeed.cs`

**Frontend (Yi.Vben5/apps/web-antd/src):** ⚠️ REQUIRED - DO NOT SKIP
- [ ] API Model: `api/{module-name}/{entity-name}/model.d.ts` ⚠️ CRITICAL
- [ ] API Functions: `api/{module-name}/{entity-name}/index.ts` ⚠️ CRITICAL
- [ ] View Index: `views/{module-name}/{entity-name}/index.vue` ⚠️ CRITICAL
- [ ] View Data: `views/{module-name}/{entity-name}/data.ts` ⚠️ CRITICAL
- [ ] View Drawer: `views/{module-name}/{entity-name}/{entity-name}-drawer.vue` ⚠️ CRITICAL
- [ ] Dict Enum Update: `packages/@core/base/shared/src/constants/dict-enum.ts` (if enums exist)

**Verification:** ⚠️ REQUIRED - MUST PASS
- [ ] Backend: Run `dotnet build` in Yi.Abp directory - MUST SUCCEED
- [ ] Backend: Fix any compilation errors
- [ ] Frontend: Run TypeScript type check - MUST SUCCEED
- [ ] Frontend: Fix any type errors
- [ ] Frontend: Run lint check (recommended)
- [ ] Documentation: Document the module in `docs/`（目录不存在则创建）

**Completion Check:** ⚠️ VERIFY BEFORE MARKING DONE
- [ ] All backend files created and verified
- [ ] All frontend files created and verified
- [ ] Backend build passes
- [ ] Frontend type check passes
- [ ] Documentation completed

## Execution Principles ⚠️ MANDATORY

**CRITICAL**: This workflow follows a hybrid sequential-parallel pattern.

1. **Step 1 → Step 2 Sequential**: Understand requirements first, then create entity class.
2. **Step 3 Parallel**: After entity is created, spawn 3 agents IN PARALLEL using a single message with multiple Agent tool calls.
3. **Step 4 Sequential**: After ALL 3 agents complete, verify build sequentially (compile entire Yi.Abp solution once).
4. **Verification Before Completion**: Step 4 verification MUST pass before considering the task complete.
5. **No Agent Compilation**: Agents must NOT run dotnet build/typecheck — they only generate code files. Compilation happens in Step 4.

## Naming Conventions

| Layer | Project Prefix | Module Class Prefix | Example |
|-------|---------------|---------------------|---------|
| Framework | `Yi.Framework.` | `YiFramework` | `Yi.Framework.Core`, `YiFrameworkCoreModule` |
| Module | `Yi.Framework.` | `YiFramework` | `Yi.Framework.Rbac.Domain`, `YiFrameworkRbacDomainModule` |
| Application | `Yi.Abp.` | `YiAbp` | `Yi.Abp.Domain`, `YiAbpDomainModule` |

## Workflow

### Step 1: Understand Requirements ⚠️ REQUIRED

Ask the user to clarify:
- Entity name and properties
- Module name (if different from entity)
- Enums: which fields need enums, with values and Chinese labels
- Entity type: aggregate_root / entity
- Relations (join queries, e.g., Category)
- Tree structure (if applicable)
- Menu parent (optional, will derive icon from entity name)
- Whether to add to existing module menu or create new module menu

**Do NOT proceed to Step 2 until requirements are clear.**

### Step 2: Create Entity & Enums ⚠️ REQUIRED

#### 2.1 Enums (if applicable)

Location: `Yi.Abp/module/{module-name}/Yi.Framework.{Module}.Domain.Shared/Enums/{EnumName}.cs`

```csharp
using System.ComponentModel;

namespace Yi.Framework.{Module}.Domain.Shared.Enums;

public enum {EnumName}
{
    [Description("中文标签")] Value0 = 0,
    [Description("中文标签")] Value1 = 1,
}
```

#### 2.2 Entity Class

Location: `Yi.Abp/module/{module-name}/Yi.Framework.{Module}.Domain/Entities/{EntityName}AggregateRoot.cs`

See `references/backend-patterns.md` for full template. Key points:
- Inherit `AggregateRoot<Guid>`
- Implement standard interfaces: `ISoftDelete`, `IAuditedObject`, `IOrderNum`, `IState`
- Use `[SugarTable]` and `[SugarColumn]` attributes
- Use `[SugarIndex]` for indexed fields
- Use `[Navigate]` for navigation properties

**STEP 2 CHECKPOINT - VERIFY BEFORE PROCEEDING:**
- [ ] Enum files created (if applicable)
- [ ] Entity file created
- [ ] Entity follows naming conventions

**⚠️ DO NOT proceed to Step 3 until entity file exists.**

### Step 3: Dispatch Parallel Agents ⚡ REQUIRED

**CRITICAL**: Spawn ALL 3 agents in a SINGLE message with multiple Agent tool calls. This enables parallel execution.

#### Agent A: Backend CRUD Generator

**Input**: Entity file path, module name, enum file paths (if any)

**Output**:
- DTOs (5 files): `Application.Contracts/Dtos/{EntityName}/`
- IService: `Application.Contracts/IServices/I{EntityName}Service.cs`
- Service: `Application/Services/{EntityName}Service.cs`

**Agent Prompt Template**:
```
You are Agent A: Backend CRUD Generator.

Task: Generate backend CRUD code for entity.

Input:
- Entity path: {entity_file_path}
- Module: {module_name}
- Enum paths: {enum_file_paths_or_none}

Instructions:
1. Read the entity file to extract: fields, types, navigations, indexes, namespace
2. Check if entity has ParentId field → this is a TREE entity (requires special handling)
3. Read enum files (if any) to understand enum types and their namespaces
4. Read references/backend-patterns.md for patterns
5. For TREE entities, READ Yi.Framework.Rbac.Application/Services/System/DeptService.cs as reference (NOT user/config)
6. Read Yi.Framework.Ddd.Application/YiCrudAppService.cs to understand base class method signatures

⚠️ TREE ENTITY CHECK (ParentId field exists):
- Return type: List<{GetListOutputDto}> (NOT PagedResultDto<T>)
- Use [Route("xxx/list")] custom route, method name: GetListAsync
- pagerConfig.enabled: false in frontend
- Need GetTreeAsync method for tree structure
- Need GetListExcludeAsync method (exclude self and children for edit)
- Need GetAllChildrenIdsAsync recursive helper method
- Need CheckUpdateInputDtoAsync validation (parent cannot be self or children)

7. Generate DTOs in Application.Contracts/Dtos/{Entity}/:
   - {Entity}GetOutputDto.cs
   - {Entity}GetListOutputDto.cs
   - {Entity}GetListInputVo.cs (TREE: DO NOT inherit PagedAllResultRequestDto, no pagination)
   - {Entity}CreateInputVo.cs
   - {Entity}UpdateInputVo.cs
   
   ⚠️ NAMESPACE CHECK (REQUIRED before writing each file):
   - Check all types used in the file and add required using statements
   - Common namespaces: Yi.Framework.{Module}.Domain.Entities, Yi.Framework.{Module}.Domain.Shared.Enums, Yi.Framework.Ddd.Application.Contracts, Volo.Abp.Application.Dtos
   - DO NOT rely on compilation to find missing namespaces

8. Generate IService in Application.Contracts/IServices/I{Entity}Service.cs
   - Inherit IYiCrudAppService<{Entity}, {GetOutputDto}, {GetListOutputDto}, Guid, {GetListInputVo}, {CreateInputVo}, {UpdateInputVo}>
   - TREE: Add GetTreeAsync and GetListExcludeAsync method signatures
   - Add all required using statements

9. Generate Service in Application/Services/{Entity}Service.cs
   - Inherit YiCrudAppService<{Entity}, {GetOutputDto}, {GetListOutputDto}, Guid, {GetListInputVo}, {CreateInputVo}, {UpdateInputVo}>
   
   ⚠️ METHOD SIGNATURE (CRITICAL):
   - TREE: GetListAsync returns List<T>, use [Route("xxx/list")], NOT override (different signature)
   - NON-TREE: GetListAsync returns PagedResultDto<T>, use override keyword
   - NEVER use `new` keyword to hide base method
   
   TREE entity methods (reference DeptService.cs):
   - GetListAsync: [Route("xxx/list")] → List<T>
   - GetTreeAsync: tree structure for dropdown
   - GetListExcludeAsync: [HttpGet, Route("xxx/list/exclude/{id}")] → List<T>
   - GetAllChildrenIdsAsync + GetChildrenIdsRecursive: recursive helper
   
   - Use WhereIF for conditional filtering
   - Use LeftJoin + Select for relation fields projection
   - Add all required using statements

10. DO NOT run dotnet build or any compilation command. Your task is ONLY to generate code files.

Follow CLAUDE.md naming conventions. Create all files using Write tool.
```

#### Agent B: Frontend Generator

**Input**: Entity file path, module name, enum file paths (if any)

**Output**:
- `api/{module}/{entity}/model.d.ts`
- `api/{module}/{entity}/index.ts`
- `views/{module}/{entity}/index.vue`
- `views/{module}/{entity}/data.ts`
- `views/{module}/{entity}/{entity}-drawer.vue`

**Agent Prompt Template**:
```
You are Agent B: Frontend Generator.

Task: Generate frontend code for entity.

Input:
- Entity path: {entity_file_path}
- Module: {module_name}
- Enum paths: {enum_file_paths_or_none}

Instructions:
1. Read the entity file to extract: fields, types, descriptions
2. Check if entity has ParentId field → this is a TREE entity (requires special handling)
3. For TREE entities, READ Yi.Vben5/apps/web-antd/src/views/system/dept/ directory as reference (NOT user/config)
4. Read enum files (if any) to map enum → number type
5. Read references/frontend-patterns.md for patterns

⚠️ IMPORT CHECK (REQUIRED before writing each file):
- Check all types and imports used in the file
- Ensure all import paths are correct relative to file location
- Common imports: @vben/request, dict-enum constants, API types
- DO NOT rely on type check to find missing imports

6. Generate api/{module}/{entity}/model.d.ts:
   - TypeScript interfaces matching entity fields
   - Enum fields as number type
   - Add all required imports

7. Generate api/{module}/{entity}/index.ts:
   - API functions: list, info, add, update, remove
   - TREE: list returns Entity[] array (NOT paginated)
   - TREE: add nodeList function (exclude self and children for edit)
   - Follow existing patterns in Yi.Vben5
   - Add all required imports

8. Generate views/{module}/{entity}/index.vue:
   - VXE Table with CRUD operations
   
   ⚠️ TREE CONFIG (if ParentId exists):
   - pagerConfig: { enabled: false } (no pagination)
   - treeConfig: { parentField: 'parentId', rowField: 'id', transform: true }
   - rowConfig: { keyField: 'id' }
   - proxyConfig.ajax.query: Map root node parentId (EMPTY_GUID) to null
   - querySuccess: Default expand all (eachTree + setAllTreeExpand)
   - Toolbar: Add expand/collapse all buttons
   - Action: Add "sub-add" button (add child node)
   - cellDblclick: Toggle tree expand
   - toggleTreeExpand: Sync expand state
   - Reference: Yi.Vben5/apps/web-antd/src/views/system/dept/index.vue
   
   - Use Vben5 patterns (BasicTable, Toolbar)
   - Add all required imports

9. Generate views/{module}/{entity}/data.ts:
   - querySchema: search form fields
   - columns: table columns
   
   ⚠️ TREE COLUMN:
   - Tree node column: { field: 'xxxName', title: '名称', treeNode: true }
   
   - Dict rendering for enums: slots.default → renderDict(row.field, DictEnum.XXX)
   - drawerSchema: create/edit form fields
   
   ⚠️ TREE DRAWER:
   - ParentId field: component: 'TreeSelect', use nodeList API for options
   - Show condition: parentId !== EMPTY_GUID (hide for root edit)
   
   - Add all required imports

10. Generate views/{module}/{entity}/{entity}-drawer.vue:
    - Drawer component for create/edit
    - Use BasicDrawer from Vben5
    - TREE: Call nodeList API to get parent options (exclude self and children)
    - Add all required imports

11. DO NOT run typecheck, lint, or any compilation command. Your task is ONLY to generate code files.

Follow existing patterns in Yi.Vben5. Create all files using Write tool.
```

#### Agent C: Seed Data Generator

**Input**: Entity file path, module name, enum file paths (if any), menu parent, menu option (A/B)

**Output**:
- Update existing: `module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed.cs`
- Update existing: `module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryDataSeed.cs` (if enums exist)
- Update: `packages/@core/base/shared/src/constants/dict-enum.ts` (if enums exist)

**Agent Prompt Template**:
```
You are Agent C: Seed Data Generator.

Task: Generate menu and dictionary seed data for entity.

Input:
- Entity path: {entity_file_path}
- Module: {module_name}
- Enum paths: {enum_file_paths_or_none}
- Menu parent: {menu_parent_or_none}
- Menu option: {A_or_B} (A=update existing menu file, B=create new module menu in existing file)

Instructions:
1. Read the entity file to get entity description
2. Read enum files to extract enum values with descriptions
3. Read references/menu-seed-patterns.md for patterns

⚠️ NAMESPACE/IMPORT CHECK (REQUIRED before editing each file):
- Read the target file first to understand existing imports and namespace
- Ensure all new types added have correct namespace/import statements
- DO NOT rely on compilation to find missing namespaces

Menu Seed (CRITICAL):
- Yi.Abp project uses a single MenuDataSeed.cs file, NOT separate files per entity
- Location: Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed.cs
- Edit the existing file to add new menu entries in GetSeedData() method
- Permission codes: {module}:{entity}:{action} (list, add, edit, remove)
- Menu icon: Derive from entity name using rules below

Icon Derivation Rules:
- *User* → user-outlined
- *Role* → team-outlined
- *Order* → shopping-cart-outlined
- *Log* → file-text-outlined
- *Config* → setting-outlined
- *Category* → appstore-outlined
- *Payment* → pay-circle-outlined
- *Product* → shopping-outlined
- Default → appstore-outlined

Dictionary Seed (if enums exist):
- Yi.Abp project uses a single DictionaryDataSeed.cs file, NOT separate files per entity
- Location: Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryDataSeed.cs
- Edit the existing file to add new dictionary entries in GetSeedData() method
- DictType: {module}_{enum_lower} (e.g., pokemon_status)
- DictValue: enum int value as string (e.g., "0", "1")
- DictLabel: Description attribute value

dict-enum.ts Update (if enums exist):
- Location: Yi.Vben5/packages/@core/base/shared/src/constants/dict-enum.ts
- Read the file first to understand existing constants
- Add constant: {MODULE}_{ENUM_UPPER} = '{module}_{enum_lower}'
- Example: POKEMON_STATUS = 'pokemon_status'

5. DO NOT run dotnet build or any compilation command. Your task is ONLY to generate/update code files.

Create all files using Write/Edit tools.
```

#### Dispatch Example

```python
# Spawn ALL agents in ONE message with multiple Agent tool calls
Agent(description="Backend CRUD", prompt="...Agent A prompt...")
Agent(description="Frontend code", prompt="...Agent B prompt...")
Agent(description="Seed data", prompt="...Agent C prompt...")
```

**STEP 3 CHECKPOINT - WAIT FOR ALL AGENTS:**
- [ ] Agent A completed: DTOs, IService, Service created
- [ ] Agent B completed: API files, Vue views created
- [ ] Agent C completed: Menu/Dict seeds updated, dict-enum.ts updated

**⚠️ DO NOT proceed to Step 4 until ALL 3 agents report completion.**

### Step 4: Build Verification ⚠️ REQUIRED - MUST PASS

**CRITICAL**: This step MUST pass before the task is considered complete. Do NOT skip verification.

**⚠️ IMPORTANT**: Run compilation ONLY after ALL 3 agents (A, B, C) have completed their code generation tasks. Do NOT compile after each individual agent finishes.

#### 4.1 Backend Verification (REQUIRED)

1. **Wait for ALL agents to complete** before running any build command
2. Run `dotnet build` in `Yi.Abp` directory
3. If build fails:
   - Read error messages carefully
   - Fix compilation errors (common: wrong base class, missing using statements)
   - Re-run build until successful
4. **DO NOT proceed until backend build succeeds**

#### 4.2 Frontend Verification (REQUIRED)

1. **Wait for ALL agents to complete** before running any typecheck command
2. Navigate to `Yi.Vben5/apps/web-antd` directory
3. Run TypeScript type check: `pnpm run typecheck` (or equivalent command)
4. If type errors exist:
   - Fix TypeScript errors in API models and view files
   - Ensure all imports are correct
   - Re-run type check until successful
5. Run lint check: `pnpm run lint` (optional but recommended)
6. **DO NOT proceed until frontend verification passes**

#### 4.3 Documentation (REQUIRED)

Document the module in `docs/{ModuleName}模块开发文档.md` with:
- Module overview
- Entity structure
- API endpoints
- Frontend routes and permissions

**Note**: If `docs/` directory does not exist, create it first.

**FINAL CHECKPOINT - TASK COMPLETION VERIFICATION:**
- [ ] Backend build: `dotnet build` passes without errors
- [ ] Frontend type check: TypeScript compilation passes without errors
- [ ] Frontend lint: No critical linting errors (warnings acceptable)
- [ ] Documentation: Module documented in `docs/`
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

## Reference Files

- `references/backend-patterns.md` - Entity, DTO, and service templates
- `references/menu-seed-patterns.md` - Menu seed data examples
- `references/frontend-patterns.md` - Frontend API and view templates
- `references/troubleshooting.md` - Common issues and solutions

## Examples

Reference existing modules:
- **普通列表（分页）**: Config, User — `Yi.Abp/module/rbac/` + `Yi.Vben5/apps/web-antd/src/views/system/config/` + `Yi.Vben5/apps/web-antd/src/api/system/config`
- **树表（无分页）**: Dept — `Yi.Abp/module/rbac/Yi.Framework.Rbac.Application/Services/System/DeptService.cs` + `Yi.Vben5/apps/web-antd/src/views/system/dept/` + `Yi.Vben5/apps/web-antd/src/api/system/dept`