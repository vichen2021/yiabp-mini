import type { FormSchemaGetter } from '#/adapter/form';
import type { VxeGridProps } from '#/adapter/vxe-table';

/**
 * 查询表单 - 对应后端 FileGetListInputVo（fileName, startCreationTime, endCreationTime）
 */
export const querySchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    fieldName: 'fileName',
    label: '文件名称',
  },
  {
    component: 'RangePicker',
    fieldName: 'creationTime',
    label: '创建时间',
  },
];

/**
 * 表格列 - 对应后端 FileGetListOutputDto（id, fileSize, beautifySize, contentType, fileName, creationTime）
 */
export const columns: VxeGridProps['columns'] = [
  { type: 'checkbox', width: 60 },
  {
    title: '文件名称',
    field: 'fileName',
    showOverflow: true,
  },
  {
    title: '可读大小',
    field: 'beautifySize',
    width: 100,
  },
  {
    title: '文件类型',
    field: 'contentType',
    width: 140,
    showOverflow: true,
  },
  {
    title: '创建时间',
    field: 'creationTime',
    sortable: true,
    width: 170,
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
