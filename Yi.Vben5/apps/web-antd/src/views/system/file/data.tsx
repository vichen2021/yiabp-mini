import type { FormSchemaGetter } from '#/adapter/form';
import type { VxeGridProps } from '#/adapter/vxe-table';

const fileTypeTextMap: Record<string, string> = {
  0: '普通文件',
  1: '图片',
  2: '缩略图',
  3: 'Excel',
  4: '临时文件',
  excel: 'Excel',
  file: '普通文件',
  image: '图片',
  temp: '临时文件',
  thumbnail: '缩略图',
};

const providerTextMap: Record<string, string> = {
  Aliyun: '云端',
  FileSystem: '本地',
};

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
    title: '文件后缀',
    field: 'extension',
    width: 100,
    formatter: ({ cellValue }) => cellValue || '-',
  },
  {
    title: '文件分类',
    field: 'fileType',
    width: 100,
    formatter: ({ cellValue }) => fileTypeTextMap[String(cellValue)] ?? cellValue ?? '-',
  },
  {
    title: '存储来源',
    field: 'provider',
    width: 100,
    formatter: ({ cellValue }) => providerTextMap[String(cellValue)] ?? cellValue ?? '-',
  },
  {
    title: 'MIME类型',
    field: 'contentType',
    width: 180,
    showOverflow: true,
  },
  {
    title: '存储Key',
    field: 'storageKey',
    width: 180,
    showOverflow: true,
  },
  {
    title: 'Hash',
    field: 'hash',
    width: 180,
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
