export interface Menu {
  id?: string | number;
  createBy?: string | null;
  createTime?: string;
  creationTime?: string;
  updateBy?: string | null;
  updateTime?: string | null;
  remark?: string | null;
  menuId?: string | number;
  parentId?: string | number | null;
  parentName?: string;
  menuName: string;
  menuType: string;
  orderNum?: number;
  path?: string;
  router?: string;
  routerName?: string;
  component?: string;
  query?: string | null;
  permissionCode?: string;
  perms?: string;
  icon?: string;
  menuIcon?: string;
  isFrame?: string;
  isLink?: boolean;
  isCache?: boolean | string;
  visible?: string;
  status?: string;
  state?: boolean;
  isShow?: boolean;
  children?: Menu[];
}

/**
 * @description 菜单信息
 * @param label 菜单名称
 */
export interface MenuOption {
  id: number;
  parentId: number;
  label: string;
  weight: number;
  children: MenuOption[];
  key: string; // 实际上不存在 ide报错
  menuType: string;
  icon: string;
}

/**
 * @description 菜单返回
 * @param checkedKeys 选中的菜单id
 * @param menus 菜单信息
 */
export interface MenuResp {
  checkedKeys: number[];
  menus: MenuOption[];
}

/**
 * 菜单表单查询
 */
export interface MenuQuery {
  menuName?: string;
  visible?: string;
  status?: string;
}
