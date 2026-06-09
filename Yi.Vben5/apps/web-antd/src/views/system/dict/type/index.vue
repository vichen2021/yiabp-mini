<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { DictType } from '#/api/system/dict/dict-type-model';

import { ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';

import { Button, Space } from 'antdv-next';

import {
  useVbenVxeGrid,
  VbenTableAction,
  vxeCheckboxChecked,
} from '#/adapter/vxe-table';
import {
  dictTypeExport,
  dictTypeList,
  dictTypeRemove,
  refreshDictTypeCache,
} from '#/api/system/dict/dict-type';
import { commonDownloadExcel } from '#/utils/file/download';
import { confirmDangerAction } from '#/utils/modal';

import { emitter } from '../mitt';
import { columns, querySchema } from './data';
import dictTypeModal from './dict-type-modal.vue';

const formOptions: VbenFormProps = {
  commonConfig: {
    labelWidth: 70,
    componentProps: {
      allowClear: true,
    },
  },
  schema: querySchema(),
  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3',
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
        return await dictTypeList({
          SkipCount: page.currentPage,
          MaxResultCount: page.pageSize,
          ...formValues,
        });
      },
    },
  },
  rowConfig: {
    keyField: 'id',
    // 高亮当前行
    isCurrent: true,
  },
  id: 'system-dict-type-index',
  rowClassName: 'hover:cursor-pointer',
};

const lastDictType = ref('');

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
  gridEvents: {
    cellClick: (e: any) => {
      const { row } = e;
      if (lastDictType.value === row.dictType) {
        return;
      }
      emitter.emit('rowClick', row.dictType);
      lastDictType.value = row.dictType;
    },
  },
});
const [DictTypeModal, modalApi] = useVbenModal({
  connectedComponent: dictTypeModal,
});

function handleAdd() {
  modalApi.setData({});
  modalApi.open();
}

async function handleEdit(record: DictType) {
  modalApi.setData({ id: record.id });
  modalApi.open();
}

async function handleDelete(row: DictType) {
  await dictTypeRemove([row.id]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: DictType) => row.id);
  confirmDangerAction({
    content: `确认删除选中的${ids.length}条记录吗？`,
    onConfirmed: async () => {
      await dictTypeRemove(ids);
      await tableApi.query();
    },
  });
}

async function handleRefreshCache() {
  await refreshDictTypeCache();
  await tableApi.query();
}

function handleDownloadExcel() {
  commonDownloadExcel(
    dictTypeExport,
    '字典类型数据',
    tableApi.formApi.form.values,
  );
}
</script>

<template>
  <div>
    <BasicTable id="dict-type" table-title="字典类型列表">
      <template #toolbar-tools>
        <Space>
          <Button
            v-access:code="['system:dict:edit']"
            @click="handleRefreshCache"
          >
            刷新缓存
          </Button>
          <Button
            v-access:code="['system:dict:export']"
            @click="handleDownloadExcel"
          >
            {{ $t('pages.common.export') }}
          </Button>
          <Button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['system:dict:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </Button>
          <Button
            type="primary"
            v-access:code="['system:dict:add']"
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
              auth: 'system:dict:edit',
              onClick: () => handleEdit(row),
              text: $t('pages.common.edit'),
            },
            {
              auth: 'system:dict:remove',
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
    <DictTypeModal @reload="tableApi.query()" />
  </div>
</template>

<style lang="scss">
div#dict-type {
  .vxe-body--row {
    &.row--current {
      // 选中行bold
      font-weight: 600;
    }
  }
}
</style>
