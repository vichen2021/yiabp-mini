export interface Role {
  id: string;
  creationTime: string;
  creatorId?: string | null;
  lastModifierId?: string | null;
  lastModificationTime?: string | null;
  isDeleted?: boolean;
  orderNum: number;
  state: boolean;
  roleName: string;
  roleCode: string;
  remark?: string | null;
  dataScope: string;
  menus?: any[];
  depts?: any[];
}

export interface DeptOption {
  id: number;
  parentId: number;
  label: string;
  weight: number;
  children: DeptOption[];
  key: string; // 实际上不存在 ide报错
}

export interface DeptResp {
  checkedKeys: number[];
  depts: DeptOption[];
}
