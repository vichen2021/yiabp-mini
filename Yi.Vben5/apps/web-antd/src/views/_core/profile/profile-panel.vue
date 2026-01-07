<script setup lang="ts">
import type { UserInfoResp } from '#/api/core/user';

import { computed } from 'vue';

import { preferences, usePreferences } from '@vben/preferences';

import {
  Card,
  Descriptions,
  DescriptionsItem,
  Tag,
  Tooltip,
} from 'ant-design-vue';

import { userUpdateAvatar } from '#/api/system/profile';
import { CropperAvatar } from '#/components/cropper';

const props = defineProps<{ profile?: UserInfoResp }>();

defineEmits<{
  // 头像上传完毕
  uploadFinish: [];
}>();

const avatar = computed(
  () => props.profile?.user.avatar || preferences.app.defaultAvatar,
);

const { isDark } = usePreferences();
const poetrySrc = computed(() => {
  const color = isDark.value ? 'white' : 'gray';
  return `https://v2.jinrishici.com/one.svg?font-size=12&color=${color}`;
});
</script>

<template>
  <Card :loading="!profile" class="h-full lg:w-1/3">
    <div v-if="profile" class="flex flex-col items-center gap-[24px]">
      <div class="flex flex-col items-center gap-[20px]">
        <Tooltip title="点击上传头像">
          <CropperAvatar
            :show-btn="false"
            :upload-api="userUpdateAvatar"
            :value="avatar"
            width="120"
            @change="$emit('uploadFinish')"
          />
        </Tooltip>
        <div class="flex flex-col items-center gap-[8px]">
          <span class="text-foreground text-xl font-bold">
            {{ profile.user.nick ?? '未知' }}
          </span>
          <!-- https://www.jinrishici.com/doc/#image -->
          <img :src="poetrySrc" />
        </div>
      </div>
      <div class="px-[24px]">
        <Descriptions :column="1">
          <DescriptionsItem label="账号">
            {{ profile.user.userName }}
          </DescriptionsItem>
          <DescriptionsItem label="手机号码">
            {{ profile.user.phone || '未绑定手机号' }}
          </DescriptionsItem>
          <DescriptionsItem label="邮箱">
            {{ profile.user.email || '未绑定邮箱' }}
          </DescriptionsItem>
          <DescriptionsItem label="部门">
            <Tag color="processing">
              {{ profile.user.deptName ?? '未分配部门' }}
            </Tag>
          </DescriptionsItem>
          <DescriptionsItem v-if="profile.roles && profile.roles.length > 0" label="角色">
            <Tag v-for="role in profile.roles" :key="role" color="processing">
              {{ role }}
            </Tag>
          </DescriptionsItem>
          <DescriptionsItem label="注册时间">
            {{ profile.user.creationTime }}
          </DescriptionsItem>
        </Descriptions>
      </div>
    </div>
  </Card>
</template>
