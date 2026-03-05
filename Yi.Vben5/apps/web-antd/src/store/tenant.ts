import type { TenantResp } from '#/api/core/auth';

import { ref } from 'vue';

import { defineStore } from 'pinia';

import { tenantList as tenantListApi } from '#/api/core/auth';

/**
 * 用于超级管理员切换租户
 */
export const useTenantStore = defineStore('app-tenant', () => {
  // 是否已经选中租户
  const checked = ref(false);
  // 是否开启租户功能
  const tenantEnable = ref(true);
  const tenantList = ref<TenantResp[]>([]);

  // 初始化 获取租户信息
  async function initTenant() {
    const list = await tenantListApi();
    tenantList.value = list;
    // 如果有租户数据则启用
    tenantEnable.value = list.length > 0;
  }

  async function setChecked(_checked: boolean) {
    checked.value = _checked;
  }

  function $reset() {
    checked.value = false;
    tenantEnable.value = true;
    tenantList.value = [];
  }

  return {
    $reset,
    checked,
    initTenant,
    setChecked,
    tenantEnable,
    tenantList,
  };
});
