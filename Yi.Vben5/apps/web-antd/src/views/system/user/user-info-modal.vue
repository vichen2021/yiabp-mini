<script setup lang="ts">
import type { User } from '#/api/system/user/model';

import { computed, shallowRef } from 'vue';

import {
  useVbenModal,
  VbenDescriptions,
  VbenDescriptionsItem,
} from '@vben/common-ui';

import { Avatar, Tag } from 'antdv-next';

import { findUserInfo } from '#/api/system/user';

const [BasicModal, modalApi] = useVbenModal({
  onOpenChange: handleOpenChange,
  onClosed() {
    currentUser.value = null;
  },
});

const currentUser = shallowRef<null | User>(null);

async function handleOpenChange(open: boolean) {
  if (!open) {
    return null;
  }
  modalApi.modalLoading(true);

  const { userId } = modalApi.getData() as { userId: number | string };
  const response = await findUserInfo(userId);

  // 新接口直接返回完整的用户数据，包含posts和roles数组
  currentUser.value = response as User;

  modalApi.modalLoading(false);
}

const sexLabel = computed(() => {
  if (!currentUser.value) {
    return '-';
  }
  const { sex } = currentUser.value;
  if (sex === 'Man') return '男';
  if (sex === 'Woman') return '女';
  return '-';
});
</script>

<template>
  <BasicModal :footer="false" :fullscreen-button="false" title="用户信息">
    <VbenDescriptions v-if="currentUser" size="small" :column="1" bordered>
      <VbenDescriptionsItem label="用户ID">
        {{ currentUser.id }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="头像">
        <Avatar v-if="currentUser.icon" :src="currentUser.icon" :size="48" />
        <span v-else>-</span>
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="姓名">
        {{ currentUser.name || '-' }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="昵称">
        {{ currentUser.nick || '-' }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="用户名">
        {{ currentUser.userName || '-' }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="年龄">
        {{ currentUser.age || '-' }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="性别">
        {{ sexLabel }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="用户状态">
        <Tag :color="currentUser.state ? 'success' : 'error'">
          {{ currentUser.state ? '启用' : '禁用' }}
        </Tag>
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="手机号">
        {{ currentUser.phone || '-' }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="邮箱">
        {{ currentUser.email || '-' }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="地址">
        {{ currentUser.address || '-' }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="IP地址">
        {{ currentUser.ip || '-' }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="个人简介">
        {{ currentUser.introduction || '-' }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="岗位">
        <div
          v-if="currentUser.posts && currentUser.posts.length > 0"
          class="flex flex-wrap gap-0.5"
        >
          <Tag v-for="item in currentUser.posts" :key="item.postId">
            {{ item.postName }}
          </Tag>
        </div>
        <span v-else>-</span>
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="角色">
        <div
          v-if="currentUser.roles && currentUser.roles.length > 0"
          class="flex flex-wrap gap-0.5"
        >
          <Tag v-for="item in currentUser.roles" :key="item.roleId">
            {{ item.roleName }}
          </Tag>
        </div>
        <span v-else>-</span>
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="创建时间">
        {{ currentUser.creationTime }}
      </VbenDescriptionsItem>
      <VbenDescriptionsItem label="备注">
        {{ currentUser.remark || '-' }}
      </VbenDescriptionsItem>
    </VbenDescriptions>
  </BasicModal>
</template>
