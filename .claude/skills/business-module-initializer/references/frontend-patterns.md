# Frontend Code Patterns

This document provides detailed code patterns for frontend implementation.

## API File Patterns

### Basic API Structure (with Pagination)

```typescript
import type { {EntityName} } from './model';

import type { ID, IDS, PageQuery, PageResult } from '#/api/common';

import { requestClient } from '#/api/request';

enum Api {
  {entityName}List = '/{entity-name}/list',
  {entityName}Select = '/{entity-name}/select-data-list',
  root = '/{entity-name}',
}

/**
 * {Entity name}分页列表
 * @param params 请求参数
 * @returns 列表
 */
export function {entityName}List(params?: PageQuery) {
  return requestClient.get<PageResult<{EntityName}>>(Api.root, { params });
}

export function {entityName}Info({entityName}Id: ID) {
  return requestClient.get<{EntityName}>(`${Api.root}/${{entityName}Id}`);
}

export function {entityName}Add(data: Partial<{EntityName}>) {
  return requestClient.postWithMsg<void>(Api.root, data);
}

export function {entityName}Update(data: Partial<{EntityName}>) {
  return requestClient.putWithMsg<void>(`${Api.root}/${data.id}`, data);
}

export function {entityName}Remove({entityName}Ids: IDS) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: {entityName}Ids.join(',') },
  });
}

/**
 * 获取{Entity name}下拉列表
 * @param keywords 关键词（可选）
 * @returns {Entity name}列表
 */
export function {entityName}OptionSelect(keywords?: string) {
  return requestClient.get<{EntityName}[]>(`${Api.{entityName}Select}?keywords=${keywords || ''}`);
}
```

### Model Definition

```typescript
export interface {EntityName} {
  creationTime: string;
  creatorId?: string | null;
  state: boolean;
  id: string;
  // Entity properties
  children?: {EntityName}[]; // If tree structure
}
```

## View File Patterns

### List View (index.vue)

```vue
<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';
import type { VxeGridProps } from '#/adapter/vxe-table';
import type { {EntityName} } from '#/api/system/{entity-name}/model';

import { nextTick } from 'vue';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { getVxePopupContainer } from '@vben/utils';

import { Popconfirm, Space } from 'ant-design-vue';

import { useVbenVxeGrid } from '#/adapter/vxe-table';
import { {entityName}List, {entityName}Remove } from '#/api/system/{entity-name}';

import { columns, querySchema } from './data';
import {entityName}Drawer from './{entity-name}-drawer.vue';

const formOptions: VbenFormProps = {
  commonConfig: {
    labelWidth: 80,
    componentProps: {
      allowClear: true,
    },
  },
  schema: querySchema(),
  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
  // 日期选择格式化（如果使用日期范围查询）
  fieldMappingTime: [
    [
      'creationTime',
      ['startTime', 'endTime'],
      ['YYYY-MM-DD 00:00:00', 'YYYY-MM-DD 23:59:59'],
    ],
  ],
};

const gridOptions: VxeGridProps = {
  checkboxConfig: {
    highlight: true,
    reserve: true,
  },
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: {},
  proxyConfig: {
    ajax: {
      query: async ({ page }, formValues = {}) => {
        return await {entityName}List({
          SkipCount: page.currentPage,
          MaxResultCount: page.pageSize,
          ...formValues,
        });
      },
    },
  },
  rowConfig: {
    keyField: 'id',
  },
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const [{EntityName}Drawer, drawerApi] = useVbenDrawer({
  connectedComponent: {entityName}Drawer,
});

function handleAdd() {
  drawerApi.setData({ update: false });
  drawerApi.open();
}

async function handleEdit(record: {EntityName}) {
  drawerApi.setData({ id: record.id, update: true });
  drawerApi.open();
}

async function handleDelete(row: {EntityName}) {
  await {entityName}Remove(row.id);
  await tableApi.query();
}
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="{Entity Name}列表">
      <template #toolbar-tools>
        <Space>
          <a-button
            type="primary"
            v-access:code="['system:{entity-name}:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </a-button>
        </Space>
      </template>
      <template #action="{ row }">
        <Space>
          <ghost-button
            v-access:code="['system:{entity-name}:edit']"
            @click="handleEdit(row)"
          >
            {{ $t('pages.common.edit') }}
          </ghost-button>
          <Popconfirm
            :get-popup-container="getVxePopupContainer"
            placement="left"
            title="确认删除？"
            @confirm="handleDelete(row)"
          >
            <ghost-button
              danger
              v-access:code="['system:{entity-name}:remove']"
              @click.stop=""
            >
              {{ $t('pages.common.delete') }}
            </ghost-button>
          </Popconfirm>
        </Space>
      </template>
    </BasicTable>
    <{EntityName}Drawer @reload="tableApi.query()" />
  </Page>
</template>
```

### Tree Structure List View

For tree structures, add tree configuration:

```typescript
const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

const gridOptions: VxeGridProps = {
  // ... other config
  proxyConfig: {
    ajax: {
      query: async (_, formValues = {}) => {
        const resp = await {entityName}List(formValues);
        const items = resp.map((item) => ({
          ...item,
          parentId: item.parentId === EMPTY_GUID ? null : item.parentId,
        }));
        return { items };
      },
      querySuccess: () => {
        eachTree(tableApi.grid.getData(), (item) => (item.expand = true));
        nextTick(() => {
          setExpandOrCollapse(true);
        });
      },
    },
  },
  treeConfig: {
    parentField: 'parentId',
    rowField: 'id',
    transform: true,
  },
};

function setExpandOrCollapse(expand: boolean) {
  eachTree(tableApi.grid.getData(), (item) => (item.expand = expand));
  tableApi.grid?.setAllTreeExpand(expand);
}
```

