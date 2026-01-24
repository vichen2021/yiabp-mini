<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { Field } from '#/api/code/field/model';

import { onMounted, ref } from 'vue';

import { Page, useVbenDrawer } from '@vben/common-ui';
import { getVxePopupContainer } from '@vben/utils';

import { Modal, Popconfirm, Space } from 'ant-design-vue';

import { useVbenVxeGrid, vxeCheckboxChecked } from '#/adapter/vxe-table';
import { fieldList, fieldRemove } from '#/api/code/field';
import { tableSelectList } from '#/api/code/table';

import { columns, querySchema } from './data';
import fieldDrawer from './field-drawer.vue';

// 表名映射
const tableMap = ref<Record<string, string>>({});
// 表选项列表
const tableOptions = ref<Array<{ label: string; value: string }>>([]);

// 加载表信息和选项
async function loadTableData() {
  try {
    const tables = await tableSelectList();
    const map: Record<string, string> = {};
    const options: Array<{ label: string; value: string }> = [];
    tables.forEach((table) => {
      map[table.id] = table.name;
      options.push({
        label: `${table.name}${table.description ? ` (${table.description})` : ''}`,
        value: table.id,
      });
    });
    tableMap.value = map;
    tableOptions.value = options;
    
    // 更新搜索表单的表选项
    tableApi.formApi.updateSchema([
      {
        componentProps: {
          options: tableOptions.value,
        },
        fieldName: 'tableId',
      },
    ]);
  } catch (error) {
    console.error('加载表信息失败:', error);
  }
}

onMounted(() => {
  loadTableData();
});

const formOptions: VbenFormProps = {
  commonConfig: {
    labelWidth: 80,
    componentProps: {
      allowClear: true,
    },
  },
  schema: querySchema(),
  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
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
        const result = await fieldList({
          SkipCount: page.currentPage,
          MaxResultCount: page.pageSize,
          ...formValues,
        });
        // 为每个字段添加表名
        result.items = result.items.map((field) => ({
          ...field,
          tableName: tableMap.value[field.tableId] || field.tableId,
        }));
        return result;
      },
    },
  },
  rowConfig: {
    keyField: 'id',
  },
  id: 'code-field-index',
};

// eslint-disable-next-line @typescript-eslint/ban-ts-comment
// @ts-ignore 类型实例化过深
const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const [FieldDrawer, drawerApi] = useVbenDrawer({
  connectedComponent: fieldDrawer,
});

function handleAdd() {
  drawerApi.setData({});
  drawerApi.open();
}

function handleEdit(row: Field) {
  drawerApi.setData({ id: row.id });
  drawerApi.open();
}

async function handleDelete(row: Field) {
  await fieldRemove([row.id]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: Field) => row.id);
  Modal.confirm({
    title: '提示',
    okType: 'danger',
    content: `确认删除选中的${ids.length}条记录吗？`,
    onOk: async () => {
      await fieldRemove(ids);
      await tableApi.query();
    },
  });
}
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="字段列表">
      <template #toolbar-tools>
        <Space>
          <a-button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['code:field:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </a-button>
          <a-button
            type="primary"
            v-access:code="['code:field:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </a-button>
        </Space>
      </template>
      <template #action="{ row }">
        <Space>
          <ghost-button
            v-access:code="['code:field:edit']"
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
              v-access:code="['code:field:remove']"
              @click.stop=""
            >
              {{ $t('pages.common.delete') }}
            </ghost-button>
          </Popconfirm>
        </Space>
      </template>
    </BasicTable>
    <FieldDrawer @reload="tableApi.query()" />
  </Page>
</template>
