/**
 * 后端使用 .NET `Guid` 类型，根节点 / 空值统一用 `Guid.Empty` 表示。
 * 前端 TreeSelect/ApiTreeSelect 的"顶级"选项 value 通常为 `''`，提交前需要转成空 GUID，
 * 否则 ABP 模型绑定会报 `The input field is required.`。
 */
export const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

/**
 * 判断给定值是否为空 GUID（包含 `null`、`undefined`、空字符串、全零 GUID）。
 *
 * 常用场景：
 * - 树表把根节点的 `parentId` 置为 `null` 让 vxe-table 正确识别根节点
 * - 表单 `dependencies.show` 中根据是否顶级控制字段显隐
 */
export function isEmptyGuid(value: null | string | undefined): boolean {
  return !value || value === EMPTY_GUID;
}

/**
 * 把空值（`null`、`undefined`、空字符串）统一转换为空 GUID。
 * 用于表单提交前归一化 `parentId` 等字段，避免后端非空 `Guid` 绑定失败。
 */
export function toGuidOrEmpty(value: null | string | undefined): string {
  return value ? value : EMPTY_GUID;
}

/**
 * 把空 GUID（含空字符串/null/undefined）转为 `null`，便于 vxe-table
 * 树形 `transform: true` 识别根节点。
 */
export function emptyGuidToNull(
  value: null | string | undefined,
): null | string {
  return isEmptyGuid(value) ? null : (value as string);
}
