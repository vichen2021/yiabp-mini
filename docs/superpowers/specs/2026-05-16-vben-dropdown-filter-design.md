# Vben 管理端下拉筛选重构设计

## 背景

当前管理端多个基础模块页面都存在 `Select`、`TreeSelect`、业务选择器和实体下拉数据源写法不一致的问题。部分下拉没有启用搜索，部分树形下拉筛选字段分散在页面中，实体下拉数据源也存在 `list`、`tree`、`select-data-list` 混用情况。

本设计用于约束 `master` 基础分支中的下拉筛选重构，先处理基础能力，再由 CMS 分支合并 master 后继续处理 CMS 产品模块。

## 分支推进原则

本任务必须遵循分支边界：

1. 先在 `master` 基础分支处理基础模块和通用前端能力。
2. `master` 基础模块处理完成后提交。
3. CMS 分支合并 `master`。
4. 合并完成后，再处理 CMS 产品模块。

当前 `.master-branch` 只处理基础能力，不处理 CMS 产品模块。

## master 阶段范围

本阶段只覆盖 `master` 基础模块相关页面和通用规则：

- RBAC / system 基础能力：用户、角色、菜单、部门、岗位、客户端等。
- tenant-management：租户、租户套餐等。
- setting-management：参数配置、字典、通知等基础配置页面。
- file-management：文件、文件配置等。
- audit-logging / monitor：登录日志、操作日志等审计相关页面。

不处理：

- `workflow`。
- `demo`。
- `dashboard`。
- `article`、`member`、`app-management` 等 CMS 产品模块。
- 具体 `wj-cms/*` 产品分支差异能力。

## 核心原则

### 1. 原则上使用实体类自带的 select-data-list 接口

实体下拉数据源原则上使用实体类基类提供的 `select-data-list` 接口：

```text
GET ${Api.root}/select-data-list
```

前端 API 封装应按实体提供明确方法，例如：

```ts
export function deptSelectList(params?: { keywords?: string }) {
  return requestClient.get<Dept[]>(Api.deptSelectList, { params });
}
```

适用场景：

- 表单中选择实体。
- 过滤条件中选择实体。
- 业务抽屉中加载实体下拉候选项。
- 树形实体下拉获取扁平候选数据后前端转树。

例外场景：

- 权限树、角色菜单树、租户套餐菜单树等具有业务勾选状态、排除规则或权限裁剪的数据，可继续使用专用接口。
- 编辑树形父级时需要排除当前节点及子孙的接口，可继续使用已有专用排除接口。
- 后端接口确实没有 `select-data-list` 或返回结构不能满足业务时，应先确认后端契约，不在前端发明路径。

### 2. 前端负责下拉展示和筛选体验

后端 `select-data-list` 负责提供下拉候选数据。前端负责：

- 设置 `showSearch`。
- 设置筛选字段。
- 必要时将扁平数据转树。
- 处理显示标签和完整路径标签。
- 处理空 Guid 与前端空值转换。

### 3. 树形数据默认扁平返回，前端转树

除特殊业务树接口外，树形实体下拉优先使用实体 `select-data-list` 返回的扁平列表，然后前端使用 `listToTree` 转换。

转换前需要处理根节点父级值：

```ts
parentId: emptyGuidToNull(item.parentId)
```

用于让前端树组件正确识别根节点。

## Select 筛选规则

普通 `Select` 如果数据来自本地 `options` 或已加载实体下拉，应统一启用按标签搜索：

```ts
{
  showSearch: true,
  optionFilterProp: 'label',
  optionLabelProp: 'label',
  filterOption: true,
  getPopupContainer,
}
```

适用对象：

- 字典下拉。
- 固定枚举下拉。
- 已加载的实体候选项下拉。

不强制适用对象：

- 只有两三个选项且业务明确不需要搜索的下拉。
- 禁用态展示字段。
- 特殊远程搜索组件。

## TreeSelect 筛选规则

树形下拉应统一启用搜索，并明确按实际节点名称字段筛选：

```ts
{
  showSearch: true,
  treeNodeFilterProp: '实际节点名称字段',
  treeNodeLabelProp: 'fullName',
  getPopupContainer,
}
```

常见字段：

| 场景 | 筛选字段 |
|------|----------|
| 部门树 | `deptName` |
| 菜单树 | `menuName` |
| label/value 树 | `label` |

如果使用 `addFullName(treeData, nameField, ' / ')` 生成完整路径显示，则选中后显示字段使用：

```ts
treeNodeLabelProp: 'fullName'
```

## 空 Guid 规则

后端用空 Guid 表示顶级/无父级时，前端必须统一处理：

- 编辑展示前：`emptyGuidToNull(parentId)`。
- 表格转树前：`emptyGuidToNull(parentId)`。
- 提交保存前：`toGuidOrEmpty(parentId)`。

禁止为了匹配空 Guid 在下拉树中额外添加“顶级”假节点，除非业务数据本身真实存在该节点。

## 不做的事情

本阶段不做以下事项：

- 不修改后端接口契约。
- 不引入新依赖。
- 不全局修改 Vben `Select` / `TreeSelect` adapter 默认行为。
- 不处理 CMS 产品模块。
- 不处理 workflow/demo/dashboard。
- 不做无关格式化。

## 推荐实现方式

采用“页面 schema 统一 + 小范围工具函数辅助”的方式。

如确实存在大量重复配置，可在前端合适位置增加轻量工具函数，但必须保持业务语义清晰，不能把一次性业务逻辑塞进无边界公共工具。

示例方向：

```ts
selectSearchProps()

treeSelectSearchProps({ filterProp: 'deptName' })
```

工具函数只承载通用 UI 配置，不承载实体业务规则。

## 验收标准

- master 基础模块中的主要 `Select` 可按 label 搜索。
- master 基础模块中的主要 `TreeSelect` 可按节点名称搜索。
- 实体下拉原则上使用 `select-data-list` 数据源。
- 树形实体下拉能正确处理空 Guid 根节点。
- 编辑树形父级时不显示裸空 Guid。
- 不混入数据库、lockfile、无关配置等文件。
- 提交信息使用中文并带 `fix:` 前缀。
