<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';
import type { Recordable } from '@vben/types';

import type { VxeGridProps } from '#/adapter/vxe-table';

import { nextTick } from 'vue';

import { Page, useVbenModal } from '@vben/common-ui';
import { listToTree } from '@vben/utils';

import { Button, Space } from 'antdv-next';

import { useVbenVxeGrid, VbenTableAction } from '#/adapter/vxe-table';

import { treeList, treeRemove } from './api';
import { columns, querySchema } from './data';
import treeModal from './tree-modal.vue';

const formOptions: VbenFormProps = {
  commonConfig: {
    labelWidth: 80,
  },
  schema: querySchema(),
  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
};

const gridOptions: VxeGridProps = {
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: {
    enabled: false,
  },
  proxyConfig: {
    ajax: {
      query: async (_, formValues = {}) => {
        const resp = await treeList({
          ...formValues,
        });
        const treeData = listToTree(resp, {
          id: 'id',
          pid: 'parentId',
          children: 'children',
        });
        return { rows: treeData };
      },
      // 默认请求接口后展开全部 不需要可以删除这段
      querySuccess: () => {
        nextTick(() => {
          expandAll();
        });
      },
    },
  },
  rowConfig: {
    keyField: 'id',
  },

  treeConfig: {
    parentField: 'parentId',
    rowField: 'id',
    transform: false,
  },
};

const [BasicTable, tableApi] = useVbenVxeGrid({ formOptions, gridOptions });
const [TreeModal, modalApi] = useVbenModal({
  connectedComponent: treeModal,
});

function handleAdd() {
  modalApi.setData({ update: false });
  modalApi.open();
}

async function handleEdit(row: Recordable<any>) {
  modalApi.setData({ id: row.id, update: true });
  modalApi.open();
}

async function handleDelete(row: Recordable<any>) {
  await treeRemove(row.id);
  await tableApi.query();
}

function expandAll() {
  tableApi.grid?.setAllTreeExpand(true);
}

function collapseAll() {
  tableApi.grid?.setAllTreeExpand(false);
}
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable>
      <template #toolbar-actions>
        <span class="pl-[7px] text-[16px]">测试树列表</span>
      </template>
      <template #toolbar-tools>
        <Space>
          <Button @click="collapseAll">
            {{ $t('pages.common.collapse') }}
          </Button>
          <Button @click="expandAll">
            {{ $t('pages.common.expand') }}
          </Button>
          <Button
            type="primary"
            v-access:code="['system:tree:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </Button>
        </Space>
      </template>
      <template #action="{ row }">
        <VbenTableAction
          :actions="[
            {
              auth: 'system:tree:edit',
              onClick: () => handleEdit(row),
              text: $t('pages.common.edit'),
            },
            {
              auth: 'system:tree:remove',
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
    <TreeModal @reload="tableApi.query()" />
  </Page>
</template>
