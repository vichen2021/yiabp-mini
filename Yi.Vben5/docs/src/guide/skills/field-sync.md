# 字段同步器

## 功能说明

同步实体字段变更到整个代码库，包括 DTOs、服务实现、前端 API 和视图、以及字典种子数据。当添加、删除或修改实体字段时，自动同步所有相关文件。

## 使用场景

- 向实体添加新字段
- 从实体删除字段
- 更改字段类型（例如，int 到 bool，添加枚举）
- 重命名实体字段

## 提示词示例

- `使用 field-sync 同步 @*AggregateRoot.cs 实体的字段变更`
- `同步 Role 实体的字段变更`
- `添加 Remark 和 PhoneNumber 字段到 Role 实体`

## 工作流程

### 步骤 1：理解变更

询问用户以明确：
- 正在修改哪个实体？
- 正在添加/删除/更改哪些字段？
- 新的字段类型是什么？
- 是否需要创建新的枚举？

### 步骤 2：更新后端 DTOs

#### 2.1 查找 DTO 位置

DTOs 位于：`Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application.Contracts/Dtos/{EntityName}/`

#### 2.2 更新 5 个 DTO 文件

对于以下每个文件，添加/删除/更新相应字段：

1. `{EntityName}GetOutputDto.cs` - 单条实体检索输出
2. `{EntityName}GetListOutputDto.cs` - 列表查询输出
3. `{EntityName}CreateInputVo.cs` - 创建输入
4. `{EntityName}UpdateInputVo.cs` - 更新输入
5. `{EntityName}GetListInputVo.cs` - 查询参数（如果字段不再需要过滤，则删除）

**类型映射：**
- C# `bool` → TypeScript `boolean`
- C# `int` → TypeScript `number`
- C# `Guid` → TypeScript `string`
- C# `Guid?` → TypeScript `string | null`
- C# Enum → TypeScript `number`（在导入中使用枚举名称）
- 新 Enum → 添加导入：`import type { EnumName } from '{module-path}/enums';`

### 步骤 3：更新服务实现

位置：`Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application/Services/{EntityName}Service.cs`

#### 3.1 更新 GetListAsync 方法

- 更新查询中的 `WhereIF` 条件
- 更新 `Select` 映射以包含新字段或删除旧字段

### 步骤 4：更新前端 API Model

位置：`Yi.Vben5/apps/web-antd/src/api/{module-name}/{entity-name}/model.d.ts`

更新 TypeScript 接口以匹配 DTO 变更。

### 步骤 5：更新前端视图数据

位置：`Yi.Vben5/apps/web-antd/src/views/{module-name}/{entity-name}/data.ts`

#### 5.1 更新列（用于表格显示）

添加或删除列定义：

```typescript
{
  field: 'fieldName',
  title: 'Display Name',
  minWidth: 100,
  formatter({ cellValue }) {
    // 对于布尔值
    return cellValue ? 'Yes' : 'No';
    // 对于枚举
    return cellValue === 1 ? 'Value1' : 'Value0';
  },
}
```

#### 5.2 更新 drawerSchema（用于创建/编辑表单）

添加或删除表单字段定义：

```typescript
// 布尔字段
{
  component: 'Switch',  // 或带选项的 'Select'
  fieldName: 'fieldName',
  label: 'Display Name',
}

// 枚举字段
{
  component: 'Select',
  componentProps: {
    options: [
      { label: 'Option 1', value: 0 },
      { label: 'Option 2', value: 1 },
    ],
  },
  fieldName: 'fieldName',
  label: 'Display Name',
}

// 外键字段（Guid）
{
  component: 'Input',  // 或用于下拉的 ApiSelect
  fieldName: 'fieldName',
  label: 'Display Name',
}
```

#### 5.3 更新 querySchema（用于搜索过滤器）

添加或删除查询条件。

### 步骤 6：添加字典种子数据（如需要）

如果字段是用于下拉框的布尔值或枚举，添加字典数据。

#### 6.1 更新 DictionaryTypeDataSeed.cs

位置：`Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryTypeDataSeed.cs`

添加新字典类型：

```csharp
DictionaryTypeAggregateRoot dictNew = new DictionaryTypeAggregateRoot()
{
    DictName = "Dictionary Type Name",
    DictType = "dict_type_key",
    OrderNum = 100,
    Remark = "Description",
    IsDeleted = false,
    State = true
};
entities.Add(dictNew);
```

#### 6.2 更新 DictionaryDataSeed.cs

位置：`Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/DictionaryDataSeed.cs`

添加字典数据条目：

```csharp
DictionaryEntity dictData1 = new DictionaryEntity()
{
    DictLabel = "Label 1",
    DictValue = "Value1",
    DictType = "dict_type_key",
    OrderNum = 100,
    Remark = "Description",
    IsDeleted = false,
    State = true,
    ListClass = "default"  // success, danger, warning, info, primary, default
};
entities.Add(dictData1);

DictionaryEntity dictData2 = new DictionaryEntity()
{
    DictLabel = "Label 2",
    DictValue = "Value2",
    DictType = "dict_type_key",
    OrderNum = 99,
    Remark = "Description",
    IsDeleted = false,
    State = true,
    ListClass = "primary"
};
entities.Add(dictData2);
```

### 步骤 7：验证

运行构建和类型检查以验证变更：

```powershell
# 后端
dotnet build Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application/Yi.Framework.{ModuleName}.Application.csproj

# 前端
cd Yi.Vben5/apps/web-antd
npm run typecheck
```

## 示例：向 Role 实体添加 Remark 和 PhoneNumber 字段

此示例展示为 RoleAggregateRoot 实体添加 `Remark`（string）和 `PhoneNumber`（string?）字段所需的变更。

### 后端变更：

**RoleGetOutputDto.cs：**
- 添加：`public string? Remark { get; set; }`
- 添加：`public string? PhoneNumber { get; set; }`

**RoleGetListOutputDto.cs：**
- 添加：`public string? Remark { get; set; }`

**RoleCreateInputVo.cs / RoleUpdateInputVo.cs：**
- 添加：`public string? Remark { get; set; }`
- 添加：`public string? PhoneNumber { get; set; }`

**RoleGetListInputVo.cs：**
- 添加（如需要过滤）：`public string? Remark { get; set; }`

**RoleService.cs：**
- 添加 WhereIF：`.WhereIF(!string.IsNullOrEmpty(input.Remark), x => x.Remark.Contains(input.Remark!))`
- 添加到 Select 映射：`Remark = x.Remark, PhoneNumber = x.PhoneNumber`

### 前端变更：

**api/system/role/model.d.ts：**
- 添加：`remark?: string | null;`
- 添加：`phoneNumber?: string | null;`

**views/system/role/data.ts：**
- 添加列条目：
  ```typescript
  { field: 'remark', title: '备注' }
  ```
- 添加 drawerSchema 条目：
  ```typescript
  { component: 'Input', fieldName: 'remark', label: '备注' }
  ```

### 字典种子（此示例不需要）：

由于两个字段都是普通字符串，不需要字典种子。

## 相关文档

- [CRUD 生成器](/guide/skills/crud-generator) - 了解 CRUD 生成器
- [技能创建器](/guide/skills/skill-creator) - 了解技能创建器
