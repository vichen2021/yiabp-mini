import type { FormSchemaGetter } from '#/adapter/form';
import type { VxeGridProps } from '#/adapter/vxe-table';

export const querySchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    fieldName: 'name',
    label: '表名',
  },
  {
    component: 'Input',
    fieldName: 'description',
    label: '备注',
  },
];

export const columns: VxeGridProps['columns'] = [
  { type: 'checkbox', width: 60 },
  {
    title: '表名',
    field: 'name',
    width: 200,
  },
  {
    title: '备注',
    field: 'description',
    minWidth: 200,
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
    label: '表名',
    rules: 'required',
  },
  {
    component: 'Textarea',
    fieldName: 'description',
    formItemClass: 'items-start',
    label: '备注',
    componentProps: {
      autoSize: true,
    },
  },
];
