# Frontend API Templates

## ⚠️ 重要：参考项目实际代码模式

模板基于 `src/views/product/item/` 的实际代码，而非假设的 Vben5 模式。

---

## Template Data Structure

从实体解析的数据结构：

```json
{
  "entityName": "Item",
  "entityNameLower": "item",
  "moduleName": "product",
  "moduleNamespace": "Product",
  "isTree": false,
  "hasEnums": true,
  "enumTypes": ["ItemTypeEnum"],
  "dictConstants": ["PRODUCT_ITEM_TYPE"],
  "fields": [
    {"name": "itemName", "label": "物品名称", "tsType": "string", "required": true, "isIndex": true},
    {"name": "itemType", "label": "物品类型", "tsType": "number", "isEnum": true, "dictConstant": "PRODUCT_ITEM_TYPE", "required": true}
  ]
}
```

---

## model.d.ts Template

```typescript
/**
 * {{entityComment}}实体接口
 */
export interface {{entityName}} {
  /** ID */
  id: string;
  /** 创建时间 */
  creationTime: string;
  /** 创建者ID */
  creatorId?: string;
  {{#each fields}}
  /** {{label}} */
  {{name}}: {{#if nullable}}{{tsType}} | null{{else}}{{tsType}}{{/if}};
  {{/each}}
  /** 排序 */
  orderNum: number;
  /** 状态 */
  state: boolean;
  /** 备注 */
  remark?: string | null;
  {{#if isTree}}
  /** 父级ID */
  parentId: string;
  /** 子节点列表 */
  children?: {{entityName}}[];
  {{/if}}
}

/**
 * {{entityComment}}列表查询参数
 */
export interface {{entityName}}ListParams {
  {{#each fields}}
  /** {{label}} */
  {{name}}?: {{tsType}};
  {{/each}}
  /** 状态 */
  state?: boolean;
  {{#unless isTree}}
  /** 开始时间 */
  startTime?: string;
  /** 结束时间 */
  endTime?: string;
  {{/unless}}
}

/**
 * {{entityComment}}创建输入
 */
export interface {{entityName}}CreateInput {
  {{#each fields}}
  /** {{label}} */
  {{name}}: {{#if nullable}}{{tsType}} | null{{else}}{{tsType}}{{/if}};
  {{/each}}
  /** 排序 */
  orderNum?: number;
  /** 状态 */
  state?: boolean;
  /** 备注 */
  remark?: string | null;
}

/**
 * {{entityComment}}更新输入
 */
export interface {{entityName}}UpdateInput {
  /** ID */
  id: string;
  {{#each fields}}
  /** {{label}} */
  {{name}}: {{#if nullable}}{{tsType}} | null{{else}}{{tsType}}{{/if}};
  {{/each}}
  /** 排序 */
  orderNum?: number;
  /** 状态 */
  state?: boolean;
  /** 备注 */
  remark?: string | null;
}
```

---

## index.ts Template (普通实体 - 分页)

⚠️ **关键：使用项目已有的 API 方法模式**

```typescript
import type { ID, IDS, PageResult } from '#/api/common';

import type {
  {{entityName}},
  {{entityName}}ListParams,
  {{entityName}}CreateInput,
  {{entityName}}UpdateInput,
} from './model';

import { requestClient } from '#/api/request';

enum Api {
  root = '/api/{{moduleName}}/{{entityNameLower}}',
}

/**
 * {{entityComment}}分页列表
 * @param params 请求参数
 * @returns 列表
 */
export function {{entityNameLower}}List(params?: {{entityName}}ListParams) {
  return requestClient.get<PageResult<{{entityName}}>>(Api.root, { params });
}

/**
 * {{entityComment}}详情
 * @param {{entityNameLower}}Id {{entityComment}}id
 * @returns {{entityComment}}信息
 */
export function {{entityNameLower}}Info({{entityNameLower}}Id: ID) {
  return requestClient.get<{{entityName}}>(`${Api.root}/${{{entityNameLower}}Id}`);
}

/**
 * {{entityComment}}新增
 * @param data 参数
 */
export function {{entityNameLower}}Add(data: {{entityName}}CreateInput) {
  return requestClient.postWithMsg<void>(Api.root, data);
}

/**
 * {{entityComment}}更新
 * @param data 参数
 */
export function {{entityNameLower}}Update(data: {{entityName}}UpdateInput) {
  return requestClient.putWithMsg<void>(`${Api.root}/${data.id}`, data);
}

/**
 * {{entityComment}}删除
 * @param {{entityNameLower}}Ids ids
 */
export function {{entityNameLower}}Remove({{entityNameLower}}Ids: IDS) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: {{entityNameLower}}Ids.join(',') },
  });
}
```

