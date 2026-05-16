<script setup lang="ts">
import type { MenuOption } from '#/api/system/menu/model';
import type { TenantPackageCreateInput, TenantPackageUpdateInput } from '#/api/system/tenant-package/model';

import { computed, nextTick, ref } from 'vue';

import { useVbenDrawer } from '@vben/common-ui';
import { $t } from '@vben/locales';
import { cloneDeep, eachTree } from '@vben/utils';

import { omit } from 'lodash-es';

import { useVbenForm } from '#/adapter/form';
import { tenantPackageMenuTreeSelect } from '#/api/system/menu';
import {
  tenantPackageAdd,
  tenantPackageInfo,
  tenantPackageUpdate,
} from '#/api/system/tenant-package';
import { MenuSelectTable } from '#/components/tree';
import { EMPTY_GUID } from '#/utils/guid';
import { defaultFormValueGetter, useBeforeCloseDiff } from '#/utils/popup';

import { drawerSchema } from './data';

const emit = defineEmits<{ reload: [] }>();

const isUpdate = ref(false);
const title = computed(() => {
  return isUpdate.value ? $t('pages.common.edit') : $t('pages.common.add');
});

const [BasicForm, formApi] = useVbenForm({
  commonConfig: {
    formItemClass: 'col-span-2',
  },
  layout: 'vertical',
  schema: drawerSchema(),
  showDefaultActions: false,
  wrapperClass: 'grid-cols-2',
});

const menuTree = ref<MenuOption[]>([]);
async function setupMenuTree(id?: string) {
  // 新增使用空 GUID 获取除了`租户管理`的所有菜单
  const resp = await tenantPackageMenuTreeSelect(id ?? EMPTY_GUID);
  const menus = resp.menus;
  // i18n处理
  eachTree(menus, (node) => {
    node.menuName = $t(node.menuName);
  });
  // 设置菜单信息
  menuTree.value = menus;
  // keys依赖于menu 需要先加载menu
  await nextTick();
  await formApi.setFieldValue('menuIds', resp.checkedKeys);
}

async function customFormValueGetter() {
  const v = await defaultFormValueGetter(formApi)();
  // 获取勾选信息
  const menuIds = menuSelectRef.value?.getCheckedKeys?.() ?? [];
  const mixStr = v + menuIds.join(',');
  return mixStr;
}

const { onBeforeClose, markInitialized, resetInitialized } = useBeforeCloseDiff(
  {
    initializedGetter: customFormValueGetter,
    currentGetter: customFormValueGetter,
  },
);

const [BasicDrawer, drawerApi] = useVbenDrawer({
  onBeforeClose,
  onClosed: handleClosed,
  onConfirm: handleConfirm,
  destroyOnClose: true,
  async onOpenChange(isOpen) {
    if (!isOpen) {
      return null;
    }
    drawerApi.drawerLoading(true);

    const { id } = drawerApi.getData() as { id?: number | string };
    isUpdate.value = !!id;
    if (isUpdate.value && id) {
      const record = await tenantPackageInfo(id);
      // 需要排除menuIds menuIds为string
      // 通过setupMenuTreeSelect设置
      await formApi.setValues(omit(record, ['menuIds']));
    }
    // init菜单 注意顺序要放在赋值record之后 内部watch会依赖record
    await setupMenuTree(id?.toString());
    await markInitialized();

    drawerApi.drawerLoading(false);
  },
});

const menuSelectRef = ref<InstanceType<typeof MenuSelectTable>>();
async function handleConfirm() {
  try {
    drawerApi.drawerLoading(true);
    const { valid } = await formApi.validate();
    if (!valid) {
      return;
    }
    // 这个用于提交
    const menuIds = menuSelectRef.value?.getCheckedKeys?.() ?? [];
    // formApi.getValues拿到的是一个readonly对象，不能直接修改，需要cloneDeep
    const data = cloneDeep(await formApi.getValues()) as TenantPackageCreateInput & { menuIds: string[] };
    data.menuIds = menuIds as string[];
    if (isUpdate.value) {
      await tenantPackageUpdate(data as unknown as TenantPackageUpdateInput);
    } else {
      await tenantPackageAdd(data);
    }
    resetInitialized();
    emit('reload');
    drawerApi.close();
  } catch (error) {
    console.error(error);
  } finally {
    drawerApi.drawerLoading(false);
  }
}

async function handleClosed() {
  await formApi.resetForm();
  resetInitialized();
}

/**
 * 通过回调更新 无法通过v-model
 * @param value 菜单选择是否严格模式
 */
function handleMenuCheckStrictlyChange(value: boolean) {
  formApi.setFieldValue('menuCheckStrictly', value);
}
</script>

<template>
  <BasicDrawer :title="title" class="w-[800px]">
    <BasicForm>
      <template #menuIds="slotProps">
        <div class="h-[600px] w-full">
          <!-- association为readonly 不能通过v-model绑定 -->
          <MenuSelectTable
            ref="menuSelectRef"
            :checked-keys="slotProps.value"
            :association="formApi.form.values.menuCheckStrictly"
            :menus="menuTree"
            @update:association="handleMenuCheckStrictlyChange"
          />
        </div>
      </template>
    </BasicForm>
  </BasicDrawer>
</template>
