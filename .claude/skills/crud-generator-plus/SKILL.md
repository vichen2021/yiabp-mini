---
name: crud-generator-plus
description: 快速生成完整 CRUD 代码，基于实体类.cs 直接解析生成前后端代码。耗时约60秒。当用户请求"生成CRUD"、"创建CRUD"、"生成基础代码"、"新建业务模块"时触发。支持普通实体和树形实体，自动处理枚举、字典、菜单种子数据。
---

# CRUD Generator Plus

高性能 CRUD 代码生成器，核心思路：**实体类.cs → C# 脚本直接解析 → 批量生成代码**。


## 工作流程

```
┌─────────────────────────────────────────────────────────────────┐
│  Step 1: LLM 生成实体类.cs + 枚举.cs                            │
│  - 根据用户需求生成实体类（包含默认公共/审计字段）                │
│  - 生成枚举类（如有枚举字段）                                    │
└─────────────────────────────────────────────────────────────────┘
                              ↓
         ┌────────────────────┴────────────────────┐
         │                                         │
┌─────────────────────────┐     ┌───────────────────────────────┐
│  Step 2a: 调用脚本生成   │     │  Step 2b: Agent 生成种子数据   │
│  - 解析实体类字段        │     │  - 菜单种子数据                │
│  - 生成后端 DTOs+Service │     │  - 字典种子数据                │
│  - 生成前端 API+Views    │     │  - dict-enum.ts 常量           │
└─────────────────────────┘     └───────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  Step 3: 构建验证                                           │
│  - 构建 Application 项目 和 前端 pnpm检查                                     │
│  - 检查生成文件质量                                             │
└─────────────────────────────────────────────────────────────────┘
```

## Step 1: 生成实体类和枚举类

### 默认字段规则

**⚠️ 重要：若用户未明确提及公共字段或审计字段的增减，则默认包含所有标准字段。**

默认包含的字段：

| 字段 | 类型 | 说明 |
|------|------|------|
| State | bool | 状态（默认 true） |
| OrderNum | int | 排序号（默认 0） |
| Remark | string? | 备注（可选，最大500字符） |
| IsDeleted | bool | 软删除标记 |
| CreationTime | DateTime | 创建时间 |
| CreatorId | Guid? | 创建者ID |
| LastModifierId | Guid? | 最后修改者ID |
| LastModificationTime | DateTime? | 最后修改时间 |

### 询问清单

仅在以下情况需要询问：

| 信息 | 何时询问 | 示例 |
|------|----------|------|
| 模块名称 | 用户未指定 | order |
| 枚举值 | 用户提及枚举但未指定值 | 普通=0,批发=1,预售=2 |
| 是否树形 | 用户提及"树形"或"父子" | 是 |
| 菜单父级 | 用户明确要求特定菜单位置 | 添加到系统管理 |

### 实体类模板

**位置**: `Yi.Abp/module/{module}/Yi.Framework.{Module}.Domain/Entities/{Entity}AggregateRoot.cs`

```csharp
using SqlSugar;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Yi.Framework.Core.Data;
using Yi.Framework.{Module}.Domain.Shared.Enums;

namespace Yi.Framework.{Module}.Domain.Entities;

/// <summary>
/// {实体中文名}
/// </summary>
[SugarTable("{Entity}")]
[SugarIndex($"index_{nameof({NameField})}", nameof({NameField}), OrderByType.Asc)]
public class {Entity}AggregateRoot : AggregateRoot<Guid>, ISoftDelete, IAuditedObject, IOrderNum, IState
{
    [SugarColumn(IsPrimaryKey = true)]
    public override Guid Id { get; protected set; }

    // 业务字段（用户定义）
    
    // 标准字段（默认包含）
    public bool State { get; set; } = true;
    public int OrderNum { get; set; } = 0;
    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }
    public bool IsDeleted { get; set; }
    public DateTime CreationTime { get; set; } = DateTime.Now;
    public Guid? CreatorId { get; set; }
    public Guid? LastModifierId { get; set; }
    public DateTime? LastModificationTime { get; set; }
}
```

### 枚举类模板

**位置**: `Yi.Abp/module/{module}/Yi.Framework.{Module}.Domain.Shared/Enums/{Enum}Enum.cs`

```csharp
using System.ComponentModel;

namespace Yi.Framework.{Module}.Domain.Shared.Enums;

public enum {Enum}Enum
{
    [Description("中文")] Value0 = 0,
    [Description("中文")] Value1 = 1,
}
```

## Step 2: 并行执行脚本和种子数据 Agent

**关键**：生成实体类后，在同一消息中**并行**执行脚本调用和种子数据 Agent。

### 2a: 调用脚本生成代码

