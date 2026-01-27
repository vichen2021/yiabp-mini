import type { FileConfig } from './model';

import type { ID, IDS, PageQuery } from '#/api/common';

import { requestClient } from '#/api/request';

enum Api {
  fileConfigChangeStatus = '/resource/oss/config/changeStatus',
  fileConfigList = '/resource/oss/config/list',
  root = '/resource/oss/config',
}

/**
 * 文件存储配置列表
 */
export function fileConfigList(params?: PageQuery) {
  return requestClient.get<FileConfig[]>(Api.fileConfigList, { params });
}

/**
 * 文件存储配置详情
 * @param ossConfigId 配置 id
 */
export function fileConfigInfo(ossConfigId: ID) {
  return requestClient.get<FileConfig>(`${Api.root}/${ossConfigId}`);
}

/**
 * 新增文件存储配置
 */
export function fileConfigAdd(data: Partial<FileConfig>) {
  return requestClient.postWithMsg<void>(Api.root, data);
}

/**
 * 更新文件存储配置
 */
export function fileConfigUpdate(data: Partial<FileConfig>) {
  return requestClient.putWithMsg<void>(`${Api.root}/${data.ossConfigId}`, data);
}

/**
 * 删除文件存储配置
 * @param ossConfigIds 配置 id 数组
 */
export function fileConfigRemove(ossConfigIds: IDS) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: ossConfigIds.join(',') },
  });
}

/**
 * 更改文件存储配置状态
 */
export function fileConfigChangeStatus(data: {
  ossConfigId: number;
  status: string;
  configKey: string;
}) {
  return requestClient.putWithMsg(Api.fileConfigChangeStatus, data);
}
