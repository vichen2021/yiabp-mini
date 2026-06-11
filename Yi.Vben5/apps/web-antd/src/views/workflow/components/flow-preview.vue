<script setup lang="ts">
import { useAppConfig } from '@vben/hooks';
import { useAccessStore } from '@vben/stores';

import { useWarmflowIframe } from './hook';

defineOptions({ name: 'FlowPreview' });

const props = defineProps<{ instanceId: string }>();

const appConfig = useAppConfig(import.meta.env, import.meta.env.PROD) as any;
const clientId = appConfig.clientId ?? import.meta.env.VITE_GLOB_APP_CLIENT_ID;

const accessStore = useAccessStore();
const params = {
  Authorization: `Bearer ${accessStore.accessToken}`,
  id: props.instanceId,
  clientid: clientId,
  type: 'FlowChart',
};

/**
 * iframe地址
 */
const url = `${import.meta.env.VITE_GLOB_API_URL}/warm-flow-ui/index.html?${new URLSearchParams(params as Record<string, string>).toString()}`;

const { iframeRef: _iframeRef } = useWarmflowIframe();
</script>

<template>
  <iframe ref="iframeRef" :src="url" class="h-[500px] w-full border"></iframe>
</template>
