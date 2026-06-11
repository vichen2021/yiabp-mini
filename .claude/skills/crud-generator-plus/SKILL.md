---
name: crud-generator-plus
description: 快速生成完整 CRUD 代码，基于实体类.cs 直接解析生成前后端代码。耗时约60秒。当用户请求"生成CRUD"、"创建CRUD"、"生成基础代码"、"新建业务模块"时触发。支持普通实体和树形实体，自动处理枚举、字典、菜单种子数据。
---

# CRUD Generator Plus

高性能 CRUD 代码生成器，核心思路：**实体类.cs → 实体规范检查 → C# 脚本直接解析 → 批量生成代码**。


## 工作流程

```
┌─────────────────────────────────────────────────────────────────┐
│  Step 1: LLM 生成实体类.cs + 枚举.cs                            │
│  - 根据用户需求生成实体类（默认继承 BaseAggregateRoot<Guid>）     │
│  - 默认不启用 ConcurrencyStamp 乐观锁                             │
│  - 生成枚举类（如有枚举字段）                                    │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  Step 2: 实体规范检查（必须通过）                                │
│  - 检查命名、继承、SugarTable、主键、接口字段                    │
│  - 检查所有 public 属性必须有 XML summary                        │
│  - 检查失败时先修复实体，不得继续生成 CRUD                       │
└─────────────────────────────────────────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  Step 3a: 调用脚本生成                                           │
│  - 解析实体类字段                                                │
│  - 生成后端 DTOs+Service                                         │
│  - 生成前端 API+Views                                            │
└─────────────────────────────────────────────────────────────────┘
                              ↓
         ┌────────────────────┴────────────────────┐
         │                                         │
┌─────────────────────────┐     ┌───────────────────────────────┐
│  Step 3b: Agent 种子数据 │     │  Step 3c: Agent 生成后优化检查 │
│  - 菜单种子数据          │     │  - 抽屉表单组件优化            │
│  - 字典种子数据          │     │  - 列表/搜索/关联字段优化      │
│  - dict-enum.ts 常量     │     │  - DTO/Service 联动检查        │
└─────────────────────────┘     └───────────────────────────────┘
                              ↓
┌─────────────────────────────────────────────────────────────────┐
│  Step 4: 主线程验收 + 构建验证                                    │
│  - 汇总 Agent 输出并检查 diff                                    │
│  - 构建 Application 项目和前端 pnpm 检查                         │
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

**位置**: `Yi.Abp/module/{module}/Yi.Module.{Module}.Domain/Entities/{Entity}AggregateRoot.cs`

```csharp
using SqlSugar;
using Volo.Abp.Auditing;
using Yi.Framework.Core.Data;
using Yi.Framework.Ddd.Domain;
using Yi.Module.{Module}.Domain.Shared.Enums;

namespace Yi.Module.{Module}.Domain.Entities;

/// <summary>
/// {实体中文名}
/// </summary>
[SugarTable("{Entity}")]
[SugarIndex($"index_{nameof({NameField})}", nameof({NameField}), OrderByType.Asc)]
public class {Entity}AggregateRoot : BaseAggregateRoot<Guid>, ISoftDelete, IAuditedObject, IOrderNum, IState
{
    /// <summary>
    /// 主键
    /// </summary>
    [SugarColumn(IsPrimaryKey = true)]
    public override Guid Id { get; protected set; }

    // 业务字段（用户定义）：每个 public 属性必须包含 XML summary
    
    // 标准字段（默认包含）
    /// <summary>
    /// 状态
    /// </summary>
    public bool State { get; set; } = true;

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; } = 0;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }

    /// <summary>
    /// 软删除标记
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建者ID
    /// </summary>
    public Guid? CreatorId { get; set; }

    /// <summary>
    /// 最后修改者ID
    /// </summary>
    public Guid? LastModifierId { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModificationTime { get; set; }
}
```

### 乐观锁规则

- 默认实体继承 `BaseAggregateRoot<Guid>`，不包含 `ConcurrencyStamp`，CodeFirst 不会默认生成 `ConcurrencyStamp` 字段。
- 如用户明确要求乐观锁/并发版本校验，实体可改为继承 ABP 原生 `AggregateRoot<Guid>`。
- 启用乐观锁时必须补齐全链路：详情 DTO 返回 `ConcurrencyStamp`、UpdateInput 接收 `ConcurrencyStamp`、前端编辑提交带回原值，并处理并发异常提示。
- 未明确要求乐观锁时，不要在实体、DTO、前端类型中生成 `ConcurrencyStamp`。

### 枚举类模板

**位置**: `Yi.Abp/module/{module}/Yi.Module.{Module}.Domain.Shared/Enums/{Enum}Enum.cs`

```csharp
using System.ComponentModel;

