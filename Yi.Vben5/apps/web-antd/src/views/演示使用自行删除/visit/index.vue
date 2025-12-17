<script lang="ts" setup>
import { ref } from 'vue';

import { Tabs } from 'ant-design-vue';

import Browser from './pages/browser.vue';
import Device from './pages/device.vue';
import Isp from './pages/isp.vue';
import LoginLine from './pages/loginLine.vue';
import VisitMap from './pages/map.vue';

const TabPane = Tabs.TabPane;

const activeKey = ref<number>(1);
</script>
<template>
  <div class="pt-[16px]">
    <Tabs v-model:activeKey="activeKey" class="h-full" tab-position="left">
      <TabPane :key="1" tab="访问量数据"> <VisitMap /> </TabPane>
      <TabPane :key="2" tab="使用设备"><Device /></TabPane>
      <TabPane :key="3" tab="使用浏览器"><Browser /></TabPane>
      <TabPane :key="4" tab="登录量"><LoginLine /></TabPane>
      <TabPane :key="5" tab="运营商占比"><Isp /></TabPane>
    </Tabs>
  </div>
</template>
