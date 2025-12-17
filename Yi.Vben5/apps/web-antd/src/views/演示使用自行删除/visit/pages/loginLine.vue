<script setup lang="ts">
import type { EChartsOption } from 'echarts';

// import * as echarts from 'echarts';
import { onMounted, ref } from 'vue';

import {
  EchartsUI,
  type EchartsUIType,
  useEcharts,
} from '@vben/plugins/echarts';

import { loginLine } from '../api';

defineOptions({ name: 'LoginLine' });

const loginLineRef = ref<EchartsUIType>();
const { renderEcharts } = useEcharts(loginLineRef);

onMounted(async () => {
  const data = await loginLine();
  console.log(data);
  const options: EChartsOption = {
    legend: {},
    series: [
      {
        data: data.success,
        itemStyle: {
          color: '#3399CC',
        },
        lineStyle: {
          color: '#3399CC',
        },
        name: '登录成功',
        type: 'line',
      },
      {
        data: data.fail,
        itemStyle: {
          color: '#CC6633',
        },
        lineStyle: {
          color: '#CC6633',
        },
        name: '登录失败',
        type: 'line',
      },
    ],
    title: {
      text: '近一月登录量统计',
    },
    toolbox: {
      feature: {
        dataView: { readOnly: true },
        dataZoom: {
          yAxisIndex: 'none',
        },
        magicType: { type: ['line', 'bar'] },
        saveAsImage: {},
      },
      show: true,
    },
    tooltip: {
      trigger: 'axis',
    },
    xAxis: {
      boundaryGap: false,
      data: data.date,
      type: 'category',
    },
    yAxis: {
      type: 'value',
    },
  };
  renderEcharts(options);
});
</script>

<template>
  <EchartsUI ref="loginLineRef" height="720px" width="100%" />
</template>

<style scoped></style>
