import type { AxiosRequestConfig } from '@vben/request';

import type { FileItem, OssFile } from './model';

import type { ID, IDS, PageQuery, PageResult } from '#/api/common';

import { ContentTypeEnum } from '#/api/helper';
import { requestClient } from '#/api/request';

enum Api {
  fileUpload = '/file/upload',
  fileBatchUpload = '/file/batch-upload',
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
 * 上传单个文件 - 对应 UploadAsync(IFormFile file)
 * @param file 文件
 * @returns 文件访问完整 URL
 */
export function fileUpload(file: File, config?: AxiosRequestConfig) {
  const formData = new FormData();
  formData.append('file', file);
  return requestClient.post<string>(Api.fileUpload, formData, {
    headers: { 'Content-Type': ContentTypeEnum.FORM_DATA },
    timeout: 60 * 1000,
    ...config,
  });
}

/**
 * 批量上传文件 - 对应 BatchUploadAsync(List<IFormFile> files)
 * @param files 文件列表
 * @returns 文件 id 列表
 */
export function fileBatchUpload(files: File[]) {
  const formData = new FormData();
  for (const file of files) {
    formData.append('files', file);
  }
  return requestClient.postWithMsg<string[]>(Api.fileBatchUpload, formData, {
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
  return requestClient.post<Blob>(
    `${Api.root}/${fileId}/download`,
    undefined,
    {
      responseType: 'blob',
      timeout: 60 * 1000,
      onDownloadProgress,
    },
  );
}

/**
 * 获取文件访问 URL
 * @param fileId 文件 id
 * @returns 文件访问 URL（相对路径）
 */
export function getFileUrl(fileId: ID): string {
  return `/api/file/get/${fileId}`;
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
    url: getFileUrl(item.id),
  }));
}
