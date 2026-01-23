import type { Template } from './model';

import type { ID, IDS, PageQuery, PageResult } from '#/api/common';

import { requestClient } from '#/api/request';

enum Api {
  root = '/template',
}

/**
 * 获取模板列表
 * @param params 请求参数
 * @returns 列表
 */
export function templateList(params?: PageQuery) {
  return requestClient.get<PageResult<Template>>(Api.root, { params });
}

/**
 * 获取模板详情
 * @param templateId 模板ID
 * @returns 模板信息
 */
export function templateInfo(templateId: ID) {
  return requestClient.get<Template>(`${Api.root}/${templateId}`);
}

/**
 * 新增模板
 * @param data 数据
 * @returns void
 */
export function templateAdd(data: Partial<Template>) {
  return requestClient.postWithMsg<void>(Api.root, data);
}

/**
 * 更新模板
 * @param data 数据
 * @returns void
 */
export function templateUpdate(data: Partial<Template>) {
  return requestClient.putWithMsg<void>(`${Api.root}/${data.id}`, data);
}

/**
 * 删除模板
 * @param templateIds 模板ID数组
 * @returns void
 */
export function templateRemove(templateIds: IDS) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: templateIds.join(',') },
  });
}