### Data File (data.ts)

```typescript
import type { FormSchemaGetter } from '#/adapter/form';
import type { VxeGridProps } from '#/adapter/vxe-table';

import { DictEnum } from '@vben/constants';
import { getPopupContainer } from '@vben/utils';

import { getDictOptions } from '#/utils/dict';
import { renderDict } from '#/utils/render';

export const querySchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    fieldName: 'configName',
    label: '参数名称',
  },
  {
    component: 'Input',
    fieldName: 'configKey',
    label: '参数键名',
  },
  {
    component: 'Select',
    componentProps: {
      getPopupContainer,
      options: getDictOptions(DictEnum.SYS_YES_NO),
    },
    fieldName: 'configType',
    label: '系统内置',
  },
  {
    component: 'RangePicker',
    fieldName: 'creationTime',
    label: '创建时间',
  },
];

export const columns: VxeGridProps['columns'] = [
  { type: 'checkbox', width: 60 },
  {
    title: '参数名称',
    field: 'configName',
  },
  {
    title: '参数KEY',
    field: 'configKey',
  },
  {
    title: '参数Value',
    field: 'configValue',
  },
  {
    title: '系统内置',
    field: 'configType',
    width: 120,
    slots: {
      default: ({ row }) => {
        return renderDict(row.configType, DictEnum.SYS_YES_NO);
      },
    },
  },
  {
    title: '备注',
    field: 'remark',
  },
  {
    title: '创建时间',
    field: 'creationTime',
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
    component: 'Input',
    fieldName: 'configName',
    label: '参数名称',
    rules: 'required',
  },
  {
    component: 'Input',
    fieldName: 'configKey',
    label: '参数键名',
    rules: 'required',
  },
  {
    component: 'Textarea',
    formItemClass: 'items-start',
    fieldName: 'configValue',
    label: '参数键值',
    componentProps: {
      autoSize: true,
    },
    rules: 'required',
  },
  {
    component: 'RadioGroup',
    componentProps: {
      buttonStyle: 'solid',
      options: getDictOptions(DictEnum.SYS_YES_NO),
      optionType: 'button',
    },
    defaultValue: 'N',
    fieldName: 'configType',
    label: '是否内置',
    rules: 'required',
  },
  {
    component: 'Textarea',
    fieldName: 'remark',
    formItemClass: 'items-start',
    label: '备注',
  },
];
```

### Drawer Component ({entity-name}-drawer.vue)

```vue
<script setup lang="ts">
import type { {EntityName} } from '#/api/system/{entity-name}/model';

import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { cloneDeep } from '@vben/utils';

import { useVbenForm } from '#/adapter/form';
import {
  {entityName}Add,
  {entityName}Info,
  {entityName}Update,
} from '#/api/system/{entity-name}';
import { defaultFormValueGetter, useBeforeCloseDiff } from '#/utils/popup';

import { drawerSchema } from './data';

const emit = defineEmits<{ reload: [] }>();

interface DrawerProps {
  id?: number | string;
  update: boolean;
}

const isUpdate = ref(false);
const title = computed(() => {
  return isUpdate.value ? $t('pages.common.edit') : $t('pages.common.add');
});

const [BasicForm, formApi] = useVbenForm({
  commonConfig: {
    componentProps: {
      class: 'w-full',
    },
    formItemClass: 'col-span-2',
    labelWidth: 80,
  },
  schema: drawerSchema(),
  showDefaultActions: false,
  wrapperClass: 'grid-cols-2',
});

const { onBeforeClose, markInitialized, resetInitialized } = useBeforeCloseDiff(
  {
    initializedGetter: defaultFormValueGetter(formApi),
    currentGetter: defaultFormValueGetter(formApi),
  },
);

const [BasicDrawer, drawerApi] = useVbenDrawer({
  onBeforeClose,
  onClosed: handleClosed,
  onConfirm: handleConfirm,
  async onOpenChange(isOpen) {
    if (!isOpen) {
      return null;
    }
    drawerApi.drawerLoading(true);

    const { id, update } = drawerApi.getData() as DrawerProps;
    isUpdate.value = update;

    if (id && update) {
      const record = await {entityName}Info(id);
      await formApi.setValues(record);
    }

    await markInitialized();
    drawerApi.drawerLoading(false);
  },
});

async function handleConfirm() {
  try {
    drawerApi.lock(true);
    const { valid } = await formApi.validate();
    if (!valid) {
      return;
    }
    const data = cloneDeep(await formApi.getValues());
    await (isUpdate.value ? {entityName}Update(data) : {entityName}Add(data));
    resetInitialized();
    emit('reload');
    drawerApi.close();
  } catch (error) {
    console.error(error);
  } finally {
    drawerApi.lock(false);
  }
}

async function handleClosed() {
  await formApi.resetForm();
  resetInitialized();
}
</script>

<template>
  <BasicDrawer :title="title" class="w-[600px]">
    <BasicForm />
  </BasicDrawer>
</template>
```

## Form Component Types

- `Input` - Text input
- `InputNumber` - Number input
- `Select` - Dropdown select
- `TreeSelect` - Tree select (for parent-child relationships)
- `RadioGroup` - Radio buttons
- `Textarea` - Multi-line text
- `DatePicker` - Date picker
- `Switch` - Toggle switch

## Common Utilities

- `getPopupContainer` - For dropdown positioning
- `renderDict` - For dictionary value rendering
- `cloneDeep` - For deep cloning objects
- `listToTree` - Convert flat list to tree structure
- `addFullName` - Add full path name for tree nodes

