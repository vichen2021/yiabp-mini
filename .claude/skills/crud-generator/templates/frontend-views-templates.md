# Frontend Views Templates

## ⚠️ 重要：参考项目实际代码模式

模板基于 `src/views/product/item/` 的实际代码，使用 **Vben5 + Ant Design Vue + VxeGrid** 的实际模式。

---

## 导入路径对照表

| 旧模板路径 | 正确路径 | 说明 |
|------------|----------|------|
| `'#/types'` 或 `'#/types/form'` | `'#/adapter/form'` | 表单类型 |
| `'vxe-table'` | `'#/adapter/vxe-table'` | VxeGrid 类型 |
| `'#/hooks'` | `'#/adapter/form'` / `'#/adapter/vxe-table'` | Hook |
| `'#/utils/render'` (getDictOptions) | `'#/utils/dict'` | 字典选项 |
| `'#/utils/render'` (renderDict) | `'#/utils/render'` | 渲染字典 |

---

## data.ts Template (普通实体)

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
    fieldName: '{{entityNameLower}}Name',
    label: '{{entityComment}}名称',
  },
  {{#each enumFields}}
  {
    component: 'Select',
    componentProps: {
      getPopupContainer,
      options: getDictOptions(DictEnum.{{dictConstant}}, true),
    },
    fieldName: '{{name}}',
    label: '{{label}}',
  },
  {{/each}}
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
    label: '状态',
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
    title: '{{entityComment}}名称',
    field: '{{entityNameLower}}Name',
  },
  {{#each enumFields}}
  {
    title: '{{label}}',
    field: '{{name}}',
    width: 120,
    slots: {
      default: ({ row }) => {
        return renderDict(row.{{name}}, DictEnum.{{dictConstant}});
      },
    },
  },
  {{/each}}
  {
    title: '排序',
    field: 'orderNum',
    width: 80,
  },
  {
    title: '状态',
    field: 'state',
    width: 100,
    slots: {
      default: ({ row }) => {
        return renderDict(String(row.state), DictEnum.SYS_NORMAL_DISABLE);
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
    fieldName: '{{entityNameLower}}Name',
    label: '{{entityComment}}名称',
    rules: 'required',
  },
  {{#each enumFields}}
  {
    component: 'Select',
    componentProps: {
      getPopupContainer,
      options: getDictOptions(DictEnum.{{dictConstant}}, true),
    },
    fieldName: '{{name}}',
    label: '{{label}}',
    rules: 'required',
  },
  {{/each}}
  {
    component: 'InputNumber',
    fieldName: 'orderNum',
    label: '排序',
    defaultValue: 0,
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
```

---

## data.ts Template (树形实体)

```typescript
import type { FormSchemaGetter } from '#/adapter/form';
import type { VxeGridProps } from '#/adapter/vxe-table';

import { DictEnum } from '@vben/constants';
import { getPopupContainer } from '@vben/utils';

import { getDictOptions } from '#/utils/dict';
import { renderDict } from '#/utils/render';

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

export const querySchema: FormSchemaGetter = () => [
  {
    component: 'Input',
    fieldName: '{{entityNameLower}}Name',
    label: '{{entityComment}}名称',
  },
  {{#each enumFields}}
  {
    component: 'Select',
    componentProps: {
      getPopupContainer,
      options: getDictOptions(DictEnum.{{dictConstant}}, true),
    },
    fieldName: '{{name}}',
    label: '{{label}}',
  },
  {{/each}}
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
    label: '状态',
  },
];

export const columns: VxeGridProps['columns'] = [
  { type: 'checkbox', width: 60 },
  {
    title: '{{entityComment}}名称',
    field: '{{entityNameLower}}Name',
    treeNode: true,
  },
  {{#each enumFields}}
  {
    title: '{{label}}',
    field: '{{name}}',
    width: 120,
    slots: {
      default: ({ row }) => {
        return renderDict(row.{{name}}, DictEnum.{{dictConstant}});
      },
    },
  },
  {{/each}}
  {
    title: '排序',
    field: 'orderNum',
    width: 80,
  },
  {
    title: '状态',
    field: 'state',
    width: 100,
    slots: {
      default: ({ row }) => {
        return renderDict(String(row.state), DictEnum.SYS_NORMAL_DISABLE);
      },
    },
  },
  {
    title: '备注',
    field: 'remark',
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
    component: 'ApiTreeSelect',
    componentProps: {
      api: async () => {
        const data = await {{entityNameLower}}Tree();
        return data;
      },
      fieldNames: { label: '{{entityNameLower}}Name', value: 'id', children: 'children' },
      showSearch: true,
      treeDefaultExpandAll: true,
      getPopupContainer,
    },
    fieldName: 'parentId',
    label: '上级{{entityComment}}',
  },
  {
    component: 'Input',
    fieldName: '{{entityNameLower}}Name',
    label: '{{entityComment}}名称',
    rules: 'required',
  },
  {{#each enumFields}}
  {
    component: 'Select',
    componentProps: {
      getPopupContainer,
      options: getDictOptions(DictEnum.{{dictConstant}}, true),
    },
    fieldName: '{{name}}',
    label: '{{label}}',
    rules: 'required',
  },
  {{/each}}
  {
    component: 'InputNumber',
    fieldName: 'orderNum',
    label: '排序',
    defaultValue: 0,
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
```

---

## index.vue Template (普通实体)

```vue
<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { {{entityName}} } from '#/api/{{moduleName}}/{{entityNameLower}}/model';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { getVxePopupContainer } from '@vben/utils';

import { Modal, Popconfirm, Space } from 'ant-design-vue';

import { useVbenVxeGrid, vxeCheckboxChecked } from '#/adapter/vxe-table';
import { {{entityNameLower}}List, {{entityNameLower}}Remove } from '#/api/{{moduleName}}/{{entityNameLower}}';

import { columns, querySchema } from './data';
import {{entityNameLower}}Drawer from './{{entityNameLower}}-drawer.vue';

const formOptions: VbenFormProps = {
  commonConfig: {
    labelWidth: 80,
    componentProps: {
      allowClear: true,
    },
  },
  schema: querySchema(),
  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
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
        return await {{entityNameLower}}List({
          SkipCount: (page.currentPage - 1) * page.pageSize,
          MaxResultCount: page.pageSize,
          ...formValues,
        });
      },
    },
  },
  rowConfig: {
    keyField: 'id',
  },
  id: '{{moduleName}}-{{entityNameLower}}-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const [{{entityName}}Drawer, drawerApi] = useVbenDrawer({
  connectedComponent: {{entityNameLower}}Drawer,
});

function handleAdd() {
  drawerApi.setData({ update: false });
  drawerApi.open();
}

async function handleEdit(record: {{entityName}}) {
  drawerApi.setData({ id: record.id, update: true });
  drawerApi.open();
}

async function handleDelete(row: {{entityName}}) {
  await {{entityNameLower}}Remove([row.id]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: {{entityName}}) => row.id);
  Modal.confirm({
    title: '提示',
    okType: 'danger',
    content: `确认删除选中的${ids.length}条记录吗？`,
    onOk: async () => {
      await {{entityNameLower}}Remove(ids);
      await tableApi.query();
    },
  });
}
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="{{entityComment}}列表">
      <template #toolbar-tools>
        <Space>
          <a-button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['{{moduleName}}:{{entityNameLower}}:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </a-button>
          <a-button
            type="primary"
            v-access:code="['{{moduleName}}:{{entityNameLower}}:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </a-button>
        </Space>
      </template>
      <template #action="{ row }">
        <Space>
          <ghost-button
            v-access:code="['{{moduleName}}:{{entityNameLower}}:edit']"
            @click.stop="handleEdit(row)"
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
              v-access:code="['{{moduleName}}:{{entityNameLower}}:remove']"
              @click.stop=""
            >
              {{ $t('pages.common.delete') }}
            </ghost-button>
          </Popconfirm>
        </Space>
      </template>
    </BasicTable>
    <{{entityName}}Drawer @reload="tableApi.query()" />
  </Page>
</template>
```

---

## index.vue Template (树形实体)

```vue
<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { {{entityName}} } from '#/api/{{moduleName}}/{{entityNameLower}}/model';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { eachTree, getVxePopupContainer } from '@vben/utils';

import { Modal, Popconfirm, Space } from 'ant-design-vue';

import { useVbenVxeGrid, vxeCheckboxChecked } from '#/adapter/vxe-table';
import { {{entityNameLower}}List, {{entityNameLower}}Remove } from '#/api/{{moduleName}}/{{entityNameLower}}';

import { columns, querySchema } from './data';
import {{entityNameLower}}Drawer from './{{entityNameLower}}-drawer.vue';

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

const formOptions: VbenFormProps = {
  commonConfig: {
    labelWidth: 80,
    componentProps: {
      allowClear: true,
    },
  },
  schema: querySchema(),
  wrapperClass: 'grid-cols-1 md:grid-cols-2',
};

const gridOptions: VxeGridProps = {
  checkboxConfig: {
    highlight: true,
    reserve: true,
  },
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: { enabled: false },
  rowConfig: {
    keyField: 'id',
  },
  treeConfig: {
    parentField: 'parentId',
    rowField: 'id',
    transform: true,
  },
  proxyConfig: {
    ajax: {
      query: async (_, formValues = {}) => {
        const res = await {{entityNameLower}}List(formValues);
        return res.map((item: {{entityName}}) => ({
          ...item,
          parentId: item.parentId === EMPTY_GUID ? null : item.parentId,
        }));
      },
    },
  },
  toolbarConfig: {
    custom: true,
    refresh: { code: 'query' },
  },
  id: '{{moduleName}}-{{entityNameLower}}-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

// 查询后默认展开
tableApi.on('query', () => {
  eachTree(tableApi.grid?.getData() || [], (item) => {
    tableApi.grid?.setTreeExpand(item, true);
  });
});

const [{{entityName}}Drawer, drawerApi] = useVbenDrawer({
  connectedComponent: {{entityNameLower}}Drawer,
});

function handleAdd() {
  drawerApi.setData({ update: false });
  drawerApi.open();
}

function handleAddChild(row: {{entityName}}) {
  drawerApi.setData({ update: false, parentId: row.id });
  drawerApi.open();
}

async function handleEdit(record: {{entityName}}) {
  drawerApi.setData({ id: record.id, update: true });
  drawerApi.open();
}

async function handleDelete(row: {{entityName}}) {
  await {{entityNameLower}}Remove([row.id]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: {{entityName}}) => row.id);
  Modal.confirm({
    title: '提示',
    okType: 'danger',
    content: `确认删除选中的${ids.length}条记录吗？`,
    onOk: async () => {
      await {{entityNameLower}}Remove(ids);
      await tableApi.query();
    },
  });
}

function handleExpandAll() {
  tableApi.grid?.setAllTreeExpand(true);
}

function handleCollapseAll() {
  tableApi.grid?.clearTreeExpand();
}
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="{{entityComment}}列表">
      <template #toolbar-tools>
        <Space>
          <a-button @click="handleExpandAll">
            展开全部
          </a-button>
          <a-button @click="handleCollapseAll">
            折叠全部
          </a-button>
          <a-button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['{{moduleName}}:{{entityNameLower}}:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </a-button>
          <a-button
            type="primary"
            v-access:code="['{{moduleName}}:{{entityNameLower}}:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </a-button>
        </Space>
      </template>
      <template #action="{ row }">
        <Space>
          <ghost-button
            v-access:code="['{{moduleName}}:{{entityNameLower}}:add']"
            @click.stop="handleAddChild(row)"
          >
            新增子节点
          </ghost-button>
          <ghost-button
            v-access:code="['{{moduleName}}:{{entityNameLower}}:edit']"
            @click.stop="handleEdit(row)"
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
              v-access:code="['{{moduleName}}:{{entityNameLower}}:remove']"
              @click.stop=""
            >
              {{ $t('pages.common.delete') }}
            </ghost-button>
          </Popconfirm>
        </Space>
      </template>
    </BasicTable>
    <{{entityName}}Drawer @reload="tableApi.query()" />
  </Page>
</template>
```

---

## drawer.vue Template

```vue
<script setup lang="ts">
import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { cloneDeep } from '@vben/utils';

import { useVbenForm } from '#/adapter/form';
import type { {{entityName}}CreateInput, {{entityName}}UpdateInput } from '#/api/{{moduleName}}/{{entityNameLower}}/model';
import { {{entityNameLower}}Add, {{entityNameLower}}Info, {{entityNameLower}}Update } from '#/api/{{moduleName}}/{{entityNameLower}}';
import { defaultFormValueGetter, useBeforeCloseDiff } from '#/utils/popup';

import { drawerSchema } from './data';

const emit = defineEmits<{ reload: [] }>();

interface DrawerProps {
  id?: number | string;
  update: boolean;
  {{#if isTree}}
  parentId?: string;
  {{/if}}
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

    const { id, update, {{#if isTree}}parentId{{/if}} } = drawerApi.getData() as DrawerProps;
    isUpdate.value = update;

    {{#if isTree}}
    if (parentId && !update) {
      await formApi.setValues({ parentId });
    }
    {{/if}}

    if (id && update) {
      const record = await {{entityNameLower}}Info(id);
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
    const data = cloneDeep(await formApi.getValues()) as {{entityName}}CreateInput | {{entityName}}UpdateInput;
    await (isUpdate.value ? {{entityNameLower}}Update(data as {{entityName}}UpdateInput) : {{entityNameLower}}Add(data as {{entityName}}CreateInput));
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

---

## Schema 字段格式对照

| 属性 | 旧模板 | 新模板 | 说明 |
|------|--------|--------|------|
| 字段名 | `field` | `fieldName` | Vben5 新 API |
| 组件 | `component` | `component` | 相同 |
| 标签 | `label` | `label` | 相同 |
| 规则 | `rules` | `rules` | 相同 |
| Schema 类型 | `VbenFormSchema[]` | `FormSchemaGetter` | 函数返回 |
| 下拉选项 | `api: () => getDictOptions(...)` | `options: getDictOptions(..., true)` | 直接数组 |

---

## renderDict 使用注意

```typescript
// 枚举类型 - 直接传 number
renderDict(row.materialType, DictEnum.PRODUCT_MATERIAL_TYPE)

// boolean 类型 - 需转为 string
renderDict(String(row.state), DictEnum.SYS_NORMAL_DISABLE)
```

---

## DictEnum 常量命名规则

| 模块 + 枚举 | DictEnum 常量 |
|-------------|---------------|
| `product` + `MaterialTypeEnum` | `PRODUCT_MATERIAL_TYPE` |
| `product` + `ItemTypeEnum` | `PRODUCT_ITEM_TYPE` |

格式: `{MODULE}_{ENUM_NAME}` (枚举名去掉 Enum 后缀，转大写)