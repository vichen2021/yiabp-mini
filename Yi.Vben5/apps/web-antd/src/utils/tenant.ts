import { DEFAULT_TENANT_ID } from '@vben/constants';

import { createGlobalState, useStorage } from '@vueuse/core';

export const LOGIN_TENANT_ID_STORAGE_KEY = 'login-tenant-id';

export const useLoginTenantId = createGlobalState(() => {
  const loginTenantId = useStorage(
    LOGIN_TENANT_ID_STORAGE_KEY,
    DEFAULT_TENANT_ID,
  );

  return {
    loginTenantId,
  };
});
