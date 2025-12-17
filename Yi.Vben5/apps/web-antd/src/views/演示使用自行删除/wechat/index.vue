<script setup lang="ts">
import { Page } from '@vben/common-ui';

import { Image } from 'ant-design-vue';

import wechatGroupImg from './img-base64.txt?raw';
</script>

<template>
  <Page>
    <div class="flex w-[360px] flex-col gap-2">
      <!-- <Alert :show-icon="true" message="人数已满，需要手动拉人" type="info" /> -->
      <div>
        <Image
          :preview="false"
          :src="wechatGroupImg"
          :width="240"
          class="pointer-events-none select-none"
        />
      </div>
    </div>
  </Page>
</template>
