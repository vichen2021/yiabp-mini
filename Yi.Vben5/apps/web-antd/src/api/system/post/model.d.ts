/**
 * @description: Post interface
 */
export interface Post {
  id: string;
  creationTime: string;
  state: boolean;
  postCode: string;
  postName: string;
  remark: null | string;
  orderNum: number;
  deptId: string;
  deptName?: string;
}
