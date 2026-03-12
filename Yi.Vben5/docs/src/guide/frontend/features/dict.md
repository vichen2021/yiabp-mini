# 字典功能

字典用于管理系统中各类下拉选项，如状态、类型等。

## 使用方式

### 获取字典选项

```typescript
import { getDictOptions } from '#/utils/dict';
import { DictEnum } from '@vben/constants';

// 用于 Select/Radio/Checkbox 组件
const options = getDictOptions(DictEnum.SYS_NORMAL_DISABLE);
```

### 渲染字典标签

```typescript
import { renderDict } from '#/utils/render';

// 在表格列中使用
{
  field: 'status',
  title: '状态',
  slots: {
    default: ({ row }) => renderDict(row.state, DictEnum.SYS_NORMAL_DISABLE),
  },
}
```

## 字典枚举

字典枚举定义在 `@vben/constants` 中：

```typescript
// packages/@core/base/shared/src/constants/dict-enum.ts
export const DictEnum = {
  SYS_COMMON_STATUS: 'sys_common_status',
  SYS_DB_TYPE: 'sys_db_type',
  SYS_DEVICE_TYPE: 'sys_device_type',
  SYS_GRANT_TYPE: 'sys_grant_type',
  SYS_NORMAL_DISABLE: 'sys_normal_disable',
  SYS_NOTICE_STATUS: 'sys_notice_status',
  SYS_NOTICE_TYPE: 'sys_notice_type',
  SYS_OPER_TYPE: 'sys_oper_type',
  SYS_OSS_ACCESS_POLICY: 'oss_access_policy',
  SYS_SHOW_HIDE: 'sys_show_hide',
  SYS_USER_SEX: 'sys_user_sex',
  SYS_YES_NO: 'sys_yes_no',
  WF_BUSINESS_STATUS: 'wf_business_status',
  WF_FORM_TYPE: 'wf_form_type',
  WF_TASK_STATUS: 'wf_task_status',
} as const;
```

## 注意事项

### 异步特性

`getDictOptions` 是异步封装，直接在业务逻辑中使用会拿到空数组：

```typescript
// ❌ 错误用法
function test() {
  const options = getDictOptions(DictEnum.SYS_NORMAL_DISABLE);
  // options 为空数组
}

// ✅ 正确用法 - 在 API 中获取
import { dictDataInfo } from '#/api/system/dict/dict-data';

async function test() {
  const data = await dictDataInfo('sys_normal_disable');
}
```

### 过滤字典选项

使用 `computed` 处理过滤：

```typescript
{
  component: 'Select',
  componentProps: {
    options: computed(() => {
      return getDictOptions(DictEnum.SYS_NORMAL_DISABLE)
        .filter(item => item.value !== '0');
    }),
  },
}
```

### 类型转换

如果业务对象为 `number` 类型，使用第二个参数：

```typescript
// value 转为 number 类型
getDictOptions(DictEnum.SYS_NORMAL_DISABLE, true)
```

::: warning 注意
确保同一字典在项目中使用相同的类型转换，否则会出现缓存问题。
:::

## 相关文档

- [表单组件](/guide/frontend/components/form) - 表单中使用字典
- [表格组件](/guide/frontend/components/table) - 表格中渲染字典
