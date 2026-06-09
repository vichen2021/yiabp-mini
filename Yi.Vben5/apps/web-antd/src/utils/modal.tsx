import type { Component } from 'vue';

import { alert, confirm, prompt } from '@vben/common-ui';
import { $t } from '@vben/locales';

import { message } from 'antdv-next';
import { isFunction, isString } from 'lodash-es';

export interface ConfirmModalProps {
  confirmText?: string;
  content?: string;
  onValidated?: () => Promise<void>;
  placeholder?: string;
  title?: string;
}

export interface ConfirmDangerActionProps {
  content: string;
  onConfirmed?: () => Promise<void> | void;
  title?: string;
}

export async function confirmDangerAction(props: ConfirmDangerActionProps) {
  try {
    await confirm({
      cancelText: $t('common.cancel'),
      centered: true,
      confirmText: $t('common.confirm'),
      content: props.content,
      icon: 'warning',
      title: props.title || $t('pages.common.tip'),
    });
    if (isFunction(props.onConfirmed)) {
      await props.onConfirmed();
    }
    return true;
  } catch {
    return false;
  }
}

type AlertContent = Component | string;

export function showErrorAlert(
  content: AlertContent,
  title = $t('pages.common.tip'),
) {
  alert({
    content,
    icon: 'error',
    title,
  }).catch(() => {});
}

export function showSuccessAlert(
  content: AlertContent,
  title = $t('pages.common.tip'),
) {
  alert({
    content,
    icon: 'success',
    title,
  }).catch(() => {});
}

export function confirmDeleteModal(props: ConfirmModalProps) {
  const placeholder = props.placeholder || `输入'确认删除'`;
  const confirmText = props.confirmText || '确认删除';
  const content = isString(props.content)
    ? props.content
    : '确认删除后将无法恢复，请谨慎操作！';

  prompt<string>({
    beforeClose: async ({ isConfirm, value }) => {
      if (!isConfirm) {
        return;
      }
      if (value !== confirmText) {
        message.error('校验不通过');
        return false;
      }
      if (isFunction(props.onValidated)) {
        await props.onValidated();
      }
    },
    centered: true,
    componentProps: {
      placeholder,
    },
    content,
    defaultValue: '',
    icon: 'warning',
    title: props.title || '提示',
  }).catch(() => {});
}
