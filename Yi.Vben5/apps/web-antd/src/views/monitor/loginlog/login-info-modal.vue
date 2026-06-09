<script setup lang="ts">
import type { LoginLog } from '#/api/monitor/logininfo/model';

import { ref } from 'vue';

import {
  useVbenModal,
  VbenDescriptions,
  VbenDescriptionsItem,
} from '@vben/common-ui';

import { renderBrowserIcon, renderOsIcon } from '#/utils/render';

const loginInfo = ref<LoginLog>();
const [BasicModal, modalApi] = useVbenModal({
  onOpenChange: (isOpen) => {
    if (!isOpen) {
      return null;
    }
    const record = modalApi.getData() as LoginLog;
    loginInfo.value = record;
  },
  onClosed() {
    loginInfo.value = undefined;
  },
});
</script>

<template>
  <BasicModal
    :footer="false"
    :fullscreen-button="false"
    class="w-[550px]"
    title="登录日志"
  >
    <VbenDescriptions v-if="loginInfo" size="small" :column="1" bordered>
      <VbenDescriptionsItem label="账号信息">
        {{
          `账号: ${loginInfo.loginUser} / ${loginInfo.loginIp} / ${loginInfo.loginLocation}`
        }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="登录时间">
        {{ loginInfo.creationTime }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="登录信息">
        <span class="font-semibold">
          {{ loginInfo.logMsg }}
        </span>
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="登录设备">
        <component :is="renderOsIcon(loginInfo.os)" />
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="浏览器">
        <component :is="renderBrowserIcon(loginInfo.browser)" />
      </VbenDescriptionsItem>
    </VbenDescriptions>
  </BasicModal>
</template>