```bash
dotnet run --file .claude/skills/crud-generator-plus/scripts/generate_crud.cs -- \
  --entity "Yi.Abp/module/{module}/Yi.Framework.{Module}.Domain/Entities/{Entity}AggregateRoot.cs" \
  --module "{module}"
```

**脚本输出文件**：

**后端（7个文件）**:
```
Application.Contracts/Dtos/{Entity}/
├── {Entity}GetOutputDto.cs
├── {Entity}GetListOutputDto.cs
├── {Entity}GetListInputVo.cs
├── {Entity}CreateInputVo.cs
└── {Entity}UpdateInputVo.cs

Application.Contracts/IServices/I{Entity}Service.cs  ← 包含 SelectListAsync 接口
Application/Services/{Entity}Service.cs              ← 包含 SelectListAsync 实现
```

**前端（5个文件）**:
```
Yi.Vben5/apps/web-antd/src/
├── api/{module}/{entity}/
│   ├── model.d.ts
│   └── index.ts                      ← 包含 selectList API
└── views/{module}/{entity}/
    ├── data.ts
    ├── index.vue
    └── {entity}-drawer.vue
```

### 新增功能：下拉列表（SelectList）

**后端接口**：
```csharp
Task<List<{Entity}GetOutputDto>> SelectListAsync(string? keywords = null);
```

**后端实现**：
- 查询条件：State == true + 关键字过滤（按 Name 字段）
- 返回：启用状态的数据列表，用于下拉选择组件

**前端 API**：
```typescript
export function {entity}SelectList(keywords?: string) {
  return requestClient.get<{Entity}[]>(`${Api.root}/select-list`, {
    params: keywords ? { keywords } : undefined,
  });
}
```

**使用场景**：
- 表单中的下拉选择框
- 关联实体的选择列表
- 搜索框的候选数据

### 新增功能：自动查询条件生成

脚本会根据实体字段自动识别并生成查询条件：

**搜索字段识别规则**（仅 string 类型）：
| 字段名关键字 | 示例 | 查询方式 |
|------|------|------|
| Name | UserName, ProductName | Contains |
| Title | Title, SubTitle | Contains |
| Code | Code, ProductCode | Contains |
| Key | ApiKey, AccessKey | Contains |
| No | OrderNo, SerialNo | Contains |
| Phone | Phone, MobilePhone | Contains |
| Email | Email, ContactEmail | Contains |

**枚举字段**：
- 自动添加精确匹配查询条件

**生成的查询参数**：
- GetListInputVo：仅包含搜索字段 + 枚举字段 + State
- 前端 querySchema：搜索字段输入框 + 枚举字段下拉框 + 状态 + 时间范围

**示例**（实体有 Name 和 Type 枚举字段）：
```csharp
// GetListInputVo
public string? Name { get; set; }
public ProductTypeEnum? Type { get; set; }
public bool? State { get; set; }

// Service 查询
.WhereIF(!string.IsNullOrEmpty(input.Name), x => x.Name.Contains(input.Name!))
.WhereIF(input.Type is not null, x => x.Type == (ProductTypeEnum)input.Type!)
```

### 树形实体额外输出

如果是树形实体（有 ParentId 字段），脚本额外生成：

```
Domain.Shared/Dtos/{Entity}TreeDto.cs
Domain/Repositories/I{Entity}Repository.cs
SqlSugarCore/Repositories/{Entity}Repository.cs
```

### 2b: 种子数据 Agent（单个 Agent 处理菜单和字典）

```markdown
任务：生成模块独立的菜单和字典种子数据（IDataSeedContributor 模式）

输入：
- 模块：{module}
- 模块Pascal：{ModulePascal}（如 ProductManage）
- 实体名：{Entity}
- 实体中文名：{实体中文名}
- 枚举信息：{枚举名} + {枚举值列表}（如有）

操作：

**创建独立种子数据文件**：
- 目录：Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/{ModulePascal}DataSeed/
- 文件：MenuDataSeed.cs, DictionaryTypeDataSeed.cs, DictionaryDataSeed.cs

**文件结构**（独立 IDataSeedContributor）：
- 类名：`{ModulePascal}MenuDataSeed`, `{ModulePascal}DictionaryTypeDataSeed`, `{ModulePascal}DictionaryDataSeed`
- 实现：`IDataSeedContributor, ITransientDependency`（不需要 partial）
- 构造函数：注入 `ISqlSugarRepository` + `IGuidGenerator`（菜单需要）
- SeedAsync 方法：包含条件检查 + 调用 GetSeedData()
- GetSeedData 方法：返回种子数据列表

**条件检查逻辑**：
- MenuDataSeed: `!await _repository.IsAnyAsync(x => x.MenuName == "{模块中文名}" && x.Router == "/{module}")`
- DictionaryTypeDataSeed: `!await _repository.IsAnyAsync(x => x.DictType == "{module}_{enum_lower}")`
- DictionaryDataSeed: `!await _repository.IsAnyAsync(x => x.DictType == "{module}_{enum_lower}")`

**菜单数据**：
- 父菜单（目录）：{模块中文名}, path: /{module}, icon: 根据关键词匹配
- 子菜单（菜单）：{实体中文名}, path: {entity}, component: {module}/{entity}/index
- 权限按钮：query/add/edit/remove

**字典数据**（如有枚举）：
- 字典类型：{module}_{enum_lower}（snake_case）
- 字典项：枚举值 Description 对应 DictLabel，枚举 int 值对应 DictValue

**前端字典常量**（dict-enum.ts）：
- 添加常量：{MODULE}_{ENUM} = '{module}_{enum_lower}'

图标规则：
- *Order* → shopping-cart-outlined
- *User* → user-outlined
- *Product* → shopping-outlined
- 默认 → appstore-outlined

报告：创建的文件、菜单项、权限码、字典项、常量列表。
```

