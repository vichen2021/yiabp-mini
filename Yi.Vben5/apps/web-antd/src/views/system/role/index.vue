<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { Role } from '#/api/system/role/model';

import { computed } from 'vue';
import { useRouter } from 'vue-router';

import { useAccess } from '@vben/access';
import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';

import { Button, Space } from 'antdv-next';

import {
  useVbenVxeGrid,
  VbenTableAction,
  vxeCheckboxChecked,
} from '#/adapter/vxe-table';
import {
  roleExport,
  roleList,
  roleRemove,
  roleUpdate,
} from '#/api/system/role';
import { TableSwitch } from '#/components/table';
import { commonDownloadExcel } from '#/utils/file/download';
import { confirmDangerAction } from '#/utils/modal';

import { columns, querySchema } from './data';
import roleAuthModal from './role-datascope-drawer.vue';
import roleDrawer from './role-drawer.vue';

const formOptions: VbenFormProps = {
  commonConfig: {
    labelWidth: 80,
    componentProps: {
      allowClear: true,
    },
  },
  schema: querySchema(),
  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
  // 日期选择格式化
  fieldMappingTime: [
    [
      'creationTime',
      ['startTime', 'endTime'],
      ['YYYY-MM-DD 00:00:00', 'YYYY-MM-DD 23:59:59'],
    ],
  ],
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
        return await roleList({
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
  id: 'system-role-index',
};

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});
const [RoleDrawer, drawerApi] = useVbenDrawer({
  connectedComponent: roleDrawer,
});

function handleAdd() {
  drawerApi.setData({});
  drawerApi.open();
}

async function handleEdit(record: Role) {
  drawerApi.setData({ id: record.id });
  drawerApi.open();
}

async function handleDelete(row: Role) {
  await roleRemove([row.id]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: Role) => row.id);
  confirmDangerAction({
    content: `确认删除选中的${ids.length}条记录吗？`,
    onConfirmed: async () => {
      await roleRemove(ids);
      await tableApi.query();
    },
  });
}

function handleDownloadExcel() {
  commonDownloadExcel(roleExport, '角色数据', tableApi.formApi.form.values, {
    fieldMappingTime: formOptions.fieldMappingTime,
  });
}

const { hasAccessByCodes, hasAccessByRoles } = useAccess();

const SUPERADMIN_ROLE_CODE = 'superadmin';
const isSuperAdmin = computed(() => hasAccessByRoles(['superadmin']));

const [RoleAuthModal, authModalApi] = useVbenModal({
  connectedComponent: roleAuthModal,
});

function handleAuthEdit(record: Role) {
  authModalApi.setData({ id: record.id });
  authModalApi.open();
}

const router = useRouter();
function handleAssignRole(record: Role) {
  router.push(`/system/role-auth/user/${record.id}`);
}
</script>

<template>
  <Page :auto-content-height="true">
    <BasicTable table-title="角色列表">
      <template #toolbar-tools>
        <Space>
          <Button
            v-access:code="['system:role:export']"
            @click="handleDownloadExcel"
          >
            {{ $t('pages.common.export') }}
          </Button>
          <Button
            :disabled="!vxeCheckboxChecked(tableApi)"
            danger
            type="primary"
            v-access:code="['system:role:remove']"
            @click="handleMultiDelete"
          >
            {{ $t('pages.common.delete') }}
          </Button>
          <Button
            type="primary"
            v-access:code="['system:role:add']"
            @click="handleAdd"
          >
            {{ $t('pages.common.add') }}
          </Button>
        </Space>
      </template>      
      <template #status="{ row }">
        <TableSwitch
          v-model:value="row.state"
          :api="() => roleUpdate(row)"
          :disabled="
            row.id === '1' ||
            row.roleCode === SUPERADMIN_ROLE_CODE ||
            !hasAccessByCodes(['system:role:edit'])
          "
          @reload="tableApi.query()"
        />
      </template>
      <template #action="{ row }">
        <!-- 租户管理员不可修改超级管理员角色 防止误操作 -->
        <!-- 超级管理员可通过租户切换来操作租户管理员角色 -->
        <template
          v-if="row.roleCode !== SUPERADMIN_ROLE_CODE || isSuperAdmin"
        >
          <VbenTableAction
            :actions="[
              {
                auth: 'system:role:edit',
                onClick: () => handleEdit(row),
                text: $t('pages.common.edit'),
              },
              {
                auth: 'system:role:edit',
                onClick: () => handleAuthEdit(row),
                text: '权限',
              },
              {
                auth: 'system:role:edit',
                onClick: () => handleAssignRole(row),
                text: '分配',
              },
              {
                auth: 'system:role:remove',
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
      </template>
    </BasicTable>
    <RoleDrawer @reload="tableApi.query()" />
    <RoleAuthModal @reload="tableApi.query()" />
  </Page>
</template>
