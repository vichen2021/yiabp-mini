<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { Table } from '#/api/code/table/model';

import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { getVxePopupContainer } from '@vben/utils';

import { Modal, Popconfirm, Space } from 'ant-design-vue';

import { useVbenVxeGrid, vxeCheckboxChecked } from '#/adapter/vxe-table';
import { tableList, tableRemove } from '#/api/code/table';
import { postCodeBuildWeb, postWebBuildCode } from '#/api/code/code-gen';

import { columns, querySchema } from './data';
import codeGenModal from './code-gen-modal.vue';
import tableDrawer from './table-drawer.vue';

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
        return await tableList({
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
  id: 'code-table-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const [TableDrawer, drawerApi] = useVbenDrawer({
  connectedComponent: tableDrawer,
});

const [CodeGenModal, codeGenModalApi] = useVbenModal({
  connectedComponent: codeGenModal,
});

function handleAdd() {
  drawerApi.setData({});
  drawerApi.open();
}

function handleEdit(row: Table) {
  drawerApi.setData({ id: row.id });
  drawerApi.open();
}

async function handleDelete(row: Table) {
  await tableRemove([row.id]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: Table) => row.id);
  Modal.confirm({
    title: '提示',
    okType: 'danger',
    content: `确认删除选中的${ids.length}条记录吗？`,
    onOk: async () => {
      await tableRemove(ids);
      await tableApi.query();
    },
  });
}

function handleGenerateCode() {
  const rows = tableApi.grid.getCheckboxRecords();
  if (rows.length === 0) {
    Modal.warning({
      title: '提示',
      content: '请至少选择一条记录',
    });
    return;
  }
  const ids = rows.map((row: Table) => row.id);
  codeGenModalApi.setData({
    tableIds: ids,
    tables: rows,
  });
  codeGenModalApi.open();
}

async function handleCodeToWeb() {
  Modal.confirm({
    title: '提示',
    content:
      '此操作将从现有代码实体类反向生成表结构，会覆盖当前数据库中的表结构数据，是否继续？',
    okType: 'danger',
    onOk: async () => {
      try {
        await postCodeBuildWeb();
        Modal.success({
          title: '成功',
          content: '从代码反向生成表结构成功！',
        });
        await tableApi.query();
      } catch (error) {
        console.error('反向生成失败:', error);
      }
    },
  });
}
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="数据表列表">
      <template #toolbar-tools>
        <Space>
          <a-button type="primary" @click="handleCodeToWeb">
            从代码生成表结构
          </a-button>
          <a-button
            :disabled="!vxeCheckboxChecked(tableApi)"
            type="primary"
            @click="handleGenerateCode"
          >
            生成代码
          </a-button>
          <a-button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['code:table:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </a-button>
          <a-button
            type="primary"
            v-access:code="['code:table:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </a-button>
        </Space>
      </template>
      <template #action="{ row }">
        <Space>
          <ghost-button
            v-access:code="['code:table:edit']"
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
              v-access:code="['code:table:remove']"
              @click.stop=""
            >
              {{ $t('pages.common.delete') }}
            </ghost-button>
          </Popconfirm>
        </Space>
      </template>
    </BasicTable>
    <TableDrawer @reload="tableApi.query()" />
    <CodeGenModal @reload="tableApi.query()" />
  </Page>
</template>
