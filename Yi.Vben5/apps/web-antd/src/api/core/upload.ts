import type { AxiosRequestConfig } from '@vben/request';

import { ContentTypeEnum } from '#/api/helper';
import { requestClient } from '#/api/request';

/**
 * Axios上传进度事件
 */
export type AxiosProgressEvent = AxiosRequestConfig['onUploadProgress'];

/**
 * 默认上传结果
 */
export interface UploadResult {
  url: string;
  fileName: string;
  ossId: string;
}

/**
 * 通过单文件上传接口（对接文件管理 /file/upload，返回 ossId 等供上传组件绑定）
 * @param file 上传的文件
 * @param options 一些配置项
 * @param options.onUploadProgress 上传进度事件
 * @param options.signal 上传取消信号
 * @param options.otherData 其他请求参数 后端拓展可能会用到
 * @returns 上传结果 { ossId, url, fileName }
 */
export function uploadApi(
  file: Blob | File,
  options?: {
    onUploadProgress?: AxiosProgressEvent;
    otherData?: Record<string, any>;
    signal?: AbortSignal;
  },
): Promise<UploadResult> {
  const { onUploadProgress, signal, otherData = {} } = options ?? {};
  const fileName = file instanceof File ? file.name : '';
  const formData = new FormData();
  formData.append('files', file);
  Object.entries(otherData).forEach(([k, v]) => {
    if (v != null) formData.append(k, v);
  });
  return requestClient
    .post<string[] | { data?: string[] }>('/file/upload', formData, {
      headers: { 'Content-Type': ContentTypeEnum.FORM_DATA },
      onUploadProgress,
      signal,
      timeout: 60_000,
    })
    .then((res) => {
      const ids = Array.isArray(res) ? res : (res?.data ?? []);
      const firstId = typeof ids[0] === 'string' ? ids[0] : String(ids[0] ?? '');
      return {
        ossId: firstId,
        url: '', // 文件管理无直链，预览/下载走 fileDownload
        fileName,
      };
    });
}

/**
 * 上传api type
 */
export type UploadApi = typeof uploadApi;
