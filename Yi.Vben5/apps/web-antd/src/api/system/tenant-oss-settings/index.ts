import type { TenantOssSetting } from './model';

import type { ID } from '#/api/common';

import { requestClient } from '#/api/request';

/**
 * 读取指定租户的 OSS 设置
 * AccessKeySecret 不回显
 */
export function tenantOssSettingGet(tenantId: ID) {
  return requestClient.get<TenantOssSetting>(
    `/file-management/tenants/${tenantId}/oss-settings`,
  );
}

/**
 * 更新指定租户的 OSS 设置
 * AccessKeySecret 为空时不覆盖原值
 */
export function tenantOssSettingUpdate(tenantId: ID, data: TenantOssSetting) {
  return requestClient.putWithMsg<void>(
    `/file-management/tenants/${tenantId}/oss-settings`,
    data,
  );
}
