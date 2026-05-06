/** 租户套餐实体接口 */
export interface TenantPackage {
  id: string;
  creationTime: string;
  creatorId?: string;
  /** 套餐名称 */
  packageName: string;
  /** 菜单树选择是否父子关联 */
  menuCheckStrictly: boolean;
  orderNum: number;
  state: boolean;
  remark?: string | null;
}

/** 租户套餐创建输入 */
export interface TenantPackageCreateInput {
  packageName: string;
  menuCheckStrictly: boolean;
  orderNum?: number;
  state?: boolean;
  remark?: string | null;
}

/** 租户套餐更新输入 */
export interface TenantPackageUpdateInput {
  id: string;
  packageName: string;
  menuCheckStrictly: boolean;
  orderNum?: number;
  state?: boolean;
  remark?: string | null;
}

/** 租户套餐列表查询参数 */
export interface TenantPackageListParams {
  packageName?: string;
  state?: boolean;
  startTime?: string;
  endTime?: string;
}
