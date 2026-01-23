import type { FormSchemaGetter } from '#/adapter/form';
import type { VxeGridProps } from '#/adapter/vxe-table';

export const querySchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    fieldName: 'name',
    label: '模板名称',
  },
];

export const columns: VxeGridProps['columns'] = [
  { type: 'checkbox', width: 60 },
  {
    title: '模板名称',
    field: 'name',
    width: 150,
  },
  {
    title: '生成路径',
    field: 'buildPath',
    minWidth: 250,
  },
  {
    title: '备注',
    field: 'remarks',
    minWidth: 150,
  },
  {
    title: '创建时间',
    field: 'creationTime',
    width: 180,
  },
  {
    field: 'action',
    fixed: 'right',
    slots: { default: 'action' },
    title: '操作',
    resizable: false,
    width: 'auto',
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
    label: 'ID',
  },
  {
    component: 'Input',
    fieldName: 'name',
    label: '模板名称',
    rules: 'required',
  },
  {
    component: 'Input',
    fieldName: 'buildPath',
    label: '生成路径',
    rules: 'required',
    componentProps: {
      placeholder: '例如: D:\\code\\Entities\\@ModelEntity.cs',
    },
  },
  {
    component: 'Textarea',
    fieldName: 'templateStr',
    formItemClass: 'items-start',
    label: '模板内容',
    componentProps: {
      autoSize: { minRows: 10, maxRows: 20 },
      placeholder: '支持占位符: @Model, @model, @field, @namespace',
    },
    rules: 'required',
  },
  {
    component: 'Textarea',
    fieldName: 'remarks',
    formItemClass: 'items-start',
    label: '备注',
    componentProps: {
      autoSize: true,
    },
  },
];
