<script setup lang="ts">
import { computed, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';

import { useVbenForm } from '#/adapter/form';

import { sseSendAll, sseSendByUserId } from './api';

const currentUserId = ref<string | undefined>(undefined);
const title = computed(() => {
  return currentUserId.value ? '发送指定消息' : '发送全体消息';
});

const [BasicModal, modalApi] = useVbenModal({
  onConfirm: handleSubmit,
  onOpenChange: (isOpen) => {
    if (!isOpen) {
      return null;
    }
    const data = modalApi.getData() as { userId: string | undefined };
    currentUserId.value = data.userId;
  },
});

const [BasicForm, formApi] = useVbenForm({
  layout: 'vertical',
  commonConfig: {
    formItemClass: 'col-span-2',
    componentProps: {
      class: 'w-full',
    },
    labelWidth: 80,
  },
  schema: [
    {
      component: 'Textarea',
      label: '消息内容',
      fieldName: 'content',
      rules: 'required',
    },
  ],
  showDefaultActions: false,
  wrapperClass: 'grid-cols-2',
});

async function handleSubmit() {
  try {
    modalApi.modalLoading(true);

    const { valid } = await formApi.validate();
    if (!valid) {
      return;
    }
    const { content } = await formApi.getValues();

    await (currentUserId.value
      ? sseSendByUserId(currentUserId.value, content)
      : sseSendAll(content));
    modalApi.close();
  } catch (error) {
    console.error(error);
  } finally {
    modalApi.modalLoading(false);
  }
}
</script>

<template>
  <BasicModal
    :close-on-click-modal="false"
    :fullscreen-button="false"
    :title="title"
  >
    <BasicForm />
  </BasicModal>
</template>
