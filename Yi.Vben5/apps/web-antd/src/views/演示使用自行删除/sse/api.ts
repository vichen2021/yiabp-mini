import { requestClient } from '#/api/request';

enum Api {
  list = '/system/sse/list',
  send = '/system/sse/send',
  sendAll = '/system/sse/sendAll',
  status = '/system/sse/status',
}

export function sseStatus() {
  return requestClient.get<boolean>(Api.status);
}

export function sseSendAll(message: string) {
  return requestClient.postWithMsg<void>(`${Api.sendAll}?message=${message}`);
}

export function sseSendByUserId(userId: string, message: string) {
  return requestClient.postWithMsg<void>(
    `${Api.send}/${userId}?message=${message}`,
  );
}

export function sseList() {
  return requestClient.get<any>(Api.list);
}
