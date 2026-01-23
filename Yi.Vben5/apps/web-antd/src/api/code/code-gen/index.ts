import type { ID } from '#/api/common';

import { requestClient } from '#/api/request';

enum Api {
  root = '/code-gen',
  webBuildCode = '/code-gen/web-build-code',
  codeBuildWeb = '/code-gen/code-build-web',
  openDir = '/code-gen/dir',
}

/**
 * Web To Code - 从Web配置生成代码
 * @param ids 表ID数组
 * @returns void
 */
export function postWebBuildCode(ids: ID[]) {
  return requestClient.postWithMsg<void>(Api.webBuildCode, ids);
}

/**
 * Code To Web - 从代码反向生成表结构
 * @returns void
 */
export function postCodeBuildWeb() {
  return requestClient.postWithMsg<void>(Api.codeBuildWeb);
}

/**
 * 打开目录
 * @param path 目录路径
 * @returns void
 */
export function postOpenDir(path: string) {
  // 将路径中的反斜杠替换为正斜杠，并编码
  const encodedPath = encodeURIComponent(path.replace(/\\/g, '/'));
  return requestClient.postWithMsg<void>(`${Api.openDir}/${encodedPath}`);
}
