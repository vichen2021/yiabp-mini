export type ID = number | string;
export type IDS = (number | string)[];

export interface BaseEntity {
  createBy?: string;
  createDept?: string;
  createTime?: string;
  updateBy?: string;
  updateTime?: string;
}

/**
 * 分页信息
 * @param rows 结果集
 * @param total 总数
 */
export interface PageResult<T = any> {
  items: T[];
  totalCount: number;
}

/**
 * 分页查询参数
 *
 * 排序支持的用法如下:
 * {isAsc:"asc",orderByColumn:"id"} order by id asc
 * {isAsc:"asc",orderByColumn:"id,createTime"} order by id asc,create_time asc
 * {isAsc:"desc",orderByColumn:"id,createTime"} order by id desc,create_time desc
 * {isAsc:"asc,desc",orderByColumn:"id,createTime"} order by id asc,create_time desc
 *
 * @param SkipCount 当前页码。项目后端使用 SqlSugar 页码分页，这里不是 ABP offset。
 * @param MaxResultCount 每页大小
 * @param orderByColumn 排序字段
 * @param isAsc 是否升序
 */
export interface PageQuery {
  isAsc?: string;
  orderByColumn?: string;
  /** 当前页码，从 1 开始；不要传 (page - 1) * pageSize。 */
  SkipCount?: number;
  /** 每页大小。 */
  MaxResultCount?: number;
  [key: string]: any;
}
