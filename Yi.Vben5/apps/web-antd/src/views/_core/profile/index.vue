<script setup lang="ts">
import type { UserInfoResp } from '#/api/core/user';
import type { BasicUserInfo } from '@vben/types';

import { computed, onMounted, onUnmounted, ref } from 'vue';

import { Profile } from '@vben/common-ui';
import { useUserStore } from '@vben/stores';

import { userProfile, userUpdateAvatar } from '#/api/system/profile';
import { CropperAvatar } from '#/components/cropper';
import { useAuthStore } from '#/store';

import AccountBind from './components/account-bind.vue';
import BaseSetting from './components/base-setting.vue';
import OnlineDevice from './components/online-device.vue';
import SecureSetting from './components/secure-setting.vue';
import { emitter } from './mitt';

const profile = ref<UserInfoResp>();
const tabsValue = ref('basic');
const authStore = useAuthStore();
const userStore = useUserStore();
const tabs = [
  {
    label: '基本设置',
    value: 'basic',
  },
  {
    label: '安全设置',
    value: 'security',
  },
  {
    label: '账号绑定',
    value: 'accountBind',
  },
  {
    label: '在线设备',
    value: 'onlineDevice',
  },
];

async function loadProfile() {
  try {
    const resp = await userProfile();
    profile.value = resp;
  } catch (error) {
    console.error('加载用户信息失败:', error);
  }
}

onMounted(loadProfile);

const profileUserInfo = computed<BasicUserInfo | null>(() => {
  const user = profile.value?.user;
  if (!user) {
    return null;
  }

  return {
    avatar: user.icon,
    realName: user.nick || user.userName,
    roles: profile.value?.roles ?? [],
    userId: String(user.userId),
    username: user.userName,
  };
});

async function handleUploadFinish() {
  await loadProfile();
  const userInfo = await authStore.fetchUserInfo();
  userStore.setUserInfo(userInfo);
}

onMounted(() => emitter.on('updateProfile', loadProfile));
onUnmounted(() => emitter.off('updateProfile'));
</script>

<template>
  <Profile
    v-model:model-value="tabsValue"
    title="个人中心"
    :tabs="tabs"
    :user-info="profileUserInfo"
  >
    <template #avatar>
      <CropperAvatar
        :show-btn="false"
        :upload-api="userUpdateAvatar"
        :value="profileUserInfo?.avatar"
        width="80"
        @change="handleUploadFinish"
      />
    </template>
    <template #content>
      <BaseSetting v-if="profile && tabsValue === 'basic'" :profile="profile" />
      <SecureSetting
        v-if="profile && tabsValue === 'security'"
        :profile="profile"
      />
      <AccountBind v-if="profile && tabsValue === 'accountBind'" />
      <OnlineDevice v-if="profile && tabsValue === 'onlineDevice'" />
    </template>
  </Profile>
</template>
