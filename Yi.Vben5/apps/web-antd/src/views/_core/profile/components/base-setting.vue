<script setup lang="ts">
import type { Recordable } from '@vben/types';

import type { UserInfoResp } from '#/api/core/user';

import { onMounted, ref } from 'vue';

import { DictEnum } from '@vben/constants';
import { ProfileBaseSetting } from '@vben/common-ui';
import { useUserStore } from '@vben/stores';

import { cloneDeep } from 'lodash-es';

import { z } from '#/adapter/form';
import { userProfileUpdate } from '#/api/system/profile';
import { useAuthStore } from '#/store';
import { getDictOptions } from '#/utils/dict';

import { emitter } from '../mitt';

const props = defineProps<{ profile: UserInfoResp }>();

const userStore = useUserStore();
const authStore = useAuthStore();
const profileBaseSettingRef = ref();

const formSchema = [
  {
    component: 'Input',
    dependencies: {
      show: () => false,
      triggerFields: [''],
    },
    fieldName: 'userId',
    label: '用户ID',
    rules: 'required',
  },
  {
    component: 'Input',
    fieldName: 'nick',
    label: '昵称',
    rules: 'required',
  },
  {
    component: 'Input',
    fieldName: 'email',
    label: '邮箱',
    rules: z.string().email('请输入正确的邮箱').optional().or(z.literal('')),
  },
  {
    component: 'RadioGroup',
    componentProps: {
      buttonStyle: 'solid',
      options: getDictOptions(DictEnum.SYS_USER_SEX),
      optionType: 'button',
    },
    defaultValue: '0',
    fieldName: 'sex',
    label: '性别',
    rules: 'required',
  },
  {
    component: 'Input',
    fieldName: 'phone',
    label: '电话',
    rules: z
      .union([
        z.literal(''),
        z.string().regex(/^1[3-9]\d{9}$/, '请输入正确的电话'),
      ])
      .optional(),
  },
];

async function handleSubmit(values: Recordable<any>) {
  try {
    const data = cloneDeep(values);
    if (data.phone) {
      data.phone = Number(data.phone);
    }
    await userProfileUpdate(data);
    const userInfo = await authStore.fetchUserInfo();
    userStore.setUserInfo(userInfo);
    emitter.emit('updateProfile');
  } catch (error) {
    console.error(error);
  }
}

onMounted(() => {
  const user = props.profile.user;
  const data = {
    userId: user.userId,
    nick: user.nick,
    email: user.email,
    phone: user.phone != null ? String(user.phone) : '',
    sex: user.sex,
  };
  profileBaseSettingRef.value?.getFormApi().setValues(data);
});
</script>

<template>
  <ProfileBaseSetting
    ref="profileBaseSettingRef"
    class="mt-[16px] md:w-full lg:w-1/2 2xl:w-2/5"
    :form-schema="formSchema"
    @submit="handleSubmit"
  />
</template>
