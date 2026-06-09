<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { FileConfig } from '#/api/system/file-config/model';

import { useAccess } from '@vben/access';
import { Page, useVbenDrawer } from '@vben/common-ui';

import { Button, Space } from 'antdv-next';

import {
  useVbenVxeGrid,
  VbenTableAction,
  vxeCheckboxChecked,
} from '#/adapter/vxe-table';
import {
  fileConfigChangeStatus,
  fileConfigList,
  fileConfigRemove,
} from '#/api/system/file-config';
import { TableSwitch } from '#/components/table';
import { confirmDangerAction } from '#/utils/modal';

import { columns, querySchema } from './data';
import fileConfigDrawer from './file-config-drawer.vue';

const formOptions: VbenFormProps = {
  schema: querySchema(),
  commonConfig: {
    labelWidth: 80,
    componentProps: {
      allowClear: true,
    },
  },
  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
};

const gridOptions: VxeGridProps = {
  checkboxConfig: {
    // 高亮
    highlight: true,
    // 翻页时保留选中状态
    reserve: true,
    // 点击行选中
    // trigger: 'row',
  },
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: {},
  proxyConfig: {
    ajax: {
      query: async ({ page }, formValues = {}) => {
        return await fileConfigList({
          SkipCount: page.currentPage,
          MaxResultCount: page.pageSize,
          ...formValues,
        });
      },
    },
  },
  rowConfig: {
    keyField: 'ossConfigId',
  },
  id: 'system-file-config-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const [FileConfigDrawer, drawerApi] = useVbenDrawer({
  connectedComponent: fileConfigDrawer,
});

function handleAdd() {
  drawerApi.setData({});
  drawerApi.open();
}

async function handleEdit(record: FileConfig) {
  drawerApi.setData({ id: record.ossConfigId });
  drawerApi.open();
}

async function handleDelete(row: FileConfig) {
  await fileConfigRemove([row.ossConfigId]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: FileConfig) => row.ossConfigId);
  confirmDangerAction({
    content: `确认删除选中的${ids.length}条记录吗？`,
    onConfirmed: async () => {
      await fileConfigRemove(ids);
      await tableApi.query();
    },
  });
}

const { hasAccessByCodes } = useAccess();
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="文件存储配置列表">
      <template #toolbar-tools>
        <Space>
          <Button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['system:fileConfig:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </Button>
          <Button
            type="primary"
            v-access:code="['system:fileConfig:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </Button>
        </Space>
      </template>
      <template #status="{ row }">
        <TableSwitch
          v-model:value="row.status"
          :api="() => fileConfigChangeStatus(row)"
          :disabled="!hasAccessByCodes(['system:fileConfig:edit'])"
          checked-text="是"
          un-checked-text="否"
          @reload="tableApi.query()"
        />
      </template>
      <template #action="{ row }">
        <VbenTableAction
          :actions="[
            {
              auth: 'system:fileConfig:edit',
              onClick: () => handleEdit(row),
              text: $t('pages.common.edit'),
            },
            {
              auth: 'system:fileConfig:remove',
              danger: true,
              popConfirm: {
                title: '确认删除？',
                confirm: () => handleDelete(row),
              },
              text: $t('pages.common.delete'),
            },
          ]"
          align="center"
        />
      </template>
    </BasicTable>
    <FileConfigDrawer @reload="tableApi.query()" />
  </Page>
</template>
