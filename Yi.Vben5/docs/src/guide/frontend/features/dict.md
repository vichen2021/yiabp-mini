# 字典功能

字典用于管理系统中各类下拉选项，如状态、类型等。

## 使用方式

### 获取字典选项

```typescript
import { getDictOptions } from '#/utils/dict';
import { DictEnum } from '@vben/constants';

// 用于 Select/Radio/Checkbox 组件
const options = getDictOptions(DictEnum.SYS_STATUS);
```

### 渲染字典标签

```typescript
import { renderDict } from '#/utils/render';

// 在表格列中使用
{
  field: 'status',
  title: '状态',
  slots: {
    default: ({ row }) => renderDict(row.status, DictEnum.SYS_STATUS),
  },
}
```

## 字典枚举

字典枚举定义在 `@vben/constants` 中：

```typescript
// dict-enum.ts
export const DictEnum = {
  SYS_STATUS: 'sys_status',
  SYS_YES_NO: 'sys_yes_no',
  SYS_NOTICE_TYPE: 'sys_notice_type',
} as const;
```

## 注意事项

### 异步特性

`getDictOptions` 是异步封装，直接在业务逻辑中使用会拿到空数组：

```typescript
// ❌ 错误用法
function test() {
  const options = getDictOptions(DictEnum.SYS_STATUS);
  // options 为空数组
}

// ✅ 正确用法 - 在 API 中获取
import { dictDataInfo } from '#/api/system/dict-data';

async function test() {
  const data = await dictDataInfo('sys_status');
}
```

### 过滤字典选项

使用 `computed` 处理过滤：

```typescript
{
  component: 'Select',
  componentProps: {
    options: computed(() => {
      return getDictOptions(DictEnum.SYS_STATUS)
        .filter(item => item.value !== '0');
    }),
  },
}
```

### 类型转换

如果业务对象为 `number` 类型，使用第二个参数：

```typescript
// value 转为 number 类型
getDictOptions(DictEnum.SYS_STATUS, true)
```

::: warning 注意
确保同一字典在项目中使用相同的类型转换，否则会出现缓存问题。
:::

## 相关文档

- [表单组件](/guide/frontend/components/form) - 表单中使用字典
- [表格组件](/guide/frontend/components/table) - 表格中渲染字典
