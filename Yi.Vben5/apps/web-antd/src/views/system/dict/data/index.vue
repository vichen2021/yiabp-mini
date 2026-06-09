<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { PageQuery } from '#/api/common';
import type { DictData } from '#/api/system/dict/dict-data-model';

import { ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';

import { Button, Space } from 'antdv-next';

import {
  useVbenVxeGrid,
  VbenTableAction,
  vxeCheckboxChecked,
} from '#/adapter/vxe-table';
import {
  dictDataExport,
  dictDataList,
  dictDataRemove,
} from '#/api/system/dict/dict-data';
import { commonDownloadExcel } from '#/utils/file/download';
import { confirmDangerAction } from '#/utils/modal';

import { emitter } from '../mitt';
import { columns, querySchema } from './data';
import dictDataDrawer from './dict-data-drawer.vue';

const dictType = ref('');

const formOptions: VbenFormProps = {
  commonConfig: {
    labelWidth: 80,
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
        const params: PageQuery = {
          SkipCount: page.currentPage,
          MaxResultCount: page.pageSize,
          ...formValues,
        };
        if (dictType.value) {
          params.dictType = dictType.value;
        }

        return await dictDataList(params);
      },
    },
  },
  rowConfig: {
    keyField: 'id',
  },
  id: 'system-dict-data-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const [DictDataDrawer, drawerApi] = useVbenDrawer({
  connectedComponent: dictDataDrawer,
});

function handleAdd() {
  drawerApi.setData({ dictType: dictType.value });
  drawerApi.open();
}

async function handleEdit(record: DictData) {
  drawerApi.setData({
    dictType: dictType.value,
    id: record.id,
  });
  drawerApi.open();
}

async function handleDelete(row: DictData) {
  await dictDataRemove([row.id]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: DictData) => row.id);
  confirmDangerAction({
    content: `确认删除选中的${ids.length}条记录吗？`,
    onConfirmed: async () => {
      await dictDataRemove(ids);
      await tableApi.query();
    },
  });
}

function handleDownloadExcel() {
  commonDownloadExcel(dictDataExport, '字典数据', tableApi.formApi.form.values);
}

emitter.on('rowClick', async (value) => {
  dictType.value = value;
  await tableApi.query();
});
</script>

<template>
  <div>
    <BasicTable id="dict-data" table-title="字典数据列表">
      <template #toolbar-tools>
        <Space>
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
            :disabled="dictType === ''"
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
    <DictDataDrawer @reload="tableApi.query()" />
  </div>
</template>