namespace Yi.Module.{Module}.Domain.Shared.Enums;

public enum {Enum}Enum
{
    [Description("中文")] Value0 = 0,
    [Description("中文")] Value1 = 1,
}
```

## Step 2: 实体规范检查（生成前置门禁）

**关键**：生成实体类和枚举类后，必须先执行实体规范检查。检查失败时，先根据输出修复实体，再重新检查；检查通过前不得执行 CRUD 生成脚本。

```bash
dotnet run --file .claude/skills/crud-generator-plus/scripts/check_entities.cs -- \
  --path "Yi.Abp/module/{module}/Yi.Module.{Module}.Domain/Entities/{Entity}AggregateRoot.cs"
```

**检查要求**：
- 实体文件名与类名一致，后缀支持 `AggregateRoot.cs` 和 `Entity.cs`
- `AggregateRoot` 后缀类默认继承 `BaseAggregateRoot<Guid>`；明确需要乐观锁时允许继承 ABP 原生 `AggregateRoot<Guid>`
- `Entity` 类继承 `Entity<Guid>`
- 必须包含 `[SugarTable]`
- 必须包含 `[SugarColumn(IsPrimaryKey = true)]` 的 `Id` 主键
- 接口字段必须完整匹配：`ISoftDelete`、`IAuditedObject`、`IOrderNum`、`IState`
- 所有 `public` 属性必须包含 XML `summary`

## Step 3: 检查通过后执行脚本并启动并行 Agent

**关键**：只有实体规范检查通过后，才能执行 CRUD 生成脚本。脚本通常只需要几秒，脚本完成后必须立即并行启动两个子 Agent：种子数据 Agent 和生成后优化检查 Agent。主线程只负责调度、等待结果、汇总 diff 和执行最终构建验证，不要在主线程手工完成大段优化。

**Vben 5.7 基线**：生成的前端代码必须使用 `antdv-next`，不得再从 `ant-design-vue` 导入组件；应用内字典常量从 `#/constants` 导入。

### 3a: 调用脚本生成代码

```bash
dotnet run --file .claude/skills/crud-generator-plus/scripts/generate_crud.cs -- \
  --entity "Yi.Abp/module/{module}/Yi.Module.{Module}.Domain/Entities/{Entity}AggregateRoot.cs" \
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
Application/Services/{Entity}Service.cs              ← 包含 [OperLogEntity("{实体中文名}")] 、 PermissionResource、SelectListAsync 实现
```

