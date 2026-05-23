import type { TenantOssSetting } from './model';

import { requestClient } from '#/api/request';

/**
 * 读取当前租户的 OSS 存储设置
 * AccessKeySecret 不回显
 */
export function ossSettingGet() {
  return requestClient.get<TenantOssSetting>('/file-management/oss-settings');
}

/**
 * 更新当前租户的 OSS 存储设置
 * AccessKeySecret 为空时不覆盖原值
 */
export function ossSettingUpdate(data: TenantOssSetting) {
  return requestClient.putWithMsg<void>('/file-management/oss-settings', data);
}
