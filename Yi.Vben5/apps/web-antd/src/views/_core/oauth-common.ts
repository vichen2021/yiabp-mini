import type { Component, CSSProperties } from 'vue';

import { markRaw } from 'vue';

import {
  GiteeIcon,
  GithubOAuthIcon,
  SvgMaxKeyIcon,
  SvgTopiamIcon,
  SvgWechatIcon,
} from '@vben/icons';

import { authBinding } from '#/api/core/auth';
import { useLoginTenantId } from '#/utils/tenant';

export { useLoginTenantId };

/**
 * @description: oauth登录
 * @param title 标题
 * @param description 描述
 * @param avatar 图标
 * @param color 图标颜色可直接写英文颜色/hex
 */
export interface ListItem {
  title: string;
  description: string;
  avatar?: Component;
  style?: CSSProperties;
}

/**
 * @description: 绑定账号
 * @param source 来源 如gitee github 与后端的social-callback?source=xxx对应
 * @param bound 是否已经绑定
 */
export interface BindItem extends ListItem {
  source: string;
  bound?: boolean;
}

/**
 * 绑定授权
 * @param source
 */
export async function handleAuthBinding(source: string) {
  const { loginTenantId } = useLoginTenantId();
  // 这里返回打开授权页面的链接
  const href = await authBinding(source, loginTenantId.value);
  window.location.href = href;
}

/**
 * 账号绑定 list
 * 添加账号绑定只需要在这里增加即可
 */
export const accountBindList: BindItem[] = [
  {
    avatar: markRaw(GiteeIcon),
    description: '绑定Gitee账号',
    source: 'gitee',
    title: 'Gitee',
    style: { color: '#c71d23' },
  },
  {
    avatar: markRaw(GithubOAuthIcon),
    description: '绑定Github账号',
    source: 'github',
    title: 'Github',
  },
  {
    avatar: markRaw(SvgMaxKeyIcon),
    description: '绑定MaxKey账号',
    source: 'maxkey',
    title: 'MaxKey',
  },
  {
    avatar: markRaw(SvgTopiamIcon),
    description: '绑定topiam账号',
    source: 'topiam',
    title: 'Topiam',
  },
  {
    avatar: markRaw(SvgWechatIcon),
    description: '绑定wechat账号',
    source: 'wechat',
    title: 'Wechat',
  },
];
