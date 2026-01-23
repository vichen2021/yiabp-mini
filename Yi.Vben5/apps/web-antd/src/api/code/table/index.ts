import type { Table } from './model';

import type { ID, IDS, PageQuery, PageResult } from '#/api/common';

import { requestClient } from '#/api/request';

enum Api {
  root = '/table',
}

/**
 * 获取数据表列表
 * @param params 请求参数
 * @returns 列表
 */
export function tableList(params?: PageQuery) {
  return requestClient.get<PageResult<Table>>(Api.root, { params });
}

/**
 * 获取数据表详情
 * @param tableId 表ID
 * @returns 表信息
 */
export function tableInfo(tableId: ID) {
  return requestClient.get<Table>(`${Api.root}/${tableId}`);
}

/**
 * 新增数据表
 * @param data 数据
 * @returns void
 */
export function tableAdd(data: Partial<Table>) {
  return requestClient.postWithMsg<void>(Api.root, data);
}

/**
 * 更新数据表
 * @param data 数据
 * @returns void
 */
export function tableUpdate(data: Partial<Table>) {
  return requestClient.putWithMsg<void>(`${Api.root}/${data.id}`, data);
}

/**
 * 删除数据表
 * @param tableIds 表ID数组
 * @returns void
 */
export function tableRemove(tableIds: IDS) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: tableIds.join(',') },
  });
}
