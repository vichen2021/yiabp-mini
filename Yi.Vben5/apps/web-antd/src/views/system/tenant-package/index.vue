<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { TenantPackage } from '#/api/system/tenant-package/model';

import { computed } from 'vue';

import { useAccess } from '@vben/access';
import { Fallback, Page, useVbenDrawer } from '@vben/common-ui';

import { Button, Space } from 'antdv-next';

import {
  useVbenVxeGrid,
  VbenTableAction,
  vxeCheckboxChecked,
} from '#/adapter/vxe-table';
import {
  tenantPackageExport,
  tenantPackageList,
  tenantPackageRemove,
  tenantPackageUpdate,
} from '#/api/system/tenant-package';
import { TableSwitch } from '#/components/table';
import { commonDownloadExcel } from '#/utils/file/download';
import { confirmDangerAction } from '#/utils/modal';

import { columns, querySchema } from './data';
import tenantPackageDrawer from './tenant-package-drawer.vue';

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
        return await tenantPackageList({
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
  id: 'system-tenant-package-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const [TenantPackageDrawer, drawerApi] = useVbenDrawer({
  connectedComponent: tenantPackageDrawer,
});

function handleAdd() {
  drawerApi.setData({});
  drawerApi.open();
}

async function handleEdit(record: TenantPackage) {
  drawerApi.setData({ id: record.id });
  drawerApi.open();
}

async function handleDelete(row: TenantPackage) {
  await tenantPackageRemove([row.id]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: TenantPackage) => row.id);
  confirmDangerAction({
    content: `确认删除选中的${ids.length}条记录吗？`,
    onConfirmed: async () => {
      await tenantPackageRemove(ids);
      await tableApi.query();
    },
  });
}

function handleDownloadExcel() {
  commonDownloadExcel(
    tenantPackageExport,
    '租户套餐数据',
    tableApi.formApi.form.values,
  );
}

/**
 * 与后台逻辑相同
 * 只有超级管理员能访问租户相关
 */
const { hasAccessByCodes, hasAccessByRoles } = useAccess();

const isSuperAdmin = computed(() => {
  return hasAccessByRoles(['superadmin']);
});
</script>

<template>
  <Page v-if="isSuperAdmin" :auto-content-height="true">
    <BasicTable table-title="租户套餐列表">
      <template #toolbar-tools>
        <Space>
          <Button
            v-access:code="['system:tenant-package:export']"
            @click="handleDownloadExcel"
          >
            {{ $t('pages.common.export') }}
          </Button>
          <Button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['system:tenant-package:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </Button>
          <Button
            type="primary"
            v-access:code="['system:tenant-package:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </Button>
        </Space>
      </template>
      <template #status="{ row }">
        <TableSwitch
          v-model:value="row.state"
          :api="() => tenantPackageUpdate(row)"
          :disabled="!hasAccessByCodes(['system:tenant-package:edit'])"
          @reload="tableApi.query()"
        />
      </template>
      <template #action="{ row }">
        <VbenTableAction
          :actions="[
            {
              auth: 'system:tenant-package:edit',
              onClick: () => handleEdit(row),
              text: $t('pages.common.edit'),
            },
            {
              auth: 'system:tenant-package:remove',
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
    <TenantPackageDrawer @reload="tableApi.query()" />
  </Page>
  <Fallback v-else description="您没有租户的访问权限" status="403" />
</template>
