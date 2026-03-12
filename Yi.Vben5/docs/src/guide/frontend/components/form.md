# 表单组件

基于 Ant Design Vue 的表单二次封装。

## 基本使用

```typescript
import { useVbenForm } from '#/adapter/form';

const [BasicForm, formApi] = useVbenForm({
  commonConfig: {
    labelWidth: 80,
    componentProps: {
      class: 'w-full',
    },
  },
  schema: formSchema(),
  showDefaultActions: false,  // Modal/Drawer 中使用时关闭
  wrapperClass: 'grid-cols-2', // 两列布局
});
```

## Schema 配置

```typescript
export const formSchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    fieldName: 'name',
    label: '名称',
    rules: 'required',  // 必填
  },
  {
    component: 'Select',
    fieldName: 'type',
    label: '类型',
    componentProps: {
      options: getDictOptions(DictEnum.SYS_TYPE),
    },
    rules: 'selectRequired',  // 选择必填
  },
  {
    component: 'InputTextArea',
    fieldName: 'remark',
    label: '备注',
    formItemClass: 'col-span-2',  // 跨两列
  },
];
```

## 表单操作

```typescript
// 获取表单值
const values = await formApi.getValues();

// 设置表单值
await formApi.setValues({ name: 'test' });

// 重置表单
await formApi.resetForm();

// 验证表单
await formApi.validate();

// 更新 Schema
formApi.updateSchema([
  {
    fieldName: 'type',
    componentProps: {
      options: newOptions,
    },
  },
]);
```

## 校验规则

### 内置规则

```typescript
rules: 'required',        // 输入必填
rules: 'selectRequired',  // 选择必填
```

### Zod 校验

```typescript
import { z } from '#/adapter';

// 手机号校验
{
  rules: z.string()
    .regex(/^1[3-9]\d{9}$/, '请输入正确的手机号')
    .optional()
    .or(z.literal('')),
}

// 邮箱必填
{
  rules: z.string().email('请输入正确的邮箱'),
}
```

## 字典过滤

使用 `computed` 处理字典过滤：

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

## 相关文档

- [表格组件](/guide/frontend/components/table) - 表格使用
- [字典功能](/guide/frontend/features/dict) - 字典使用