**前端（5个文件）**:
```
Yi.Vben5/apps/web-antd/src/
├── api/{entity}/
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

### 新增功能：智能列表列生成

前端分页列表不再只展示第一个搜索字段。脚本会根据字段语义选择最多 6 个高价值业务字段，再追加排序、状态、备注、创建时间和操作列。

**列表列选择规则**：

| 字段类型/命名 | 示例 | 默认行为 |
|------|------|------|
| 主标题字段 | Name、{Entity}Name、Title | 优先显示，树形实体第一个列作为 treeNode |
| 编码/编号 | Code、No、Number | 优先显示 |
| 枚举字段 | TypeEnum、LevelEnum | 显示并使用字典渲染 |
| 业务数值 | Price、Amount、Count、Stock、Score | 显示 |
| 重要布尔字段 | IsDefault、IsRecommended、IsTop | 显示 |
| 联系方式 | Phone、Mobile、Email | 显示 |
| 外键 Guid | DeptId、UserId、CategoryId | 默认不显示原始 Guid |
| 长文本 | Content、Description、Json、Html、Body | 默认不显示 |
| 系统字段 | IsDeleted、CreatorId、LastModifierId、ConcurrencyStamp、ExtraProperties | 不显示 |

**标准列固定追加**：

```text
OrderNum、State、Remark、CreationTime、Action
```

### 树形实体额外输出

如果是树形实体（有 ParentId 字段），脚本额外生成：

```
Domain.Shared/Dtos/{Entity}TreeDto.cs
Domain/Repositories/I{Entity}Repository.cs
SqlSugarCore/Repositories/{Entity}Repository.cs
```

### 3b: 种子数据 Agent（单个 Agent 处理菜单和字典）

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
- 目录：Yi.Abp/module/rbac/Yi.Module.Rbac.SqlSugarCore/DataSeeds/{ModulePascal}DataSeed/
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

### 3c: 生成后优化检查 Agent

脚本生成完成后，必须自动启动一个独立子 Agent 执行生成后优化检查。该 Agent 可与种子数据 Agent 并行运行。不要等用户再次提醒，也不要只在主线程口头检查。

```markdown
任务：对刚生成的 CRUD 代码执行生成后优化检查，并直接修改生成文件。

输入：
- 模块：{module}
- 实体名：{Entity}
- 实体中文名：{实体中文名}
- 实体文件：Yi.Abp/module/{module}/Yi.Module.{Module}.Domain/Entities/{Entity}AggregateRoot.cs
- 后端生成目录：Yi.Abp/module/{module}/Yi.Module.{Module}.Application.Contracts/Dtos/{Entity}/
- 后端服务：Yi.Abp/module/{module}/Yi.Module.{Module}.Application/Services/{Entity}Service.cs
- 前端 API：Yi.Vben5/apps/web-antd/src/api/{entity}/
- 前端页面：Yi.Vben5/apps/web-antd/src/views/{module}/{entity}/

操作：
1. 搜索现有能力：
   - `rg -n "{Entity}|{FieldName}|SelectList|selectList|ApiSelect|TreeSelect" Yi.Vben5/apps/web-antd/src Yi.Abp/module`
   - 查找已有分类、类型、部门、用户、角色、字典、文件、图片等选择接口和组件。
2. 检查抽屉表单：
   - 关联 Id 字段不得保留普通 Input。
   - 枚举/字典字段必须使用 Select + DictEnum。
   - 金额、数量、排序使用 InputNumber。
   - 布尔业务字段使用 Switch 或 RadioGroup。
   - 图片/文件字段使用上传或资源选择组件。
   - 长文本使用 Textarea 或业务组件。
3. 检查列表和搜索：
   - 列表不直接显示 Guid 外键。
   - 需要展示关联名称时，补 DTO、Service 查询投影和前端列。
   - 搜索区分类/类型/枚举/状态使用下拉，时间使用范围选择。
4. 检查跨层一致性：
   - 后端 DTO、Service Select 投影、前端 model.d.ts、data.ts、drawer.vue 字段一致。
   - 跨模块关联只能依赖 Application.Contracts 或 Domain.Shared，禁止直接引用其他模块 Domain。
5. 输出报告：
   - 修改了哪些文件。
   - 哪些字段从默认 Input 优化成了业务组件。
   - 哪些字段保持默认以及原因。
   - 是否还存在需要人工确认的关联接口或业务组件。
