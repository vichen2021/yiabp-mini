export interface OperationLog {
  id: string;
  title: string;
  operType: string;
  requestMethod: string;
  operUser: string;
  operIp: string;
  method: string;
  requestParam: string;
  isSuccess: boolean;
  errorMessage: null | string;
  executionDuration: number;
  creationTime: string;
}
