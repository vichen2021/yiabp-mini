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

#### Agent A: Backend CRUD Generator (Template-Based)

**Input**: Entity file path, module name, enum file paths (if any)

**Output**:
- DTOs (5 files): `Application.Contracts/Dtos/{EntityName}/`
- IService: `Application.Contracts/IServices/I{EntityName}Service.cs`
- Service: `Application/Services/{EntityName}Service.cs`
- TreeDto (if tree entity): `Domain.Shared/Dtos/{EntityName}TreeDto.cs`
- Repository (if tree entity): `Domain/Repositories/I{EntityName}Repository.cs` + `SqlSugarCore/Repositories/{EntityName}Repository.cs`

**Agent Prompt Template**:
```
You are Agent A: Backend CRUD Generator (Template-Based).

Task: Generate backend CRUD code using templates. MAXIMIZE SPEED by minimizing file reads.

Input:
- Entity path: {entity_file_path}
- Module: {module_name}
- Enum paths: {enum_file_paths_or_none}

⚡ SPEED OPTIMIZATION - READ ONLY 3 FILES:
1. Read entity file at {entity_file_path} (extract ALL data in ONE read)
2. Read templates/dto-templates.md (contains all 5 DTO templates)
3. Read templates/service-templates.md (contains IService + Service templates)
4. If entity has ParentId → read templates/tree-templates.md (TreeDto + Repository templates)

DO NOT read DeptService.cs, YiCrudAppService.cs, or backend-patterns.md - templates contain all patterns.

📊 ENTITY DATA EXTRACTION (parse from entity file ONCE):
Extract into this structure:
```
entityName = "ProductCategory"  // class name without AggregateRoot
moduleName = "product"          // lowercase module
moduleNamespace = "Product"     // PascalCase for namespace
entityComment = "产品分类"       // from XML summary
isTree = true/false             // has ParentId field
fields = [
  {name, type, nullable, comment, isIndex, isTreeField}
]
```

📝 FILE GENERATION (batch Write calls):

**Common Files (all entities):**
- Application.Contracts/Dtos/{Entity}/
  - {Entity}GetOutputDto.cs (use GetOutputDto template)
  - {Entity}GetListOutputDto.cs (use GetListOutputDto template)
  - {Entity}GetListInputVo.cs (use GetListInputVo template - tree vs non-tree)
  - {Entity}CreateInputVo.cs (use CreateInputVo template)
  - {Entity}UpdateInputVo.cs (use UpdateInputVo template)
- Application.Contracts/IServices/I{Entity}Service.cs (use IService template)
- Application/Services/{Entity}Service.cs (use Service template - tree vs non-tree)

**Tree Entity Additional Files:**
- Domain.Shared/Dtos/{Entity}TreeDto.cs (use TreeDto template)
- Domain.Entities/{Entity}AggregateRoot.cs (append EntityExtension using template)
- Domain.Repositories/I{Entity}Repository.cs (use IRepository template)
- SqlSugarCore/Repositories/{Entity}Repository.cs (use Repository template)
- Domain.Shared/Yi.Framework.{Module}.Domain.Shared.csproj (add SqlSugarCore.Abstractions reference)

⚠️ TEMPLATE RENDERING RULES:
- Replace {{entityName}} with actual entity name
- Replace {{moduleNamespace}} with PascalCase module name
- Replace {{entityComment}} with entity description
- Replace {{entityNameLower}} with lowercase entity name (e.g., "productCategory")
- Iterate {{#each fields}} to generate field properties
- Use {{#if isTree}}, {{#if hasState}}, {{#if hasOrderNum}} for conditional sections
- Skip Id, CreationTime, CreatorId, etc. (standard fields handled by base class)

⚠️ TREE ENTITY KEY POINTS (embedded in templates):
- GetListAsync uses `new` keyword, [Route("xxx/list")], returns List<T>
- GetTreeAsync, GetListExcludeAsync, GetAllChildrenIdsAsync methods included
- CheckUpdateInputDtoAsync validates parent circular reference

DO NOT run dotnet build. Create all files using Write tool in batch.
```

#### Agent B: Frontend Generator (Template-Based)

**Input**: Entity file path, module name, enum file paths (if any)

**Output**:
- `api/{module}/{entity}/model.d.ts`
- `api/{module}/{entity}/index.ts`
- `views/{module}/{entity}/index.vue`
- `views/{module}/{entity}/data.ts`
- `views/{module}/{entity}/{entity}-drawer.vue`

