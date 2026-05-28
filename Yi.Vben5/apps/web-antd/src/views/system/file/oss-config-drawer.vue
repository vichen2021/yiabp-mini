<script setup lang="ts">
import { useVbenDrawer } from '@vben/common-ui';

import { useVbenForm } from '#/adapter/form';
import { ossSettingGet, ossSettingUpdate } from '#/api/system/tenant-oss-settings';

const [BasicForm, formApi] = useVbenForm({
  commonConfig: {
    labelWidth: 140,
    componentProps: {
      class: 'w-full',
    },
  },
  schema: [
    {
      component: 'Divider',
      componentProps: { orientation: 'center' },
      fieldName: 'divider1',
      hideLabel: true,
      renderComponentContent: () => ({ default: () => '通用设置' }),
    },
    {
      component: 'Select',
      componentProps: {
        options: [
          { label: '本地文件系统', value: 'FileSystem' },
          { label: '阿里云 OSS', value: 'Aliyun' },
        ],
      },
      fieldName: 'provider',
      label: 'OSS 提供商',
    },
    {
      component: 'Input',
      fieldName: 'pathPrefix',
      label: '存储路径前缀',
      help: '用于构建文件 StorageKey，如：uploads/roco/',
    },
    {
      component: 'Divider',
      componentProps: { orientation: 'center' },
      dependencies: {
        show: (model) => model.provider === 'Aliyun',
        triggerFields: ['provider'],
      },
      fieldName: 'divider2',
      hideLabel: true,
      renderComponentContent: () => ({ default: () => '阿里云 OSS 配置' }),
    },
    {
      component: 'Input',
      dependencies: {
        show: (model) => model.provider === 'Aliyun',
        triggerFields: ['provider'],
      },
      fieldName: 'endpoint',
      label: 'Endpoint',
      help: '如：oss-cn-hangzhou.aliyuncs.com',
    },
    {
      component: 'Input',
      dependencies: {
        show: (model) => model.provider === 'Aliyun',
        triggerFields: ['provider'],
      },
      fieldName: 'containerName',
      label: 'Bucket 名称',
    },
    {
      component: 'Input',
      dependencies: {
        show: (model) => model.provider === 'Aliyun',
        triggerFields: ['provider'],
      },
      fieldName: 'customDomain',
      label: '自定义域名',
      help: '可选，如：https://cdn.example.com；为空时使用默认 Bucket 域名',
    },
    {
      component: 'Input',
      dependencies: {
        show: (model) => model.provider === 'Aliyun',
        triggerFields: ['provider'],
      },
      fieldName: 'accessKeyId',
      label: 'AccessKeyId',
    },
    {
      component: 'InputPassword',
      dependencies: {
        show: (model) => model.provider === 'Aliyun',
        triggerFields: ['provider'],
      },
      fieldName: 'accessKeySecret',
      label: 'AccessKeySecret',
      help: '留空则不修改原密钥',
    },
    {
      component: 'Switch',
      dependencies: {
        show: (model) => model.provider === 'Aliyun',
        triggerFields: ['provider'],
      },
      defaultValue: false,
      fieldName: 'createContainerIfNotExists',
      label: 'Bucket 不存在时自动创建',
    },
  ],
  showDefaultActions: false,
});

const [BasicDrawer, drawerApi] = useVbenDrawer({
  onConfirm: handleConfirm,
  async onOpenChange(isOpen) {
    if (!isOpen) return;
    drawerApi.drawerLoading(true);
    const data = await ossSettingGet();
    await formApi.setValues(data);
    drawerApi.drawerLoading(false);
  },
});

async function handleConfirm() {
  try {
    drawerApi.lock(true);
    const { valid } = await formApi.validate();
    if (!valid) return;
    const values = await formApi.getValues();
    await ossSettingUpdate({
      provider: values.provider,
      pathPrefix: values.pathPrefix,
      accessKeyId: values.accessKeyId,
      accessKeySecret: values.accessKeySecret,
      endpoint: values.endpoint,
      containerName: values.containerName,
      customDomain: values.customDomain,
      createContainerIfNotExists: values.createContainerIfNotExists ?? false,
    });
    drawerApi.close();
  } catch (error) {
    console.error(error);
  } finally {
    drawerApi.lock(false);
  }
}
</script>

<template>
  <BasicDrawer title="OSS 存储设置">
    <BasicForm />
  </BasicDrawer>
</template>
