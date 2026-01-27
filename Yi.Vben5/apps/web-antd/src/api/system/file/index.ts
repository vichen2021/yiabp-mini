import type { AxiosRequestConfig } from '@vben/request';

import type { FileItem, OssFile } from './model';

import type { ID, IDS, PageQuery, PageResult } from '#/api/common';

import { ContentTypeEnum } from '#/api/helper';
import { requestClient } from '#/api/request';

enum Api {
  fileUpload = '/file/upload',
  root = '/file',
}

/**
 * 文件分页列表 - 对应 GetListAsync(FileGetListInputVo)
 * @param params SkipCount, MaxResultCount, fileName, startCreationTime, endCreationTime
 */
export function fileList(params?: PageQuery) {
  return requestClient.get<PageResult<FileItem>>(Api.root, { params });
}

/**
 * 文件详情 - 对应 GetAsync(id)
 * @param fileId 文件 id
 */
export function fileInfo(fileId: ID) {
  return requestClient.get<FileItem>(`${Api.root}/${fileId}`);
}

/**
 * 删除文件 - 对应 DeleteAsync(ids)
 * @param fileIds id 数组
 */
export function fileRemove(fileIds: IDS) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: fileIds.join(',') },
  });
}

/**
 * 上传文件 - 对应 UploadAsync(List<IFormFile> files)
 * @param files 文件列表
 */
export function fileUploadFiles(files: File[]) {
  const formData = new FormData();
  for (const file of files) {
    formData.append('files', file);
  }
  return requestClient.postWithMsg<void>(Api.fileUpload, formData, {
    headers: { 'Content-Type': ContentTypeEnum.FORM_DATA },
    timeout: 60 * 1000,
  });
}

/**
 * 下载文件 - 对应 DownloadAsync(Guid id)，返回 Blob，调用方配合 fileName 使用 downloadByData
 * @param fileId 文件 id
 * @param onDownloadProgress 下载进度回调
 */
export async function fileDownload(
  fileId: ID,
  onDownloadProgress?: AxiosRequestConfig['onDownloadProgress'],
): Promise<Blob> {
  const response = await requestClient.post<Blob>(
    `${Api.root}/${fileId}/download`,
    undefined,
    {
      isReturnNativeResponse: true,
      responseType: 'blob',
      timeout: 60 * 1000,
      onDownloadProgress,
    },
  );
  return response.data as unknown as Blob;
}

/**
 * 根据 id 批量查询文件信息（兼容原 ossInfo 调用方：上传 hook、审批时间线等）
 * 返回形状含 ossId、url、originalName，便于沿用现有展示逻辑
 */
export async function ossInfo(ids: ID | IDS): Promise<OssFile[]> {
  const idList = Array.isArray(ids) ? ids : [ids];
  const list = await Promise.all(idList.map((id) => fileInfo(id)));
  return list.map((item) => ({
    ...item,
    ossId: item.id,
    originalName: item.fileName,
    url: '', // File 管理无直链，预览/下载需走 fileDownload
  }));
}
