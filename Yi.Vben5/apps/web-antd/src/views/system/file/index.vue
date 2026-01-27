<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { FileItem } from '#/api/system/file/model';
import type { PageQuery } from '#/api/common';

import { onMounted, ref } from 'vue';
import { useRouter } from 'vue-router';

import { Page, useVbenModal } from '@vben/common-ui';
import { getVxePopupContainer } from '@vben/utils';

import { message, Modal, Popconfirm, Space } from 'ant-design-vue';

import { addSortParams, useVbenVxeGrid, vxeCheckboxChecked } from '#/adapter/vxe-table';
import {
  fileDownload,
  fileList,
  fileRemove,
} from '#/api/system/file';
import { calculateFileSize } from '#/utils/file';
import { downloadByData } from '#/utils/file/download';

import { columns, querySchema } from './data';
import fileUploadModal from './file-upload-modal.vue';
import imageUploadModal from './image-upload-modal.vue';

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
      ['startCreationTime', 'endCreationTime'],
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
      query: async ({ page, sorts }, formValues = {}) => {
        const params: PageQuery = {
          SkipCount: page.currentPage,
          MaxResultCount: page.pageSize,
          ...formValues,
        };
        addSortParams(params, sorts);
        return await fileList(params);
      },
    },
  },
  rowConfig: {
    keyField: 'id',
  },
  sortConfig: { remote: true, multiple: false },
  id: 'system-file-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
  gridEvents: {
    sortChange: () => tableApi.query(),
  },
});

async function handleDownload(row: FileItem) {
  const downloadSize = ref('下载中...');
  const hideLoading = message.loading({
    content: () => downloadSize.value,
    duration: 0,
  });
  try {
    const data = await fileDownload(row.id, (e) => {
      const percent = Math.floor((e.loaded / e.total!) * 100);
      const current = calculateFileSize(e.loaded);
      const total = calculateFileSize(e.total!);
      downloadSize.value = `已下载: ${current}/${total} (${percent}%)`;
    });
    downloadByData(data, row.fileName);
    message.success('下载完成');
  } finally {
    hideLoading();
  }
}

async function handleDelete(row: FileItem) {
  await fileRemove([row.id]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: FileItem) => row.id);
  Modal.confirm({
    title: '提示',
    okType: 'danger',
    content: `确认删除选中的${ids.length}条记录吗？`,
    onOk: async () => {
      await fileRemove(ids);
      await tableApi.query();
    },
  });
}

const router = useRouter();
function handleToSettings() {
  router.push('/system/file-config/index');
}

const [ImageUploadModal, imageUploadApi] = useVbenModal({
  connectedComponent: imageUploadModal,
});

const [FileUploadModal, fileUploadApi] = useVbenModal({
  connectedComponent: fileUploadModal,
});
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="文件列表">
      <template #toolbar-tools>
        <Space>
          <a-button
            v-access:code="['system:fileConfig:list']"
            @click="handleToSettings"
          >
            配置管理
          </a-button>
          <a-button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['system:file:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </a-button>
          <a-button
            v-access:code="['system:file:upload']"
            @click="fileUploadApi.open"
          >
            文件上传
          </a-button>
          <a-button
            v-access:code="['system:file:upload']"
            @click="imageUploadApi.open"
          >
            图片上传
          </a-button>
        </Space>
      </template>
      <template #action="{ row }">
        <Space>
          <ghost-button
            v-access:code="['system:file:download']"
            @click="handleDownload(row)"
          >
            {{ $t('pages.common.download') }}
          </ghost-button>
          <Popconfirm
            :get-popup-container="getVxePopupContainer"
            placement="left"
            title="确认删除？"
            @confirm="handleDelete(row)"
          >
            <ghost-button
              danger
              v-access:code="['system:file:remove']"
              @click.stop=""
            >
              {{ $t('pages.common.delete') }}
            </ghost-button>
          </Popconfirm>
        </Space>
      </template>
    </BasicTable>
    <ImageUploadModal @reload="tableApi.query" />
    <FileUploadModal @reload="tableApi.query" />
  </Page>
</template>
