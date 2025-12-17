<script setup lang="ts">
import type { EChartsOption } from 'echarts';

// import * as echarts from 'echarts';
import { onMounted, ref } from 'vue';

import {
  EchartsUI,
  type EchartsUIType,
  useEcharts,
} from '@vben/plugins/echarts';

import { browserInfoList } from '../api';

defineOptions({ name: 'Browser' });

const browserRef = ref<EchartsUIType>();
const { renderEcharts } = useEcharts(browserRef);

onMounted(async () => {
  const data = await browserInfoList();
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
      text: '使用浏览器占比',
    },
    tooltip: {
      trigger: 'item',
    },
  };
  renderEcharts(options);
});
</script>

<template>
  <EchartsUI ref="browserRef" height="720px" width="100%" />
</template>

<style scoped></style>
