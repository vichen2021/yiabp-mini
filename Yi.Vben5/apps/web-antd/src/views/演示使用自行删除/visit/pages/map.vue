<script setup lang="ts">
import type { EChartsOption } from 'echarts';

// import * as echarts from 'echarts';
import { onMounted, ref } from 'vue';

import {
  EchartsUI,
  type EchartsUIType,
  useEcharts,
} from '@vben/plugins/echarts';

import * as echarts from 'echarts/core';

import { type Temp, visitList } from '../api';
import * as chinaMap from '../china.json';

defineOptions({ name: 'VisitMap' });

const mapRef = ref<EchartsUIType>();
const { renderEcharts } = useEcharts(mapRef);

function transformData(data: Temp[]) {
  const nameList: string[] = chinaMap.features.map(
    (item) => item.properties.name,
  );
  // eslint-disable-next-line unicorn/prefer-set-has
  const dataNameList: string[] = data.map((item) => item.name);
  // 差集
  const diff = nameList.filter(
    (item) => !dataNameList.includes(item) && item.trim() !== '',
  );
  diff.forEach((name) => {
    data.push({
      name,
      value: 0,
    });
  });
}

onMounted(async () => {
  echarts.registerMap('china', chinaMap as any);
  const data = await visitList();
  transformData(data);
  const max = Math.max.apply(
    null,
    data.map((item) => item.value),
  );
  const options: EChartsOption = {
    series: [
      {
        data,
        emphasis: {
          label: {
            show: true,
          },
        },
        label: {
          // formatter: '{b}\n{c}',
          formatter: '{c}',
          position: 'inside',
          show: true,
        },
        map: 'china',
        roam: true,
        // 由于缩放  这里加上偏移
        top: 200,
        type: 'map',
        zoom: 1.5,
      },
    ],
    title: {
      left: 'right',
      text: '用户访问量数据',
    },
    toolbox: {
      feature: {
        dataView: { readOnly: true },
        saveAsImage: {},
      },
      // orient: 'vertical',
      left: 'left',
      show: true,
      top: 'top',
    },
    tooltip: {
      formatter: '{b}<br/>{c}',
      showDelay: 0,
      transitionDuration: 0.2,
      trigger: 'item',
    },
    visualMap: {
      calculable: true,
      inRange: {
        color: ['#ffffff', '#00FF66', '#00CCFF', '#CC6600'],
      },
      left: 'left',
      max,
      min: 0,
      text: ['最高', '最低'],
    },
  };
  renderEcharts(options);
});
</script>

<template>
  <EchartsUI ref="mapRef" height="720px" width="100%" />
</template>

<style scoped></style>