**Agent Prompt Template**:
```
You are Agent B: Frontend Generator (Template-Based).

Task: Generate frontend code using templates. MAXIMIZE SPEED by minimizing file reads.

Input:
- Entity path: {entity_file_path}
- Module: {module_name}
- Enum paths: {enum_file_paths_or_none}

⚡ SPEED OPTIMIZATION - READ ONLY 4 FILES:
1. Read entity file (extract ALL data in ONE read)
2. Read templates/frontend-api-templates.md (contains model.d.ts + index.ts templates)
3. Read templates/frontend-views-templates.md (contains data.ts + index.vue + drawer.vue templates)
4. Read references/frontend-patterns.md (for additional patterns if needed)

DO NOT read dept views or other reference files - templates contain all patterns.

📊 ENTITY DATA EXTRACTION (parse from entity file ONCE):
Extract into this structure:
```
entityName = "Item"
entityNameLower = "item"
moduleName = "product"
entityComment = "物品"
isTree = false/true
hasEnums = true/false
dictConstants = ["PRODUCT_ITEM_TYPE"]  // derived from module + enum name
treeNodeField = "itemName"  // field with isIndex=true, used as tree node label
fields = [
  {name: "itemName", label: "物品名称", tsType: "string", isIndex: true, isTreeNode: true, required: true},
  {name: "itemType", label: "物品类型", tsType: "number", isEnum: true, dictConstant: "PRODUCT_ITEM_TYPE", required: true},
  {name: "orderNum", label: "排序", tsType: "number"},
  {name: "state", label: "状态", tsType: "boolean", isBoolean: true},
  {name: "remark", label: "备注", tsType: "string", nullable: true},
  {name: "creationTime", label: "创建时间", tsType: "string", isDatetime: true}
]
```

📝 FILE GENERATION (batch Write calls):

**API Files (Yi.Vben5/apps/web-antd/src/api/{module}/{entity}/):**
1. model.d.ts - TypeScript interfaces (use frontend-api-templates.md)
2. index.ts - API functions (use frontend-api-templates.md - tree vs non-tree variant)

**View Files (Yi.Vben5/apps/web-antd/src/views/{module}/{entity}/):**
3. data.ts - Schema definitions (use frontend-views-templates.md - tree vs non-tree)
   - querySchema: filter fields
   - columns: table columns with renderDict for enums
   - drawerSchema: form fields
   
4. index.vue - VXE Table (use frontend-views-templates.md)
   - Non-tree: pagination + standard toolbar
   - Tree: treeConfig + expand/collapse buttons + add child action
   
5. {entity}-drawer.vue - Drawer component (use frontend-views-templates.md)

⚠️ DictEnum naming: {MODULE}_{ENUM_NAME} (e.g., PRODUCT_ITEM_TYPE)
⚠️ TreeNode field: the field with isIndex=true is used as tree node label (e.g., itemName)

DO NOT run typecheck. Create all files using Write tool in batch.
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

- `templates/dto-templates.md` - All 5 DTO templates (GetOutput, GetListOutput, GetListInput, Create, Update)
- `templates/service-templates.md` - IService + Service templates (normal and tree variants)
- `templates/tree-templates.md` - TreeDto + EntityExtension + Repository templates
- `templates/frontend-api-templates.md` - TypeScript model.d.ts + index.ts templates
- `templates/frontend-views-templates.md` - Vue view templates (data.ts, index.vue, drawer.vue)
- `references/backend-patterns.md` - Entity patterns (for Step 2 entity creation)
- `references/frontend-patterns.md` - Additional Vue patterns (fallback reference)
- `references/menu-seed-patterns.md` - Menu seed data examples
- `references/troubleshooting.md` - Common issues and solutions

## Examples

Reference existing modules:
- **普通列表（分页）**: Config, User — `Yi.Abp/module/rbac/` + `Yi.Vben5/apps/web-antd/src/views/system/config/` + `Yi.Vben5/apps/web-antd/src/api/system/config`
- **树表（无分页）**: Dept — `Yi.Abp/module/rbac/Yi.Framework.Rbac.Application/Services/System/DeptService.cs` + `Yi.Vben5/apps/web-antd/src/views/system/dept/` + `Yi.Vben5/apps/web-antd/src/api/system/dept`
- **产品模块示例**: Item — `Yi.Vben5/apps/web-antd/src/views/product/item/` + `Yi.Vben5/apps/web-antd/src/api/product/item`

---

## ⚠️ 常见问题与解决方案

以下问题在实际执行中多次出现，Agent 生成代码时必须避免：

### 问题 1：前端 API 方法错误

**错误代码**：
```typescript
// ❌ 错误：使用不存在的方法签名
return requestClient.delete(API_PREFIX, ids, { successMessageMode: 'message' });
```

**正确代码**：
```typescript
// ✅ 正确：使用项目已有的 deleteWithMsg 方法
export function {{entityNameLower}}Remove({{entityNameLower}}Ids: IDS) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: {{entityNameLower}}Ids.join(',') },
  });
}

// ✅ 正确：创建/更新方法
export function {{entityNameLower}}Add(data: {{entityName}}CreateInput) {
  return requestClient.postWithMsg<void>(Api.root, data);
}

