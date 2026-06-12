<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { Tenant } from '#/api/system/tenant/model';

import { computed, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Fallback, Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';

import { Button, Form, FormItem, Input, Space } from 'antdv-next';

import {
  useVbenVxeGrid,
  VbenTableAction,
  vxeCheckboxChecked,
} from '#/adapter/vxe-table';
import {
  tenantExport,
  tenantInit,
  tenantList,
  tenantRemove,
  tenantUpdate,
} from '#/api/system/tenant';
import { TableSwitch } from '#/components/table';
import { useTenantStore } from '#/store/tenant';
import { commonDownloadExcel } from '#/utils/file/download';
import {
  confirmDangerAction,
  showErrorAlert,
} from '#/utils/modal';

import { columns, querySchema } from './data';
import tenantDrawer from './tenant-drawer.vue';

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
    checkMethod: ({ row }) => row?.id !== '00000000-0000-0000-0000-000000000001',
  },
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: {},
  proxyConfig: {
    ajax: {
      query: async ({ page }, formValues = {}) => {
        return await tenantList({
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
  id: 'system-tenant-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const [TenantDrawer, drawerApi] = useVbenDrawer({
  connectedComponent: tenantDrawer,
});

function handleAdd() {
  drawerApi.setData({});
  drawerApi.open();
}

async function handleEdit(record: Tenant) {
  drawerApi.setData({ id: record.id });
  drawerApi.open();
}

const tenantStore = useTenantStore();
async function handleDelete(row: Tenant) {
  await tenantRemove([row.id]);
  await tableApi.query();
  // 重新加载租户信息
  tenantStore.initTenant();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: Tenant) => row.id);
  confirmDangerAction({
    content: `确认删除选中的${ids.length}条记录吗？`,
    onConfirmed: async () => {
      await tenantRemove(ids);
      await tableApi.query();
      // 重新加载租户信息
      tenantStore.initTenant();
    },
  });
}

function handleDownloadExcel() {
  commonDownloadExcel(tenantExport, '租户数据', tableApi.formApi.form.values);
}

/**
 * 与后台逻辑相同
 * 只有超级管理员能访问租户相关
 */
const { hasAccessByCodes, hasAccessByRoles } = useAccess();

const isSuperAdmin = computed(() => {
  return hasAccessByRoles(['superadmin']);
});

const initTenantId = ref('');
const initUsername = ref('');
const initPassword = ref('');
const initializingTenantId = ref('');

const [InitTenantModal, initTenantModalApi] = useVbenModal({
  onConfirm: handleInitConfirm,
  title: '初始化租户',
});

function handleInit(row: Tenant) {
  initTenantId.value = row.id;
  initUsername.value = '';
  initPassword.value = '';
  initTenantModalApi.open();
}

async function handleInitConfirm() {
  if (!initUsername.value || !initPassword.value) {
    showErrorAlert('请输入管理员账号和密码', '错误');
    return;
  }

  initTenantModalApi.close();
  initializingTenantId.value = initTenantId.value;

  try {
    const result = await tenantInit(initTenantId.value, {
      isForce: false,
      username: initUsername.value,
      password: initPassword.value,
    });

    if (result?.needForce) {
      const confirmed = await confirmDangerAction({
        content: '数据库有数据，是否清除所有数据强制初始化？',
        onConfirmed: async () => {
          try {
            await tenantInit(initTenantId.value, {
              isForce: true,
              username: initUsername.value,
              password: initPassword.value,
            });
            await tableApi.query();
          } finally {
            initializingTenantId.value = '';
          }
        },
      });
      if (!confirmed) {
        initializingTenantId.value = '';
      }
    } else {
      await tableApi.query();
      initializingTenantId.value = '';
    }
  } catch (error) {
    initializingTenantId.value = '';
    console.error(error);
  }
}
</script>

<template>
  <Page v-if="isSuperAdmin" :auto-content-height="true">
    <BasicTable table-title="租户列表">
      <template #toolbar-tools>
        <Space>
          <Button
            v-access:code="['system:tenant:export']"
            @click="handleDownloadExcel"
          >
            {{ $t('pages.common.export') }}
          </Button>
          <Button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['system:tenant:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </Button>
          <Button
            type="primary"
            v-access:code="['system:tenant:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </Button>
        </Space>
      </template>
      <template #status="{ row }">
        <TableSwitch
          v-model:value="row.state"
          :api="() => tenantUpdate(row)"
          :disabled="row.id === '00000000-0000-0000-0000-000000000001' || !hasAccessByCodes(['system:tenant:edit'])"
          @reload="tableApi.query()"
        />
      </template>
      <template #action="{ row }">
        <VbenTableAction
          v-if="row.id !== '00000000-0000-0000-0000-000000000001'"
          :actions="[
            {
              auth: 'system:tenant:edit',
              disabled: initializingTenantId === row.id,
              onClick: () => handleEdit(row),
              text: $t('pages.common.edit'),
            },
            {
              auth: 'system:tenant:edit',
              class: 'text-green-600 hover:text-green-700',
              disabled: !!initializingTenantId && initializingTenantId !== row.id,
              loading: initializingTenantId === row.id,
              onClick: () => handleInit(row),
              text: initializingTenantId === row.id ? '初始化中' : '初始化',
            },
            {
              auth: 'system:tenant:remove',
              danger: true,
              disabled: initializingTenantId === row.id,
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
    <TenantDrawer @reload="tableApi.query()" />
    <InitTenantModal>
      <p style="margin-bottom: 16px; color: #666;">请输入租户管理员账号信息：</p>
      <Form layout="vertical">
        <FormItem label="管理员账号" required>
          <Input v-model:value="initUsername" placeholder="请输入管理员账号" />
        </FormItem>
        <FormItem label="管理员密码" required>
          <Input.Password v-model:value="initPassword" placeholder="请输入管理员密码" />
        </FormItem>
      </Form>
    </InitTenantModal>
  </Page>
  <Fallback v-else description="您没有租户的访问权限" status="403" />
</template>