```

## Step 4: 主线程验收 + 构建验证

主线程在两个子 Agent 都完成后再继续：

- 阅读种子数据 Agent 和生成后优化 Agent 的报告。
- 使用 `git diff -- Yi.Abp/module/{module} Yi.Vben5/apps/web-antd/src` 检查实际改动。
- 确认没有把关联字段、枚举字段、图片文件字段机械保留为普通输入框。
- 再执行后端和前端构建验证。

### 生成后优化检查规则

脚本生成的是基础 CRUD 初稿。生成完成后必须由生成后优化检查 Agent 做一次自动优化检查，尤其是前端抽屉表单、搜索项和列表列。不要把所有字段都保留为默认 `Input`，要根据字段语义和现有模块能力改成更适合业务的组件。

### 4.1 先查现有能力

优化前先搜索当前仓库已有模块和前端组件，不要重复造轮子：

```bash
rg -n "{EntityName}|{FieldName}|SelectList|selectList|ApiSelect|TreeSelect" Yi.Vben5/apps/web-antd/src Yi.Abp/module
```

检查重点：

- 是否已有产品分类、文章分类、部门、用户、角色、字典等可复用下拉接口。
- 是否已有 `{xxx}SelectList` API，可直接用于抽屉表单。
- 是否已有同类页面的 `data.ts`、`drawer.vue` 写法可参考。
- 是否字段名暗示枚举、字典、外键、文件、图片、金额、时间范围或长文本。

### 4.2 抽屉表单组件优化规则

| 字段场景 | 示例字段 | 默认生成问题 | 应优化为 |
|------|------|------|------|
| 关联分类 | ProductCategoryId、ArticleCategoryId、CategoryId | 原始 Guid 输入框不可用 | `ApiSelect` / `TreeSelect`，调用分类 `selectList` 或树接口 |
| 关联类型 | ProductTypeId、MaterialTypeId | 原始 Guid 输入框不可用 | `ApiSelect`，调用类型下拉接口 |
| 字典/枚举 | ProductTypeEnum、LevelEnum、StatusEnum | 普通输入框不可控 | `Select` + `getDictOptions(DictEnum.xxx)` |
| 部门/组织 | DeptId、ParentDeptId | 输入 Guid | `TreeSelect`，复用部门树或部门下拉 |
| 用户/角色 | UserId、RoleId | 输入 Guid | `ApiSelect` / `ApiTreeSelect`，展示名称、提交 Id |
| 图片 | CoverImageId、AvatarId、ImageUrl | 输入字符串或 Guid | 图片上传/资源选择组件，列表展示缩略图 |
| 文件 | FileId、AttachmentId | 输入 Guid | 文件上传/文件选择组件 |
| 开关 | IsDefault、IsRecommended、IsTop | 普通输入框 | `Switch` 或 `RadioGroup` |
| 金额/数量 | Price、Amount、Stock、Count、Score | 普通输入框 | `InputNumber`，必要时设置 precision/min/max |
| 时间 | StartTime、EndTime、PublishTime | 普通输入框 | `DatePicker` / `RangePicker` |
| 长文本 | Content、Description、Body、Json、Html | 单行输入框不合适 | `Textarea`，富文本/JSON 按业务组件处理 |
| 排序 | OrderNum | 文本输入 | `InputNumber`，默认值 0 |
| 状态 | State | 文本输入 | 启用/禁用 `RadioGroup` 或 `Switch` |

**示例：产品功能中已有产品分类时**

如果实体包含：

```csharp
public Guid ProductCategoryId { get; set; }
```

且项目已有产品分类下拉接口，则抽屉表单不能保留为输入框，应改为：

```typescript
{
  component: 'ApiSelect',
  componentProps: {
    api: productCategorySelectList,
    getPopupContainer,
    labelField: 'categoryName',
    valueField: 'id',
  },
  fieldName: 'productCategoryId',
  label: '产品分类',
  rules: 'selectRequired',
}
```

如果分类是树形结构，应优先改为 `TreeSelect` 或项目已有树选择组件。

**示例：产品类型已有枚举或字典时**

如果字段是：

```csharp
public ProductTypeEnum ProductType { get; set; }
```

抽屉表单应使用字典下拉：

```typescript
{
  component: 'Select',
  componentProps: {
    getPopupContainer,
    options: getDictOptions(DictEnum.PRODUCT_TYPE, true),
  },
  fieldName: 'productType',
  label: '产品类型',
  rules: 'selectRequired',
}
```

### 4.3 列表和搜索优化规则

- 外键字段不要直接在列表显示 Guid；若需要展示，应补充名称字段或在查询投影中映射名称。
- 分类、类型、状态、枚举字段应在搜索区使用下拉框，不要用输入框。
- 金额、数量、库存、评分等数值字段可显示在列表，但搜索区不要默认生成模糊查询。
- 长文本字段默认不进入列表；需要查看时放详情页、抽屉或单独预览。
- 图片字段在列表中优先显示缩略图，不显示原始地址或文件 Id。
- 时间字段用于搜索时优先使用范围选择；业务时间字段可显示，系统更新时间默认不显示。

### 4.4 后端联动优化规则

当前端需要展示关联名称时，不要只改前端：

- DTO 增加 `{RelationName}Name` 等展示字段。
- Service 查询中通过仓储、关联查询或应用服务补齐展示值。
- 前端列表列显示名称字段，表单提交仍提交 Id。
- 跨模块关联只能依赖 `Application.Contracts` 或 `Domain.Shared`，不要直接引用其他模块 Domain。

### 4.5 优化检查清单

生成后必须逐项确认：

- 抽屉表单中没有把 `CategoryId`、`TypeId`、`DeptId`、`UserId`、`RoleId` 这类关联字段保留为普通输入框。
- 枚举/字典字段已使用 `Select` + 字典选项。
- 布尔业务字段已使用 `Switch` 或 `RadioGroup`。
- 金额、数量、排序字段已使用 `InputNumber`。
- 图片/文件字段已使用上传或资源选择组件。
- 长文本字段未进入分页列表，表单使用 `Textarea` 或业务组件。
- 前端 `model.d.ts`、`data.ts`、抽屉表单和后端 DTO/Service 保持一致。
- 若引入新的关联下拉 API，已检查权限、路由、接口路径和类型导入。

## Step 5: 增量构建验证

### 后端验证

```bash
dotnet build Yi.Abp/module/{module}/Yi.Module.{Module}.Application/Yi.Module.{Module}.Application.csproj --no-restore
```

### 前端验证

**pnpm TypeScript 检查**（仅检查生成的模块目录，避免全项目检查耗时）：

```bash
pnpm -F @vben/web-antd run typecheck
```

**文件质量检查**：

```bash
# 检查关键文件内容
head -20 Yi.Abp/module/{module}/Yi.Module.{Module}.Application.Contracts/Dtos/{Entity}/{Entity}GetListOutputDto.cs
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
| API 路径 | `/api/{entity}` | `/{entity}` (requestClient 自动添加 /api) |
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
│   ├── check_entities.cs             # 实体规范检查脚本
│   └── generate_crud.cs              # 核心生成脚本
└── references/
    └── troubleshooting.md            # 常见问题（可选）
