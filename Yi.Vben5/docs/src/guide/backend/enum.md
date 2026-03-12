# 枚举使用规范

## 强制规则

**具有 2 个及以上预定义值的字段必须使用枚举，禁止使用魔法字符串或魔法数字。**

## 后端枚举定义

枚举定义在 `Domain.Shared/Enums/` 目录下，使用 `[Description]` 特性标注中文说明，值从 0 开始递增：

```csharp
public enum StateEnum
{
    [Description("禁用")] Disable = 0,
    [Description("启用")] Enable = 1,
}
```

## 实体和 DTO 中使用

实体和 DTO 中直接使用枚举类型（SqlSugar 自动映射为 int 存储）：

```csharp
// 实体
public StateEnum State { get; set; } = StateEnum.Enable;

// 查询输入（可空，用于条件过滤）
public StateEnum? State { get; set; }
```

## 字典种子数据同步

若枚举需供外部使用，则枚举必须在 RBAC 模块的字典种子数据中注册对应的字典类型和字典项，供前端管理后台下拉框使用：

- `DictionaryTypeDataSeed.cs` — 注册字典类型
- `DictionaryDataSeed.cs` — 注册字典项，`DictValue` 为枚举，字符串形式（如 `"0"`, `"Woman"`）

## 前端管理后台（Vben5）

在 `dict-enum.ts` 中注册字典常量，表单和表格中通过字典渲染：

```typescript
// dict-enum.ts
export const DictEnum = {
  STATE: 'state',
  // ...
} as const;

// data.ts — 下拉选项（字典值转为 number）
const stateOptions = () =>
  getDictOptions(DictEnum.STATE).map((o) => ({
    ...o,
    value: Number(o.value),
  }));

// data.ts — 表格列渲染
{ field: 'state', title: '状态', slots: {
    default: ({ row }) => renderDict(row.state, DictEnum.STATE),
  },
},
```

## 相关文档

- [实体定义](/guide/backend/entity) - 了解实体定义
- [模块开发](/guide/backend/module) - 了解应用服务开发

