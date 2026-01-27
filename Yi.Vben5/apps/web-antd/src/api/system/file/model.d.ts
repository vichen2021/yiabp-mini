/**
 * 文件项 - 对应后端 FileGetListOutputDto
 */
export interface FileItem {
  id: string;
  fileSize: number;
  beautifySize: string;
  contentType: string;
  fileName: string;
  creationTime: string;
}

/**
 * 兼容原 ossInfo 调用方（上传/审批等）的返回形状，便于复用
 */
export interface OssFile extends FileItem {
  ossId: string;
  url: string;
  originalName: string;
}