```

## 示例调用

**用户请求**:
> 在 product-manage 模块创建 Material CRUD。属性：id、名称、类型枚举（原材料=0, 半成品=1, 成品=2）。

**LLM 执行**:
1. 直接生成实体类（含默认审计字段）+ 枚举类
2. 运行实体规范检查脚本，失败则先修复实体并复查
3. 检查通过后**并行执行**：
   - 调用脚本生成代码
   - 启动种子数据 Agent
4. 构建验证
5. 输出完成报告

---

## ⚠️ 关键约束

1. **默认包含审计字段** — 用户未明确说明时，自动包含 State、OrderNum、Remark、IsDeleted、CreationTime、CreatorId、LastModifierId、LastModificationTime
2. **实体先检查再生成** — 生成实体类和枚举类后，必须先运行 `check_entities.cs`，检查通过前不得执行 CRUD 生成
3. **所有 public 属性必须有 XML summary** — 缺少 summary 属于错误，必须先修复
4. **脚本后并行启动 Agent** — 实体检查通过后先执行 Step 3a 脚本；脚本完成后立即并行启动 Step 3b 种子数据 Agent 和 Step 3c 生成后优化检查 Agent
5. **单个 Agent 处理种子数据** — 菜单和字典由一个 Agent 统一处理
6. **生成后必须做优化检查** — 关联字段、枚举字典、图片文件、金额数量、长文本等不能机械保留默认输入框
7. **仅构建 Application 项目** — 不构建整个解决方案
8. **不使用 Agent 生成 DTO/Service** — 直接调用脚本生成
9. **前端验证必须运行 pnpm typecheck** — 确保生成的 TypeScript 文件类型正确
10. **字典常量命名一致性** — dict-enum.ts 的常量名必须与 data.ts 引用完全匹配（格式：`{MODULE}_{ENTITY}_{ENUM}`）
