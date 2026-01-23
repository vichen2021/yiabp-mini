import type { Field } from './model';

import type { ID, IDS, PageQuery, PageResult } from '#/api/common';

import { requestClient } from '#/api/request';

enum Api {
  root = '/field',
  fieldType = '/field/type',
}

/**
 * 获取字段列表
 * @param params 请求参数
 * @returns 列表
 */
export function fieldList(params?: PageQuery) {
  return requestClient.get<PageResult<Field>>(Api.root, { params });
}

/**
 * 获取字段详情
 * @param fieldId 字段ID
 * @returns 字段信息
 */
export function fieldInfo(fieldId: ID) {
  return requestClient.get<Field>(`${Api.root}/${fieldId}`);
}

/**
 * 新增字段
 * @param data 数据
 * @returns void
 */
export function fieldAdd(data: Partial<Field>) {
  return requestClient.postWithMsg<void>(Api.root, data);
}

/**
 * 更新字段
 * @param data 数据
 * @returns void
 */
export function fieldUpdate(data: Partial<Field>) {
  return requestClient.putWithMsg<void>(`${Api.root}/${data.id}`, data);
}

/**
 * 删除字段
 * @param fieldIds 字段ID数组
 * @returns void
 */
export function fieldRemove(fieldIds: IDS) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: fieldIds.join(',') },
  });
}

/**
 * 获取字段类型枚举
 * @returns 字段类型列表（注意：后端返回的是 lable，需要映射为 label）
 */
export function getFieldTypeEnum() {
  return requestClient.get<Array<{ lable: string; value: number }>>(
    Api.fieldType,
  ).then((res) => {
    // 将后端的 lable 映射为 label（修复拼写错误）
    return res.map((item) => ({
      label: item.lable,
      value: item.value,
    }));
  });
}
