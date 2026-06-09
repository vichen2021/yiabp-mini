<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { LeaveForm } from './api/model';

import type { VxeGridProps } from '#/adapter/vxe-table';

import { useRouter } from 'vue-router';

import { Page, useVbenModal } from '@vben/common-ui';

import { Button, Space } from 'antdv-next';

import {
  useVbenVxeGrid,
  VbenTableAction,
  vxeCheckboxChecked,
} from '#/adapter/vxe-table';
import { cancelProcessApply } from '#/api/workflow/instance';
import { commonDownloadExcel } from '#/utils/file/download';
import { confirmDangerAction } from '#/utils/modal';

import { flowInfoModal } from '../components';
import { leaveExport, leaveList, leaveRemove } from './api';
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
  checkboxConfig: {
    // 高亮
    highlight: true,
    // 翻页时保留选中状态
    reserve: true,
    // 选中 需要根据状态判断
    checkMethod: ({ row }) => ['back', 'cancel', 'draft'].includes(row.status),
  },
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: {},
  proxyConfig: {
    ajax: {
      query: async ({ page }, formValues = {}) => {
        return await leaveList({
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
  // 表格全局唯一表示 保存列配置需要用到
  id: 'workflow-leave-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const router = useRouter();
function handleAdd() {
  router.push('/workflow/leaveEdit/index');
}

async function handleEdit(row: Required<LeaveForm>) {
  router.push({ path: '/workflow/leaveEdit/index', query: { id: row.id } });
}

async function handleDelete(row: Required<LeaveForm>) {
  await leaveRemove(row.id);
  await tableApi.query();
}

async function handleRevoke(row: Required<LeaveForm>) {
  await cancelProcessApply({
    businessId: row.id,
    message: '申请人撤销流程！',
  });
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: Required<LeaveForm>) => row.id);
  confirmDangerAction({
    content: `确认删除选中的${ids.length}条记录吗？`,
    onConfirmed: async () => {
      await leaveRemove(ids);
      await tableApi.query();
    },
  });
}

function handleDownloadExcel() {
  commonDownloadExcel(
    leaveExport,
    '请假申请数据',
    tableApi.formApi.form.values,
    {
      fieldMappingTime: formOptions.fieldMappingTime,
    },
  );
}
const [FlowInfoModal, flowInfoModalApi] = useVbenModal({
  connectedComponent: flowInfoModal,
});
function handleInfo(row: Required<LeaveForm>) {
  flowInfoModalApi.setData({ businessId: row.id });
  flowInfoModalApi.open();
}
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="请假申请列表">
      <template #toolbar-tools>
        <Space>
          <Button
            v-access:code="['workflow:leave:export']"
            @click="handleDownloadExcel"
          >
            {{ $t('pages.common.export') }}
          </Button>
          <Button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['workflow:leave:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </Button>
          <Button
            type="primary"
            v-access:code="['workflow:leave:add']"
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
              auth: 'workflow:leave:edit',
              ifShow: ['draft', 'cancel', 'back'].includes(row.status),
              onClick: () => handleEdit(row),
              text: $t('pages.common.edit'),
            },
            {
              auth: 'workflow:leave:edit',
              ifShow: ['waiting'].includes(row.status),
              popConfirm: {
                title: '确认撤销？',
                confirm: () => handleRevoke(row),
              },
              text: '撤销',
            },
            {
              ifShow: row.status !== 'draft',
              onClick: () => handleInfo(row),
              text: '详情',
            },
            {
              auth: 'workflow:leave:remove',
              danger: true,
              ifShow: ['draft', 'cancel', 'back'].includes(row.status),
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
    <FlowInfoModal />
  </Page>
</template>
