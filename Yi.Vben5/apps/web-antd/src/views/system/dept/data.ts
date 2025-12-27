import type { FormSchemaGetter } from '#/adapter/form';
import type { VxeGridProps } from '#/adapter/vxe-table';

import { getPopupContainer } from '@vben/utils';

export const querySchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    fieldName: 'deptName',
    label: '部门名称',
  },
  {
    component: 'Select',
    componentProps: {
      getPopupContainer,
      options: [
        { label: '启用', value: true },
        { label: '禁用', value: false },
      ],
    },
    fieldName: 'state',
    label: '部门状态',
  },
];

export const columns: VxeGridProps['columns'] = [
  {
    field: 'deptName',
    title: '部门名称',
    treeNode: true,
  },
  {
    field: 'deptCode',
    title: '部门编码',
  },
  {
    field: 'orderNum',
    title: '排序',
  },
  {
    field: 'state',
    title: '状态',
    slots: {
      default: ({ row }) => {
        return row.state ? '启用' : '禁用';
      },
    },
  },
  {
    field: 'creationTime',
    title: '创建时间',
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
  },
  {
    component: 'TreeSelect',
    componentProps: {
      getPopupContainer,
    },
    dependencies: {
      show: (model) => model.parentId !== '00000000-0000-0000-0000-000000000000',
      triggerFields: ['parentId'],
    },
    fieldName: 'parentId',
    label: '上级部门',
    rules: 'selectRequired',
  },
  {
    component: 'Input',
    fieldName: 'deptName',
    label: '部门名称',
    rules: 'required',
  },
  {
    component: 'InputNumber',
    fieldName: 'orderNum',
    label: '显示排序',
    rules: 'required',
    defaultValue: 0,
  },
  {
    component: 'Input',
    fieldName: 'deptCode',
    label: '部门编码',
  },
  {
    component: 'Input',
    fieldName: 'leader',
    label: '负责人',
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
