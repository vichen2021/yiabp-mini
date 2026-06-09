<script setup lang="ts">
import type { RedisInfo } from '#/api/monitor/cache';

import { VbenDescriptions, VbenDescriptionsItem } from '@vben/common-ui';

interface IRedisInfo extends RedisInfo {
  dbSize: string;
}

defineProps<{ data: IRedisInfo }>();
</script>

<template>
  <VbenDescriptions
    bordered
    :column="{ lg: 4, md: 3, sm: 1, xl: 4, xs: 1 }"
    size="small"
  >
    <VbenDescriptionsItem label="redis版本">
      {{ data.redis_version }}
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="redis模式">
      {{ data.redis_mode === 'standalone' ? '单机模式' : '集群模式' }}
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="tcp端口">
      {{ data.tcp_port }}
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="客户端数">
      {{ data.connected_clients }}
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="运行时间">
      {{ data.uptime_in_days }} 天
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="使用内存">
      {{ data.used_memory_human }}
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="使用CPU">
      {{ Number.parseFloat(data?.used_cpu_user_children ?? '0').toFixed(2) }}
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="内存配置">
      {{ data.maxmemory_human }}
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="AOF是否开启">
      {{ data.aof_enabled === '0' ? '否' : '是' }}
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="RDB是否成功">
      {{ data.rdb_last_bgsave_status }}
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="key数量">
      {{ data.dbSize }}
    </VbenDescriptionsItem>
    <VbenDescriptionsItem label="网络入口/出口">
      {{
        `${data.instantaneous_input_kbps}kps/${data.instantaneous_output_kbps}kps`
      }}
    </VbenDescriptionsItem>
  </VbenDescriptions>
</template>
