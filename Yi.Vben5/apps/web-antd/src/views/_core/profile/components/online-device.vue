<script setup lang="ts">
import type { Recordable } from '@vben/types';

import type { VxeGridProps } from '#/adapter/vxe-table';

import { useVbenVxeGrid, VbenTableAction } from '#/adapter/vxe-table';
import { forceLogout2, onlineDeviceList } from '#/api/monitor/online';
import { columns } from '#/views/monitor/online/data';

const gridOptions: VxeGridProps = {
  columns,
  keepSource: true,
  pagerConfig: {
    enabled: false,
  },
  proxyConfig: {
    ajax: {
      query: async () => {
        return await onlineDeviceList();
      },
    },
  },
  rowConfig: {
    keyField: 'tokenId',
  },
};

const [BasicTable, tableApi] = useVbenVxeGrid({ gridOptions });

async function handleForceOffline(row: Recordable<any>) {
  await forceLogout2(row.tokenId);
  await tableApi.query();
}
</script>

<template>
  <div>
    <BasicTable table-title="我的在线设备">
      <template #action="{ row }">
        <VbenTableAction
          :actions="[
            {
              danger: true,
              popConfirm: {
                title: `确认强制下线[${row.userName}]?`,
                confirm: () => handleForceOffline(row),
              },
              text: '强制下线',
            },
          ]"
        />
      </template>
    </BasicTable>
  </div>
</template>
