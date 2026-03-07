<script lang="ts" setup>
import type { LoginAndRegisterParams, VbenFormSchema } from '@vben/common-ui';

import type { TenantResp } from '#/api/core/auth';
import type { CaptchaResponse } from '#/api/core/captcha';

import { computed, onMounted, ref, useTemplateRef } from 'vue';

import { AuthenticationLogin, z } from '@vben/common-ui';
import { DEFAULT_TENANT_ID } from '@vben/constants';
import { $t } from '@vben/locales';

import { omit } from 'lodash-es';

import { tenantList } from '#/api/core/auth';
import { captchaImage } from '#/api/core/captcha';
import { useAuthStore } from '#/store';

import { useLoginTenantId } from '../oauth-common';
import OAuthLogin from './oauth-login.vue';

defineOptions({ name: 'Login' });

const authStore = useAuthStore();

const loginFormRef = useTemplateRef('loginFormRef');

const captchaInfo = ref<CaptchaResponse>({
  isEnableCaptcha: false,
  img: '',
  uuid: '',
});
// 验证码loading
const captchaLoading = ref(false);

async function loadCaptcha() {
  try {
    captchaLoading.value = true;

    const resp = await captchaImage();
    if (resp.isEnableCaptcha) {
      resp.img = `data:image/png;base64,${resp.img}`;
    }
    captchaInfo.value = resp;
  } catch (error) {
    console.error(error);
  } finally {
    captchaLoading.value = false;
  }
}

const tenantInfo = ref<TenantResp[]>([]);
const hasTenants = ref(false);

async function loadTenant() {
  const resp = await tenantList();
  hasTenants.value = resp.length > 0;

  // 只有当有租户数据时才添加默认租户选项
  if (hasTenants.value) {
    // 在租户列表前添加"主租户"选项
    tenantInfo.value = [
      {
        id: DEFAULT_TENANT_ID,
        name: '默认租户',
      } as TenantResp,
      ...resp,
    ];
    // 默认选中主租户
    loginFormRef.value?.getFormApi().setFieldValue('tenantId', DEFAULT_TENANT_ID);
  }
}

onMounted(async () => {
  await Promise.all([loadCaptcha(), loadTenant()]);
});

const { loginTenantId } = useLoginTenantId();

const formSchema = computed((): VbenFormSchema[] => {
  return [
    {
      component: 'VbenSelect',
      componentProps: {
        class: 'bg-background h-[40px] focus:border-primary',
        contentClass: 'max-h-[256px] overflow-y-auto',
        options: tenantInfo.value?.map((item) => ({
          label: item.name,
          value: item.id,
        })),
        placeholder: $t('authentication.selectAccount'),
      },
      defaultValue: DEFAULT_TENANT_ID,
      dependencies: {
        if: () => hasTenants.value,
        // 可以把这里当做watch
        trigger: (model) => {
          // 给oauth登录使用
          loginTenantId.value = model?.tenantId ?? DEFAULT_TENANT_ID;
        },
        triggerFields: ['', 'tenantId'],
      },
      fieldName: 'tenantId',
      label: $t('authentication.selectAccount'),
      rules: z.string().min(1, { message: $t('authentication.selectAccount') }),
    },
    {
      component: 'VbenInput',
      componentProps: {
        class: 'focus:border-primary',
        placeholder: $t('authentication.usernameTip'),
      },
      defaultValue: 'cc',
      fieldName: 'username',
      label: $t('authentication.username'),
      rules: z.string().min(1, { message: $t('authentication.usernameTip') }),
    },
    {
      component: 'VbenInputPassword',
      componentProps: {
        class: 'focus:border-primary',
        placeholder: $t('authentication.password'),
      },
      defaultValue: '123456',
      fieldName: 'password',
      label: $t('authentication.password'),
      rules: z.string().min(5, { message: $t('authentication.passwordTip') }),
    },
    {
      component: 'VbenInputCaptcha',
      componentProps: {
        captcha: captchaInfo.value.img,
        class: 'focus:border-primary',
        onCaptchaClick: loadCaptcha,
        placeholder: $t('authentication.code'),
        loading: captchaLoading.value,
      },
      dependencies: {
        if: () => captchaInfo.value.isEnableCaptcha,
        triggerFields: [''],
      },
      fieldName: 'code',
      label: $t('authentication.code'),
      rules: z
        .string()
        .min(1, { message: $t('authentication.verifyRequiredTip') }),
    },
  ];
});

async function handleAccountLogin(values: LoginAndRegisterParams) {
  try {
    const requestParam: any = omit(values, ['code']);
    // 验证码
    if (captchaInfo.value.isEnableCaptcha) {
      requestParam.code = values.code;
      requestParam.uuid = captchaInfo.value.uuid;
    }
    // 登录
    await authStore.authLogin(requestParam);
  } catch (error) {
    console.error(error);
    // 处理验证码错误
    if (error instanceof Error) {
      // 刷新验证码
      loginFormRef.value?.getFormApi().setFieldValue('code', '');
      await loadCaptcha();
    }
  }
}
</script>

<template>
  <AuthenticationLogin
    ref="loginFormRef"
    :form-schema="formSchema"
    :loading="authStore.loginLoading"
    :show-register="false"
    :show-third-party-login="true"
    @submit="handleAccountLogin"
  >
    <!-- 可通过show-third-party-login控制是否显示第三方登录 -->
    <template #third-party-login>
      <OAuthLogin />
    </template>
  </AuthenticationLogin>
</template>
