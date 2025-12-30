/**
 * @description: 用户导入
 * @param updateSupport 是否覆盖数据
 * @param file excel文件
 */
export interface UserImportParam {
  updateSupport: boolean;
  file: Blob | File;
}

/**
 * @description: 重置密码
 */
export interface ResetPwdParam {
  id: string;
  password: string;
}

export interface Dept {
  deptId: string; // 改为string类型以匹配新的部门ID格式
  parentId: string;
  parentName?: string;
  ancestors: string;
  deptName: string;
  orderNum: number;
  leader: string;
  phone?: string;
  email?: string;
  status: string;
  createTime?: string;
}

export interface Role {
  roleId: string;
  roleName: string;
  roleKey: string;
  roleSort: number;
  dataScope: string;
  menuCheckStrictly?: boolean;
  deptCheckStrictly?: boolean;
  status: string;
  remark: string;
  createTime?: string;
  flag: boolean;
  superAdmin: boolean;
}

export interface User {
  id: string;
  name: string;
  age: number;
  userName: string;
  icon?: null | string;
  nick: string;
  email: string;
  ip?: null | string;
  address: string;
  phone: number;
  introduction: string;
  remark: string;
  sex: string; // 示例值: 'Male' | 'Female'
  deptId: string;
  creationTime: string; // ISO 时间字符串
  creatorId?: null | string;
  state: boolean;
  deptName?: null | string;
  posts?: Post[];
  roles?: Role[];
}

export interface Post {
  postId: number;
  postCode: string;
  postName: string;
  postSort: number;
  status: string;
  remark: string;
  createTime: string;
}

/**
 * @description 用户信息
 * @param user 用户个人信息
 * @param roleIds 角色IDS 不传id为空
 * @param roles 所有的角色
 * @param postIds 岗位IDS 不传id为空
 * @param posts 所有的岗位
 */
export interface UserInfoResponse {
  user?: User;
  roleIds?: string[];
  roles: Role[];
  postIds?: number[];
  posts?: Post[];
}

/**
 * @description: 部门输出DTO (对应C# DeptGetListOutputDto)
 */
export interface DeptGetListOutputDto {
  creationTime: string;
  creatorId?: string;
  state: boolean;
  deptName: string;
  deptCode: string;
  leader?: string;
  parentId: string;
  remark?: string;
  orderNum: number;
  /**
   * 子部门列表（用于树形结构）
   */
  children?: DeptGetListOutputDto[];
}

export interface DeptTreeData {
  id: number;
  label: string;
  children?: DeptTreeData[];
}
