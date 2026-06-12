import { computed } from 'vue';

import { preferences, updatePreferences } from '@vben/preferences';
import { useAccessStore, useUserStore } from '@vben/stores';

const SUPERADMIN_ROLE_KEY = 'superadmin';

function useAccess() {
  const accessStore = useAccessStore();
  const userStore = useUserStore();
  const accessMode = computed(() => {
    return preferences.app.accessMode;
  });

  /**
   * 基于角色判断是否有权限
   * @description: Determine whether there is permission，The role is judged by the user's role
   * @param roles
   */
  function hasAccessByRoles(roles: string[]) {
    const userRoleSet = new Set(
      userStore.userRoles.map((role) => role.toLowerCase()),
    );
    if (userRoleSet.has(SUPERADMIN_ROLE_KEY)) {
      return true;
    }

    const intersection = roles.filter((item) =>
      userRoleSet.has(item.toLowerCase()),
    );
    return intersection.length > 0;
  }

  /**
   * 基于权限码判断是否有权限
   * @description: Determine whether there is permission，The permission code is judged by the user's permission code
   * @param codes
   */
  function hasAccessByCodes(codes: string[]) {
    const userCodesSet = new Set(accessStore.accessCodes);
    if (userCodesSet.has('*:*:*') || userCodesSet.has('*')) {
      return true;
    }

    const intersection = codes.filter((item) => userCodesSet.has(item));
    return intersection.length > 0;
  }

  async function toggleAccessMode() {
    updatePreferences({
      app: {
        accessMode:
          preferences.app.accessMode === 'frontend' ? 'backend' : 'frontend',
      },
    });
  }

  return {
    accessMode,
    hasAccessByCodes,
    hasAccessByRoles,
    toggleAccessMode,
  };
}

export { useAccess };
