# 表单组件

基于 Vben Form 与 antdv-next 的表单二次封装。业务页面通过 `#/adapter/form` 使用统一适配器，不直接在页面内组装底层 antdv-next 表单。

## 2.1 / Vben 5.7 约定

- 业务表单优先使用 `useVbenForm`，弹窗和抽屉中的表单也使用同一套 schema。
- antdv-next 基础组件由 `apps/web-antd/src/adapter/component/index.ts` 统一注册到表单组件适配器。
- 新页面不要再从 `ant-design-vue` 导入组件；需要手动导入时使用 `antdv-next`。
- 字典常量从 `#/constants` 导入，不再从 `@vben/constants` 导入。
- 抽屉、弹窗中的表单组件需要确认首次打开即可渲染真实组件，不能只显示组件名称文本。

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
import { DictEnum } from '#/constants';
import { getDictOptions } from '#/utils/dict';

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
      options: getDictOptions(DictEnum.SYS_NOTICE_TYPE),
    },
    rules: 'selectRequired',  // 选择必填
  },
  {
    component: 'Textarea',
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
import { getDictOptions } from '#/utils/dict';
import { DictEnum } from '#/constants';

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

## 常用组件

| 场景 | 推荐组件 | 说明 |
|------|----------|------|
| 文本 | `Input` | 默认文本输入 |
| 长文本 | `Textarea` | 备注、描述、内容 |
| 数值 | `InputNumber` | 排序、金额、数量 |
| 字典/枚举 | `Select` | 配合 `getDictOptions` |
| 布尔状态 | `Switch` / `RadioGroup` | 按业务展示选择 |
| 时间 | `DatePicker` / `RangePicker` | 单时间或时间范围 |
| 远程下拉 | `ApiSelect` / `ApiTreeSelect` | 关联用户、部门、分类等 |
| 上传 | `FileUpload` / `ImageUpload` | 文件和图片 |

生成器生成后必须检查关联 Id、枚举、图片、文件、金额、长文本等字段，不能机械保留为普通 `Input`。

## 相关文档

- [表格组件](/guide/frontend/components/table) - 表格使用
- [字典功能](/guide/frontend/features/dict) - 字典使用
