<script setup lang="ts">
import { computed, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { cloneDeep } from '@vben/utils';

import { useVbenForm } from '#/adapter/form';
import { fieldAdd, fieldInfo, fieldUpdate } from '#/api/code/field';
import { getFieldTypeEnum } from '#/api/code/field';
import { tableSelectList } from '#/api/code/table';
import { defaultFormValueGetter, useBeforeCloseDiff } from '#/utils/popup';

import { drawerSchema } from './data';

const emit = defineEmits<{ reload: [] }>();

const isUpdate = ref(false);
const title = computed(() => {
  return isUpdate.value ? $t('pages.common.edit') : $t('pages.common.add');
});

const [BasicForm, formApi] = useVbenForm({
  commonConfig: {
    labelWidth: 100,
  },
  schema: drawerSchema(),
  showDefaultActions: false,
});

const { onBeforeClose, markInitialized, resetInitialized } = useBeforeCloseDiff(
  {
    initializedGetter: defaultFormValueGetter(formApi),
    currentGetter: defaultFormValueGetter(formApi),
  },
);

const [BasicDrawer, drawerApi] = useVbenDrawer({
  onBeforeClose,
  onClosed: handleClosed,
  onConfirm: handleConfirm,
  onOpenChange: async (isOpen) => {
    if (!isOpen) {
      return null;
    }
    drawerApi.drawerLoading(true);

    try {
      const { id } = drawerApi.getData() as { id?: string };
      isUpdate.value = !!id;

      // 加载表选项
      const tables = await tableSelectList();
      const tableOptions = tables.map((table) => ({
        label: `${table.name}${table.description ? ` (${table.description})` : ''}`,
        value: table.id,
      }));

      // 加载字段类型选项
      const fieldTypes = await getFieldTypeEnum();

      // 更新表单 schema
      formApi.updateSchema([
        {
          componentProps: {
            options: tableOptions,
          },
          fieldName: 'tableId',
        },
        {
          componentProps: {
            options: fieldTypes,
          },
          fieldName: 'fieldType',
        },
      ]);

      if (isUpdate.value && id) {
        const record = await fieldInfo(id);
        await formApi.setValues(record);
      }
      await markInitialized();
    } catch (error) {
      console.error('加载数据失败:', error);
    } finally {
      drawerApi.drawerLoading(false);
    }
  },
});

async function handleConfirm() {
  try {
    drawerApi.lock(true);
    const { valid } = await formApi.validate();
    if (!valid) {
      return;
    }
    const data = cloneDeep(await formApi.getValues());
    await (isUpdate.value ? fieldUpdate(data) : fieldAdd(data));
    resetInitialized();
    emit('reload');
    drawerApi.close();
  } catch (error) {
    console.error(error);
  } finally {
    drawerApi.lock(false);
  }
}

async function handleClosed() {
  await formApi.resetForm();
  resetInitialized();
}
</script>

<template>
  <BasicDrawer :title="title" class="w-[600px]">
    <BasicForm />
  </BasicDrawer>
</template>
