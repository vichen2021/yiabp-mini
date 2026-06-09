<script setup lang="ts">
import { computed, ref } from 'vue';

import { confirm } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { Switch } from 'antdv-next';
import { isFunction } from 'lodash-es';

type CheckedType = boolean | number | string;

interface Props {
  /**
   * 选中的文本
   * @default i18n 启用
   */
  checkedText?: string;
  /**
   * 未选中的文本
   * @default i18n 禁用
   */
  unCheckedText?: string;
  checkedValue?: CheckedType;
  unCheckedValue?: CheckedType;
  disabled?: boolean;
  /**
   * 需要自己在内部处理更新的逻辑 因为status已经双向绑定了 可以直接获取
   */
  api: () => PromiseLike<void>;
  /**
   * 更新前是否弹窗确认
   * @default false
   */
  confirm?: boolean;
  /**
   * 对应的提示内容
   * @param checked 选中的值(更新后的值)
   * @default string '确认要更新状态吗？'
   */
  confirmText?: (checked: CheckedType) => string;
}

const props = withDefaults(defineProps<Props>(), {
  checkedText: undefined,
  unCheckedText: undefined,
  checkedValue: true,
  unCheckedValue: false,
  confirm: false,
  confirmText: undefined,
});

const emit = defineEmits<{ reload: [] }>();

// 修改为computed 支持语言切换
const checkedTextComputed = computed(() => {
  return props.checkedText ?? $t('pages.common.enable');
});

const unCheckedTextComputed = computed(() => {
  return props.unCheckedText ?? $t('pages.common.disable');
});

const currentChecked = defineModel<CheckedType>('value', {
  default: false,
});

const loading = ref(false);

async function confirmUpdate(checked: CheckedType, lastStatus: CheckedType) {
  const content = isFunction(props.confirmText)
    ? props.confirmText(checked)
    : `确认要更新状态吗？`;

  try {
    await confirm({
      cancelText: $t('common.cancel'),
      centered: true,
      confirmText: $t('common.confirm'),
      icon: 'warning',
      title: '提示',
      content,
    });
  } catch {
    currentChecked.value = lastStatus;
    return;
  }

  try {
    loading.value = true;
    const { api } = props;
    isFunction(api) && (await api());
    emit('reload');
  } catch {
    currentChecked.value = lastStatus;
  } finally {
    loading.value = false;
  }
}

async function handleChange(checked: CheckedType, e: Event) {
  // 阻止事件冒泡 否则会跟行选中冲突
  e.stopPropagation();
  const { checkedValue, unCheckedValue } = props;
  // 原本的状态
  const lastStatus = checked === checkedValue ? unCheckedValue : checkedValue;
  // 切换状态
  currentChecked.value = checked;
  const { api } = props;
  try {
    loading.value = true;

    if (props.confirm) {
      loading.value = false;
      await confirmUpdate(checked, lastStatus);
      return;
    }

    isFunction(api) && (await api());
    emit('reload');
  } catch {
    currentChecked.value = lastStatus;
  } finally {
    loading.value = false;
  }
}
</script>

<template>
  <Switch
    v-bind="$attrs"
    :loading="loading"
    :disabled="disabled"
    :checked="currentChecked"
    :checked-children="checkedTextComputed"
    :checked-value="checkedValue"
    :un-checked-children="unCheckedTextComputed"
    :un-checked-value="unCheckedValue"
    @change="handleChange"
  />
</template>
