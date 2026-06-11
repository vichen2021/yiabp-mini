<script setup lang="ts">
import type { VbenFormProps } from '@vben/common-ui';

import type { VxeGridProps } from '#/adapter/vxe-table';
import type { User } from '#/api/system/user/model';
import type { MenuProps } from 'antdv-next';

import { computed, ref } from 'vue';

import { useAccess } from '@vben/access';
import { Page, useVbenDrawer, useVbenModal } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { preferences } from '@vben/preferences';

import { Avatar, Button, Dropdown, Popconfirm, Space } from 'antdv-next';

import {
  useVbenVxeGrid,
  vxeCheckboxChecked,
} from '#/adapter/vxe-table';
import {
  userExport,
  userList,
  userRemove,
  userUpdate,
} from '#/api/system/user';
import { TableSwitch } from '#/components/table';
import { commonDownloadExcel } from '#/utils/file/download';
import { confirmDangerAction } from '#/utils/modal';

import { columns, querySchema } from './data';
import DeptTree from './dept-tree.vue';
import userDrawer from './user-drawer.vue';
import userImportModal from './user-import-modal.vue';
import userInfoModal from './user-info-modal.vue';
import userResetPwdModal from './user-reset-pwd-modal.vue';

/**
 * 导入
 */
const [UserImpotModal, userImportModalApi] = useVbenModal({
  connectedComponent: userImportModal,
});

function handleImport() {
  userImportModalApi.open();
}

// 左边部门用
const selectDeptId = ref<string[]>([]);

const formOptions: VbenFormProps = {
  schema: querySchema(),
  commonConfig: {
    labelWidth: 80,
    componentProps: {
      allowClear: true,
    },
  },
  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',
  handleReset: async () => {
    selectDeptId.value = [];

    const { formApi, reload } = tableApi;
    await formApi.resetForm();
    const formValues = formApi.form.values;
    formApi.setLatestSubmissionValues(formValues);
    await reload(formValues);
  },
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
    trigger: 'default',
    checkMethod: ({ row }) => row?.id !== 1,
  },
  columns,
  height: 'auto',
  keepSource: true,
  pagerConfig: {},
  proxyConfig: {
    ajax: {
      query: async ({ page }, formValues = {}) => {
        // 部门树选择处理
        if (selectDeptId.value.length === 1) {
          formValues.deptId = selectDeptId.value[0];
        } else {
          Reflect.deleteProperty(formValues, 'deptId');
        }

        return await userList({
          SkipCount: page.currentPage,
          MaxResultCount: page.pageSize,
          ...formValues,
        });
      },
    },
  },
  headerCellConfig: {
    height: 44,
  },
  cellConfig: {
    height: 48,
  },
  rowConfig: {
    keyField: 'id',
  },
  id: 'system-user-index',
};
const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,
  gridOptions,
});

const [UserDrawer, userDrawerApi] = useVbenDrawer({
  connectedComponent: userDrawer,
});

function handleAdd() {
  userDrawerApi.setData({});
  userDrawerApi.open();
}

function handleEdit(row: User) {
  userDrawerApi.setData({ id: row.id });
  userDrawerApi.open();
}

async function handleDelete(row: User) {
  await userRemove([row.id]);
  await tableApi.query();
}

function handleMultiDelete() {
  const rows = tableApi.grid.getCheckboxRecords();
  const ids = rows.map((row: User) => row.id);
  confirmDangerAction({
    content: `确认删除选中的${ids.length}条记录吗？`,
    onConfirmed: async () => {
      await userRemove(ids);
      await tableApi.query();
    },
  });
}

function handleDownloadExcel() {
  commonDownloadExcel(userExport, '用户管理', tableApi.formApi.form.values, {
    fieldMappingTime: formOptions.fieldMappingTime,
  });
}

const [UserInfoModal, userInfoModalApi] = useVbenModal({
  connectedComponent: userInfoModal,
});
function handleUserInfo(row: User) {
  userInfoModalApi.setData({ userId: row.id });
  userInfoModalApi.open();
}

const [UserResetPwdModal, userResetPwdModalApi] = useVbenModal({
  connectedComponent: userResetPwdModal,
});

function handleResetPwd(record: User) {
  userResetPwdModalApi.setData({ record });
  userResetPwdModalApi.open();
}

const { hasAccessByCodes } = useAccess();
const menuItems = computed(() => {
  const items: MenuProps['items'] = [{ key: 'info', label: '用户信息' }];
  if (hasAccessByCodes(['system:user:resetPwd'])) {
    items.push({ key: 'resetPwd', label: '重置密码' });
  }
  return items;
});

function handleMenuClick(key: string, row: User) {
  switch (key) {
    case 'info': {
      handleUserInfo(row);
      break;
    }
    case 'resetPwd': {
      handleResetPwd(row);
      break;
    }
  }
}
</script>

<template>
  <Page :auto-content-height="true">
    <div class="flex h-full gap-[8px]">
      <DeptTree
        v-model:select-dept-id="selectDeptId"
        class="w-[260px]"
        @reload="() => tableApi.reload()"
        @select="() => tableApi.reload()"
      />
      <BasicTable class="flex-1 overflow-hidden" table-title="用户列表">
        <template #toolbar-tools>
          <Space>
            <Button
              v-access:code="['system:user:export']"
              @click="handleDownloadExcel"
            >
              {{ $t('pages.common.export') }}
            </Button>
            <Button
              v-access:code="['system:user:import']"
              @click="handleImport"
            >
              {{ $t('pages.common.import') }}
            </Button>
            <Button
              :disabled="!vxeCheckboxChecked(tableApi)"
              danger
              type="primary"
              v-access:code="['system:user:remove']"
              @click="handleMultiDelete"
            >
              {{ $t('pages.common.delete') }}
            </Button>
            <Button
              type="primary"
              v-access:code="['system:user:add']"
              @click="handleAdd"
            >
              {{ $t('pages.common.add') }}
            </Button>
          </Space>
        </template>
        <template #avatar="{ row }">
          <!-- 可能要判断空字符串情况 所以没有使用?? -->
          <Avatar :src="row.icon || preferences.app.defaultAvatar" />
        </template>
        <template #status="{ row }">
          <TableSwitch
            v-model:value="row.state"
            :api="() => userUpdate(row)"
            :disabled="
              row.id === '1' || !hasAccessByCodes(['system:user:edit'])
            "
            @reload="() => tableApi.query()"
          />
        </template>
        <template #action="{ row }">
          <template v-if="row.id !== '1'">
            <Space>
              <Button
                type="link"
                v-access:code="['system:user:edit']"
                @click.stop="handleEdit(row)"
              >
                {{ $t('pages.common.edit') }}
              </Button>
              <Popconfirm
                placement="left"
                title="确认删除？"
                @confirm="handleDelete(row)"
              >
                <Button
                  danger
                  type="link"
                  v-access:code="['system:user:remove']"
                  @click.stop=""
                >
                  {{ $t('pages.common.delete') }}
                </Button>
              </Popconfirm>
            </Space>
            <Dropdown
              placement="bottomRight"
              :menu="{
                items: menuItems,
                onClick: (info) => handleMenuClick(String(info.key), row),
              }"
            >
              <Button size="small" type="link">
                {{ $t('pages.common.more') }}
              </Button>
            </Dropdown>
          </template>
        </template>
      </BasicTable>
    </div>
    <UserImpotModal @reload="tableApi.query()" />
    <UserDrawer @reload="tableApi.query()" />
    <UserInfoModal />
    <UserResetPwdModal />
  </Page>
</template>
