import type { FormSchemaGetter } from '#/adapter/form';
import type { VxeGridProps } from '#/adapter/vxe-table';

export const querySchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    fieldName: 'packageName',
    label: '套餐名称',
  },
];

export const columns: VxeGridProps['columns'] = [
  { type: 'checkbox', width: 60 },
  {
    title: '套餐名称',
    field: 'packageName',
  },
  {
    title: '备注',
    field: 'remark',
  },
  {
    title: '状态',
    field: 'state',
    slots: { default: 'status' },
  },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '操作',
    resizable: false,
    width: 200,
  },
];

export const drawerSchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    dependencies: {
      show: () => false,
      triggerFields: [''],
    },
    fieldName: 'id',
  },
  {
    component: 'Radio',
    dependencies: {
      show: () => false,
      triggerFields: [''],
    },
    fieldName: 'menuCheckStrictly',
  },
  {
    component: 'Input',
    fieldName: 'packageName',
    label: '套餐名称',
    rules: 'required',
  },
  {
    component: 'menuIds',
    defaultValue: [],
    fieldName: 'menuIds',
    label: '关联菜单',
  },
  {
    component: 'Textarea',
    fieldName: 'remark',
    label: '备注',
  },
  {
    component: 'RadioGroup',
    componentProps: {
      buttonStyle: 'solid',
      options: [
        { label: '启用', value: true },
        { label: '禁用', value: false },
      ],
      optionType: 'button',
    },
    defaultValue: true,
    fieldName: 'state',
    label: '状态',
  },
];