## Step 3: 增量构建验证

### 后端验证

```bash
dotnet build Yi.Abp/module/{module}/Yi.Framework.{Module}.Application/Yi.Framework.{Module}.Application.csproj --no-restore
```

### 前端验证

**pnpm TypeScript 检查**（仅检查生成的模块目录，避免全项目检查耗时）：

```bash
cd Yi.Vben5 && pnpm run check:type --filter="@vben/web-antd" 2>&1 | grep -E "({module}|error TS)" || echo "前端类型检查通过"
```

**文件质量检查**：

```bash
# 检查关键文件内容
head -20 Yi.Abp/module/{module}/Yi.Framework.{Module}.Application.Contracts/Dtos/{Entity}/{Entity}GetListOutputDto.cs
grep DictEnum Yi.Vben5/apps/web-antd/src/views/{module}/{entity}/data.ts
```

### 验证失败处理

| 错误类型 | 处理方式 |
|------|----------|
| 后端构建失败 | 检查实体类语法、DTO 引用、服务实现 |
| 前端 typecheck 失败 | 检查 API 类型定义、dict-enum 常量引用 |
| 字典常量不匹配 | 统一 dict-enum.ts 与 data.ts 的常量名称 |

## 命名规范速查

| 类型 | 后端命名 | 前端命名 |
|------|----------|----------|
| 实体 | `{Entity}AggregateRoot` | `{entity}` (camelCase) |
| DTO 输入 | `{Entity}CreateInputVo` | `{Entity}CreateInput` |
| DTO 输出 | `{Entity}GetOutputDto` | `{Entity}` |
| 服务接口 | `I{Entity}Service` | - |
| 服务实现 | `{Entity}Service` | - |
| API 路径 | `/api/{module}/{entity}` | `/{module}/{entity}` (requestClient 自动添加 /api) |
| 权限码 | `{module}:{entity}:{action}` | - |
| 字典常量 | - | `{MODULE}_{ENUM}` (不含实体名) |
| 字典类型 | `{module}_{enum_lower}` | - |
| 种子数据目录 | `DataSeeds/{ModulePascal}DataSeed/` | - |

## 错误处理

| 场景 | 处理方式 |
|------|----------|
| 模块不存在 | 先调用 module-generator 创建模块 |
| 实体已存在 | 报错，提示删除或改名 |
| 构建失败 | 显示错误，修复后重新验证 |

## 文件清单

**Skill 目录结构**:
```
crud-generator-plus/
├── SKILL.md                          # 本文件
├── scripts/
│   └── generate_crud.cs              # 核心生成脚本
└── references/
    └── troubleshooting.md            # 常见问题（可选）
```

## 示例调用

**用户请求**:
> 在 product-manage 模块创建 Material CRUD。属性：id、名称、类型枚举（原材料=0, 半成品=1, 成品=2）。

**LLM 执行**:
1. 直接生成实体类（含默认审计字段）+ 枚举类
2. **并行执行**：
   - 调用脚本生成代码
   - 启动种子数据 Agent
3. 构建验证
4. 输出完成报告

---

## ⚠️ 关键约束

1. **默认包含审计字段** — 用户未明确说明时，自动包含 State、OrderNum、Remark、IsDeleted、CreationTime、CreatorId、LastModifierId、LastModificationTime
2. **脚本和 Agent 并行执行** — Step 2a 和 Step 2b 在同一消息中并行启动
3. **单个 Agent 处理种子数据** — 菜单和字典由一个 Agent 统一处理
4. **仅构建 Application 项目** — 不构建整个解决方案
5. **不使用 Agent 生成 DTO/Service** — 直接调用脚本生成
6. **前端验证必须运行 pnpm typecheck** — 确保生成的 TypeScript 文件类型正确
7. **字典常量命名一致性** — dict-enum.ts 的常量名必须与 data.ts 引用完全匹配（格式：`{MODULE}_{ENTITY}_{ENUM}`）