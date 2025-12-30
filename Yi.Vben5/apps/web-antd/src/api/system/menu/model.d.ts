export interface Menu {
  id: string;
  isDeleted: boolean;
  creationTime: string;
  creatorId: string | null;
  lastModifierId: string | null;
  lastModificationTime: string | null;
  orderNum: number;
  state: boolean;
  menuName: string;
  routerName?: string | null;
  menuType: string;
  permissionCode?: string | null;
  parentId: string;
  menuIcon?: string | null;
  router?: string | null;
  isLink: boolean;
  isCache: boolean;
  isShow: boolean;
  remark?: string | null;
  component?: string | null;
  query?: string | null;
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
  isShow?: boolean;
  state?: boolean;
}
