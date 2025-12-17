<script setup lang="ts">
import type { EChartsOption } from 'echarts';

// import * as echarts from 'echarts';
import { onMounted, ref } from 'vue';

import {
  EchartsUI,
  type EchartsUIType,
  useEcharts,
} from '@vben/plugins/echarts';

import { deviceInfoList } from '../api';

defineOptions({ name: 'Device' });

const deviceRef = ref<EchartsUIType>();
const { renderEcharts } = useEcharts(deviceRef);

onMounted(async () => {
  const data = await deviceInfoList();
  const options: EChartsOption = {
    legend: {
      left: 'left',
      orient: 'vertical',
    },
    series: [
      {
        data,
        emphasis: {
          itemStyle: {
            shadowBlur: 10,
            shadowColor: 'rgba(0, 0, 0, 0.5)',
            shadowOffsetX: 0,
          },
        },
        // 百分比
        label: {
          formatter: '{b}: {c} - ({d}%)', // 自定义显示格式(b:name, c:value, d:百分比)
          show: true,
        },
        radius: '50%',
        type: 'pie',
      },
    ],
    title: {
      left: 'center',
      text: '使用设备占比',
    },
    tooltip: {
      trigger: 'item',
    },
  };
  renderEcharts(options);
});
</script>

<template>
  <EchartsUI ref="deviceRef" height="720px" width="100%" />
</template>

<style scoped></style>
