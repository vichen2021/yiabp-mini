import { requestClient } from '#/api/request';

export interface Temp {
  name: string;
  value: number;
}

export function visitList() {
  return requestClient.get<Temp[]>('/monitor/logininfor/visitsMap');
}

export function deviceInfoList() {
  return requestClient.get<Temp[]>('/monitor/logininfor/deviceInfoList');
}

export function browserInfoList() {
  return requestClient.get<Temp[]>('/monitor/logininfor/browserInfoList');
}

export function ispInfoList() {
  return requestClient.get<Temp[]>('/monitor/logininfor/ispInfoList');
}

export interface LoginLineResp {
  date: string[];
  fail: number[];
  success: number[];
}

export function loginLine() {
  return requestClient.get<LoginLineResp>('/monitor/logininfor/loginLine');
}
