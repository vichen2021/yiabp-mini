<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { Notice } from '#/api/system/notice/model';

import { Page, useVbenModal } from '@vben/common-ui';

import { Button, Space } from 'antdv-next';

import {
  useVbenVxeGrid,
  VbenTableAction,
  vxeCheckboxChecked,
} from '#/adapter/vxe-table';
import { noticeList, noticeRemove, noticeSendOnline } from '#/api/system/notice';
import { confirmDangerAction, showSuccessAlert } from '#/utils/modal';

import { columns, querySchema } from './data';
import noticeModal from './notice-modal.vue';

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
        return await noticeList({
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
  id: 'system-notice-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const [NoticeModal, modalApi] = useVbenModal({
  connectedComponent: noticeModal,
});

function handleAdd() {
  modalApi.setData({});
  modalApi.open();
}

async function handleEdit(record: Notice) {
  modalApi.setData({ id: record.id });
  modalApi.open();
}

async function handleDelete(row: Notice) {
  await noticeRemove([row.id]);
  await tableApi.query();
}

async function handleSendNotice(row: Notice) {
  await noticeSendOnline(row.id);
  showSuccessAlert('通知已推送至所有在线用户');
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: Notice) => row.id);
  confirmDangerAction({
    content: `确认删除选中的${ids.length}条记录吗？`,
    onConfirmed: async () => {
      await noticeRemove(ids);
      await tableApi.query();
    },
  });
}
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="通知公告列表">
      <template #toolbar-tools>
        <Space>
          <Button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['system:notice:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </Button>
          <Button
            type="primary"
            v-access:code="['system:notice:add']"
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
              auth: 'system:notice:edit',
              onClick: () => handleSendNotice(row),
              text: '推送',
            },
            {
              auth: 'system:notice:edit',
              onClick: () => handleEdit(row),
              text: $t('pages.common.edit'),
            },
            {
              auth: 'system:notice:remove',
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
    <NoticeModal @reload="tableApi.query()" />
  </Page>
</template>
