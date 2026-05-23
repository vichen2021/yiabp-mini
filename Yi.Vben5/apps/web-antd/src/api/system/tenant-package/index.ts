import type { ID, IDS, PageResult } from '#/api/common';
import type { TenantPackage, TenantPackageListParams, TenantPackageCreateInput, TenantPackageUpdateInput } from './model';
import { requestClient } from '#/api/request';

enum Api {
  root = '/tenant-package',
}

/** 租户套餐分页列表 */
export function tenantPackageList(params?: TenantPackageListParams) {
  return requestClient.get<PageResult<TenantPackage>>(Api.root, { params });
}

/** 租户套餐详情 */
export function tenantPackageInfo(tenantPackageId: ID) {
  return requestClient.get<TenantPackage>(`${Api.root}/${tenantPackageId}`);
}

/** 租户套餐新增 */
export function tenantPackageAdd(data: TenantPackageCreateInput) {
  return requestClient.postWithMsg<void>(Api.root, data);
}

/** 租户套餐更新 */
export function tenantPackageUpdate(data: TenantPackageUpdateInput) {
  return requestClient.putWithMsg<void>(`${Api.root}/${data.id}`, data);
}

/** 租户套餐删除 */
export function tenantPackageRemove(tenantPackageIds: IDS) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: tenantPackageIds },
  });
}

/** 租户套餐下拉列表 */
export function tenantPackageSelectList(keywords?: string) {
  return requestClient.get<TenantPackage[]>(`${Api.root}/select-data-list`, {
    params: keywords ? { keywords } : undefined,
  });
}

/** 租户套餐导出 */
export function tenantPackageExport(data: Partial<TenantPackage>) {
  return requestClient.get<Blob>(`${Api.root}/export`, {
    params: data,
    responseType: 'blob',
  });
}
