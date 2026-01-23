import type { FormSchemaGetter } from '#/adapter/form';
import type { VxeGridProps } from '#/adapter/vxe-table';

export const querySchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    fieldName: 'name',
    label: '字段名',
  },
  {
    component: 'Input',
    fieldName: 'tableId',
    label: '表ID',
  },
];

export const columns: VxeGridProps['columns'] = [
  { type: 'checkbox', width: 60 },
  {
    title: '字段名',
    field: 'name',
    width: 150,
  },
  {
    title: '字段类型',
    field: 'fieldType',
    width: 120,
  },
  {
    title: '长度',
    field: 'length',
    width: 80,
  },
  {
    title: '是否必填',
    field: 'isRequired',
    width: 100,
    slots: {
      default: ({ row }) => {
        return row.isRequired ? '是' : '否';
      },
    },
  },
  {
    title: '是否主键',
    field: 'isKey',
    width: 100,
    slots: {
      default: ({ row }) => {
        return row.isKey ? '是' : '否';
      },
    },
  },
  {
    title: '备注',
    field: 'description',
    minWidth: 150,
  },
  {
    title: '排序',
    field: 'orderNum',
    width: 80,
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
    fieldName: 'tableId',
    label: '表ID',
    rules: 'required',
  },
  {
    component: 'Input',
    fieldName: 'name',
    label: '字段名',
    rules: 'required',
  },
  {
    component: 'Select',
    fieldName: 'fieldType',
    label: '字段类型',
    componentProps: {
      options: [
        { label: 'String', value: 0 },
        { label: 'Int', value: 1 },
        { label: 'Long', value: 2 },
        { label: 'Bool', value: 3 },
        { label: 'Decimal', value: 4 },
        { label: 'DateTime', value: 5 },
        { label: 'Guid', value: 6 },
      ],
    },
    rules: 'required',
  },
  {
    component: 'InputNumber',
    fieldName: 'length',
    label: '长度',
    componentProps: {
      min: 0,
    },
  },
  {
    component: 'Switch',
    fieldName: 'isRequired',
    label: '是否必填',
    defaultValue: false,
  },
  {
    component: 'Switch',
    fieldName: 'isKey',
    label: '是否主键',
    defaultValue: false,
  },
  {
    component: 'Switch',
    fieldName: 'isAutoAdd',
    label: '是否自增',
    defaultValue: false,
  },
  {
    component: 'Switch',
    fieldName: 'isPublic',
    label: '是否公共',
    defaultValue: false,
  },
  {
    component: 'InputNumber',
    fieldName: 'orderNum',
    label: '排序',
    componentProps: {
      min: 0,
    },
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
