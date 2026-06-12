<script setup lang="ts">
import type { ActionItem } from './types';

import { computed, ref } from 'vue';

import { cn } from '@vben-core/shared/utils';

import { Popover, PopoverContent, PopoverTrigger } from '../../ui';
import { VbenButton } from '../button';
import { VbenIcon } from '../icon';

const props = defineProps<{ action: ActionItem }>();

const open = ref(false);

const buttonClass = computed(() =>
  cn(
    'gap-1',
    props.action.danger && 'text-destructive hover:text-destructive',
    props.action.class,
  ),
);

const variant = computed(() => props.action.variant ?? 'link');
const size = computed(() => props.action.size ?? 'default');

function onClick() {
  if (props.action.disabled || props.action.loading) return;
  props.action.onClick?.();
}

function onConfirm() {
  open.value = false;
  const pc = props.action.popConfirm;
  if (pc?.confirm) {
    pc.confirm();
  } else {
    props.action.onClick?.();
  }
}

function onCancel() {
  open.value = false;
}
</script>

<template>
  <!-- 气泡确认 -->
  <Popover v-if="action.popConfirm" v-model:open="open">
    <PopoverTrigger as-child>
      <VbenButton
        :class="buttonClass"
        :disabled="action.disabled"
        :loading="action.loading"
        :size="size"
        class="p-2"
        :variant="variant"
      >
        <VbenIcon :icon="action.icon" v-if="action.icon" class="size-4" />
        <span v-if="action.text">{{ action.text }}</span>
      </VbenButton>
    </PopoverTrigger>
    <PopoverContent
      align="center"
      class="z-popup relative w-auto min-w-[150px] max-w-[220px] rounded-[8px] border border-[#f0f0f0] bg-[#ffffff] px-3 py-2.5 text-[#1f1f1f] shadow-[0_6px_16px_0_rgba(0,0,0,0.08),0_3px_6px_-4px_rgba(0,0,0,0.12),0_9px_28px_8px_rgba(0,0,0,0.05)] dark:border-border dark:bg-popover dark:text-popover-foreground"
      :side-offset="8"
      side="left"
    >
      <div
        class="absolute right-[-4px] top-1/2 size-2 -translate-y-1/2 rotate-45 border-r border-t border-[#f0f0f0] bg-[#ffffff] dark:border-border dark:bg-popover"
      ></div>
      <div class="mb-2 flex items-center gap-2 text-sm leading-[22px]">
        <span
          class="inline-flex size-3.5 shrink-0 items-center justify-center rounded-full bg-[#faad14] text-[10px] font-bold leading-none text-white"
        >
          !
        </span>
        <span>{{ action.popConfirm.title ?? '确认删除？' }}</span>
      </div>
      <div class="flex justify-end gap-3">
        <button
          class="inline-flex h-6 items-center justify-center rounded-[4px] border border-[#d9d9d9] bg-[#ffffff] px-[7px] text-sm leading-[22px] text-[#1f1f1f] hover:border-primary hover:text-primary dark:border-border dark:bg-background dark:text-foreground"
          type="button"
          @click="onCancel"
        >
          {{ action.popConfirm.cancelText ?? '取消' }}
        </button>
        <button
          class="inline-flex h-6 items-center justify-center rounded-[4px] border border-primary bg-primary px-[7px] text-sm leading-[22px] text-primary-foreground hover:bg-primary/90"
          type="button"
          @click="onConfirm"
        >
          {{ action.popConfirm.okText ?? '确定' }}
        </button>
      </div>
    </PopoverContent>
  </Popover>

  <!-- 普通按钮 -->
  <VbenButton
    v-else
    :class="buttonClass"
    :disabled="action.disabled"
    :loading="action.loading"
    :size="size"
    class="p-2"
    :variant="variant"
    @click="onClick"
  >
    <VbenIcon :icon="action.icon" v-if="action.icon" class="size-4" />
    <span v-if="action.text">{{ action.text }}</span>
  </VbenButton>
</template>
