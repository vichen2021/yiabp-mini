<!--
2025年03月08日重构为原生表单(反向重构??)
该文件作为例子 使用原生表单而非useVbenForm
-->
<script setup lang="ts">
import type { FormInstance } from 'antdv-next';
import type { RuleObject } from 'antdv-next/dist/form/types';

import { computed, ref } from 'vue';

import { useVbenModal } from '@vben/common-ui';
import { DictEnum } from '@vben/constants';
import { $t } from '@vben/locales';
import { cloneDeep } from '@vben/utils';

import { Form, FormItem, Input, RadioGroup } from 'antdv-next';
import { pick } from 'lodash-es';

import type { Notice } from '#/api/system/notice/model';

import { noticeAdd, noticeInfo, noticeUpdate } from '#/api/system/notice';
import { Tinymce } from '#/components/tinymce';
import { getDictOptions } from '#/utils/dict';
import { useBeforeCloseDiff } from '#/utils/popup';

const emit = defineEmits<{ reload: [] }>();

const isUpdate = ref(false);
const title = computed(() => {
  return isUpdate.value ? $t('pages.common.edit') : $t('pages.common.add');
});

/**
 * 定义表单数据类型
 */
interface FormData {
  id?: string;
  title?: string;
  state?: boolean | string;
  type?: string;
  content?: string;
}

/**
 * 定义默认值 用于reset
 */
const defaultValues: FormData = {
  id: undefined,
  title: '',
  state: '0',
  type: '1',
  content: '',
};

/**
 * 表单数据ref
 */
const formData = ref(defaultValues);
const formRef = ref<FormInstance>();

type AntdFormRules<T> = Partial<Record<keyof T, RuleObject[]>> & {
  [key: string]: RuleObject[];
};
/**
 * 表单校验规则
 */
const formRules = ref<AntdFormRules<FormData>>({
  state: [{ required: true, message: $t('ui.formRules.selectRequired') }],
  content: [{ required: true, message: $t('ui.formRules.required') }],
  type: [{ required: true, message: $t('ui.formRules.selectRequired') }],
  title: [{ required: true, message: $t('ui.formRules.required') }],
});

function customFormValueGetter() {
  return JSON.stringify(formData.value);
}

const { onBeforeClose, markInitialized, resetInitialized } = useBeforeCloseDiff(
  {
    initializedGetter: customFormValueGetter,
    currentGetter: customFormValueGetter,
  },
);

const [BasicModal, modalApi] = useVbenModal({
  class: 'w-[800px]',
  fullscreenButton: true,
  onBeforeClose,
  onClosed: handleClosed,
  onConfirm: handleConfirm,
  onOpenChange: async (isOpen) => {
    if (!isOpen) {
      return null;
    }
    modalApi.modalLoading(true);

    const { id } = modalApi.getData() as { id?: string };
    isUpdate.value = !!id;
    if (isUpdate.value && id) {
      const record = await noticeInfo(id);
      // 只赋值存在的字段，并处理state的转换（boolean转string用于RadioGroup）
      const filterRecord = pick(record, Object.keys(defaultValues));
      formData.value = {
        ...filterRecord,
        state: typeof filterRecord.state === 'boolean' ? (filterRecord.state ? '1' : '0') : filterRecord.state,
      };
    }
    await markInitialized();

    modalApi.modalLoading(false);
  },
});

async function handleConfirm() {
  try {
    modalApi.lock(true);
    await formRef.value?.validate();
    // 可能会做数据处理 使用cloneDeep深拷贝
    const data = cloneDeep(formData.value);
    // 转换state从string到boolean（RadioGroup返回string，但API需要boolean）
    if (typeof data.state === 'string') {
      data.state = data.state === '1' || data.state === 'true';
    }
    await (isUpdate.value ? noticeUpdate(data) : noticeAdd(data as Partial<Notice>));
    resetInitialized();
    emit('reload');
    modalApi.close();
  } catch (error) {
    console.error(error);
  } finally {
    modalApi.lock(false);
  }
}

async function handleClosed() {
  formData.value = defaultValues;
  formRef.value?.resetFields();
  resetInitialized();
}
</script>

<template>
  <BasicModal :title="title">
    <Form ref="formRef" layout="vertical" :model="formData" :rules="formRules">
      <FormItem label="公告标题" name="title">
        <Input
          :placeholder="$t('ui.formRules.required')"
          v-model:value="formData.title"
        />
      </FormItem>
      <div class="grid sm:grid-cols-1 lg:grid-cols-2">
        <FormItem label="公告状态" name="state">
          <RadioGroup
            button-style="solid"
            option-type="button"
            v-model:value="formData.state"
            :options="getDictOptions(DictEnum.SYS_NOTICE_STATUS)"
          />
        </FormItem>
        <FormItem label="公告类型" name="type">
          <RadioGroup
            button-style="solid"
            option-type="button"
            v-model:value="formData.type"
            :options="getDictOptions(DictEnum.SYS_NOTICE_TYPE)"
          />
        </FormItem>
      </div>
      <FormItem label="公告内容" name="content">
        <Tinymce v-model="formData.content" />
      </FormItem>
    </Form>
  </BasicModal>
</template>