export function {{entityNameLower}}Update(data: {{entityName}}UpdateInput) {
  return requestClient.putWithMsg<void>(`${Api.root}/${data.id}`, data);
}
```

### 问题 2：缺少类型导入

**错误代码**：
```typescript
// ❌ 错误：缺少 PageResult 导入，导致 TypeScript 错误
import { requestClient } from '#/api/request';
```

**正确代码**：
```typescript
// ✅ 正确：必须导入所有需要的类型
import type { ID, IDS, PageResult } from '#/api/common';
import { requestClient } from '#/api/request';
```

### 问题 3：导入路径错误

**错误路径** | **正确路径**
---|---
`'#/types'` 或 `'#/types/form'` | `'#/adapter/form'`
`'vxe-table'` | `'#/adapter/vxe-table'`
`'#/hooks'` | `'#/adapter/form'` 或 `'#/adapter/vxe-table'`
`'#/utils/render'` (getDictOptions) | `'#/utils/dict'`
`'#/utils/render'` (renderDict) | `'#/utils/render'` ✓

### 问题 4：Schema 格式错误

**错误代码**：
```typescript
// ❌ 错误：旧版 VbenFormSchema 数组格式
import type { VbenFormSchema } from '#/types';
export const querySchema: VbenFormSchema[] = [
  { field: 'materialName', label: '材料名称', component: 'Input' },
];
```

**正确代码**：
```typescript
// ✅ 正确：新版 FormSchemaGetter 函数格式
import type { FormSchemaGetter } from '#/adapter/form';
export const querySchema: FormSchemaGetter = () => [
  { component: 'Input', fieldName: 'materialName', label: '材料名称' },
];
```

**关键区别**：
- `field` → `fieldName`
- `VbenFormSchema[]` → `FormSchemaGetter` (函数返回)

### 问题 5：字典下拉选项格式错误

**错误代码**：
```typescript
// ❌ 错误：使用 api 返回函数
componentProps: {
  api: () => getDictOptions(DictEnum.PRODUCT_MATERIAL_TYPE),
  labelField: 'label',
  valueField: 'value',
  numberToString: true,
},
```

**正确代码**：
```typescript
// ✅ 正确：直接使用 options
componentProps: {
  getPopupContainer,
  options: getDictOptions(DictEnum.PRODUCT_MATERIAL_TYPE, true),
},
```

### 问题 6：renderDict 参数错误

**错误代码**：
```typescript
// ❌ 错误：boolean 类型直接传入
renderDict(row.state, DictEnum.SYS_NORMAL_DISABLE)
```

**正确代码**：
```typescript
// ✅ 正确：boolean 需转为 string，枚举可直接传 number
renderDict(String(row.state), DictEnum.SYS_NORMAL_DISABLE)
renderDict(row.materialType, DictEnum.PRODUCT_MATERIAL_TYPE)  // 枚举可直接传
```

### 问题 7：视图组件导入错误

**错误代码**：
```vue
<script setup lang="ts">
import { Page } from '@vben/common-ui';
import { useVbenModal, useVbenVxeGrid } from '#/hooks';
import { VbenDrawer } from '#/components';
```

**正确代码**：
```vue
<script setup lang="ts">
import { Page, useVbenDrawer } from '@vben/common-ui';
import { useVbenVxeGrid, vxeCheckboxChecked } from '#/adapter/vxe-table';
import { useVbenForm } from '#/adapter/form';
```

### 问题 8：类型断言缺失

**错误代码**：
```typescript
const data = cloneDeep(await formApi.getValues());
await (isUpdate.value ? {{entityNameLower}}Update(data) : {{entityNameLower}}Add(data));
```

**正确代码**：
```typescript
import type { {{entityName}}CreateInput, {{entityName}}UpdateInput } from '#/api/{{moduleName}}/{{entityNameLower}}/model';

const data = cloneDeep(await formApi.getValues()) as {{entityName}}CreateInput | {{entityName}}UpdateInput;
await (isUpdate.value 
  ? {{entityNameLower}}Update(data as {{entityName}}UpdateInput) 
  : {{entityNameLower}}Add(data as {{entityName}}CreateInput));
```

---

## ⚠️ Agent 执行检查清单

生成代码后，Agent 必须自检以下问题：

**API 文件 (index.ts)**：
- [ ] 导入 `type { ID, IDS, PageResult } from '#/api/common'`
- [ ] 使用 `requestClient.postWithMsg/putWithMsg/deleteWithMsg`
- [ ] 删除使用 `params: { ids: ids.join(',') }` 格式

**Data 文件 (data.ts)**：
- [ ] 导入 `type { FormSchemaGetter } from '#/adapter/form'`
- [ ] 导入 `type { VxeGridProps } from '#/adapter/vxe-table'`
- [ ] 导入 `getDictOptions from '#/utils/dict'`
- [ ] 导入 `renderDict from '#/utils/render'`
- [ ] Schema 使用 `FormSchemaGetter = () => [...]` 格式
- [ ] 字段使用 `fieldName` 而非 `field`
- [ ] 下拉使用 `options: getDictOptions(...)` 而非 `api: () => getDictOptions(...)`

**Index Vue (index.vue)**：
- [ ] 导入 `Page, useVbenDrawer from '@vben/common-ui'`
- [ ] 导入 `useVbenVxeGrid, vxeCheckboxChecked from '#/adapter/vxe-table'`
- [ ] 使用 `BasicTable, BasicDrawer` 组件名

**Drawer Vue (drawer.vue)**：
- [ ] 导入 `useVbenForm from '#/adapter/form'`
- [ ] 导入类型并使用类型断言
- [ ] 使用 `BasicForm, BasicDrawer` 组件名