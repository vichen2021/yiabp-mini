import type { FormSchemaGetter } from '#/adapter/form';
import type { VxeGridProps } from '#/adapter/vxe-table';

import { getPopupContainer } from '@vben/utils';

export const querySchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    fieldName: 'name',
    label: '字段名',
  },
  {
    component: 'Select',
    fieldName: 'tableId',
    label: '数据表',
    componentProps: {
      getPopupContainer,
      showSearch: true,
      allowClear: true,
      placeholder: '请选择数据表',
      options: [], // 将在组件中动态加载
    },
  },
];

export const columns: VxeGridProps['columns'] = [
  { type: 'checkbox', width: 60 },
  {
    title: '表名',
    field: 'tableId',
    width: 150,
    slots: {
      default: ({ row }) => {
        // 表名将通过外部传入的 tableMap 来显示
        return row.tableName || row.tableId;
      },
    },
  },
  {
    title: '字段名',
    field: 'name',
    width: 150,
  },
  {
    title: '字段类型',
    field: 'fieldType',
    width: 120,
    slots: {
      default: ({ row }) => {
        const typeMap: Record<number, string> = {
          0: 'String',
          1: 'Int',
          2: 'Long',
          3: 'Bool',
          4: 'Decimal',
          5: 'DateTime',
          6: 'Guid',
        };
        return typeMap[row.fieldType] || row.fieldType;
      },
    },
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
    component: 'Select',
    fieldName: 'tableId',
    label: '数据表',
    componentProps: {
      getPopupContainer,
      showSearch: true,
      allowClear: true,
      placeholder: '请选择数据表',
      options: [], // 将在组件中动态加载
    },
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
      getPopupContainer,
      options: [], // 将在组件中动态加载
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
