<script setup lang="ts">
import type { Recordable } from '@vben/types';

import { nextTick } from 'vue';

import { Page, useVbenModal, type VbenFormProps } from '@vben/common-ui';

import { Button, Space } from 'antdv-next';

import {
  useVbenVxeGrid,
  VbenTableAction,
  type VxeGridProps,
} from '#/adapter/vxe-table';
import { categoryList, categoryRemove } from '#/api/workflow/category';

import categoryModal from './category-modal.vue';
import { columns, querySchema } from './data';

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
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: {
    enabled: false,
  },
  proxyConfig: {
    ajax: {
      query: async (_, formValues = {}) => {
        const resp = await categoryList({
          ...formValues,
        });
        return { rows: resp };
      },
      // 默认请求接口后展开全部 不需要可以删除这段
      querySuccess: () => {
        nextTick(() => {
          expandAll();
        });
      },
    },
  },
  /**
   * 虚拟滚动  默认关闭
   */
  scrollY: {
    enabled: false,
    gt: 0,
  },
  rowConfig: {
    keyField: 'categoryId',
  },
  treeConfig: {
    parentField: 'parentId',
    rowField: 'categoryId',
    transform: true,
  },
  // 表格全局唯一表示 保存列配置需要用到
  id: 'workflow-category-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({ formOptions, gridOptions });
const [CategoryModal, modalApi] = useVbenModal({
  connectedComponent: categoryModal,
});

function handleAdd(row?: Recordable<any>) {
  modalApi.setData({ parentId: row?.categoryId });
  modalApi.open();
}

async function handleEdit(row: Recordable<any>) {
  modalApi.setData({ id: row.categoryId });
  modalApi.open();
}

async function handleDelete(row: Recordable<any>) {
  await categoryRemove(row.categoryId);
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
    <BasicTable table-title="流程分类列表">
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
            v-access:code="['workflow:category:add']"
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
              auth: 'workflow:category:edit',
              onClick: () => handleEdit(row),
              text: $t('pages.common.edit'),
            },
            {
              auth: 'workflow:category:edit',
              class: 'text-green-600 hover:text-green-700',
              onClick: () => handleAdd(row),
              text: $t('pages.common.add'),
            },
            {
              auth: 'workflow:category:remove',
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
    <CategoryModal @reload="tableApi.query()" />
  </Page>
</template>