---

## index.ts Template (树形实体 - 无分页)

```typescript
import type { ID, IDS } from '#/api/common';

import type {
  {{entityName}},
  {{entityName}}ListParams,
  {{entityName}}CreateInput,
  {{entityName}}UpdateInput,
} from './model';

import { requestClient } from '#/api/request';

enum Api {
  root = '/api/{{moduleName}}/{{entityNameLower}}',
  tree = '/api/{{moduleName}}/{{entityNameLower}}/tree',
  listExclude = '/api/{{moduleName}}/{{entityNameLower}}/list/exclude',
}

const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';

/**
 * {{entityComment}}列表（树形，无分页）
 * @param params 请求参数
 * @returns 列表
 */
export function {{entityNameLower}}List(params?: {{entityName}}ListParams) {
  return requestClient.get<{{entityName}}[]>(`${Api.root}/list`, { params });
}

/**
 * {{entityComment}}树形数据
 * @returns 树形列表
 */
export function {{entityNameLower}}Tree() {
  return requestClient.get<{{entityName}}[]>(Api.tree);
}

/**
 * {{entityComment}}节点列表（排除指定节点及其子节点）
 * @param id 排除的节点ID
 * @returns 列表
 */
export function {{entityNameLower}}NodeList(id: ID) {
  return requestClient.get<{{entityName}}[]>(`${Api.listExclude}/${id}`);
}

/**
 * {{entityComment}}详情
 * @param {{entityNameLower}}Id {{entityComment}}id
 * @returns {{entityComment}}信息
 */
export function {{entityNameLower}}Info({{entityNameLower}}Id: ID) {
  return requestClient.get<{{entityName}}>(`${Api.root}/${{{entityNameLower}}Id}`);
}

/**
 * {{entityComment}}新增
 * @param data 参数
 */
export function {{entityNameLower}}Add(data: {{entityName}}CreateInput) {
  const submitData = { ...data };
  if (submitData.parentId === EMPTY_GUID || submitData.parentId === '') {
    submitData.parentId = EMPTY_GUID;
  }
  return requestClient.postWithMsg<void>(Api.root, submitData);
}

/**
 * {{entityComment}}更新
 * @param data 参数
 */
export function {{entityNameLower}}Update(data: {{entityName}}UpdateInput) {
  return requestClient.putWithMsg<void>(`${Api.root}/${data.id}`, data);
}

/**
 * {{entityComment}}删除
 * @param {{entityNameLower}}Ids ids
 */
export function {{entityNameLower}}Remove({{entityNameLower}}Ids: IDS) {
  return requestClient.deleteWithMsg<void>(Api.root, {
    params: { ids: {{entityNameLower}}Ids.join(',') },
  });
}
```

---

## TypeScript 类型映射

| C# 类型 | TypeScript 类型 |
|---------|-----------------|
| `string` | `string` |
| `int` | `number` |
| `long` | `number` |
| `bool` | `boolean` |
| `Guid` | `string` |
| `DateTime` | `string` |
| `ItemTypeEnum` (枚举) | `number` |
| `string?` (可空) | `string | null` |

---

## 字段命名转换

| C# 字段名 | TypeScript 字段名 | 说明 |
|-----------|-------------------|------|
| `MaterialName` | `materialName` | camelCase |
| `MaterialType` | `materialType` | camelCase |

⚠️ 前端使用 camelCase，后端返回 PascalCase，Vben5 会自动转换。

---

## API 方法对照

| 模板方法 | 项目实际方法 | 说明 |
|----------|--------------|------|
| `requestClient.post(url, data, { successMessageMode: 'message' })` | `requestClient.postWithMsg(url, data)` | 创建 |
| `requestClient.put(url, data, { successMessageMode: 'message' })` | `requestClient.putWithMsg(url, data)` | 更新 |
| `requestClient.delete(url, ids, { successMessageMode: 'message' })` | `requestClient.deleteWithMsg(url, { params: { ids: ids.join(',') } })` | 删除 |

---

## 类型导入

```typescript
// 必须导入的类型
import type { ID, IDS, PageResult } from '#/api/common';
```

- `ID` - 单个 ID 类型（string | number）
- `IDS` - ID 数组类型
- `PageResult` - 分页结果类型