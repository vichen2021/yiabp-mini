# 表格组件

基于 VXE Table 的 Grid 配置式表格封装。

## 2.1 / Vben 5.7 约定

- 分页列表统一通过 `#/adapter/vxe-table` 使用 `useVbenVxeGrid`。
- 操作列统一使用 `VbenTableAction`，保持文字按钮交互，不再统一改成图标按钮。
- 删除确认使用 `actions[].popConfirm`，由公共操作列组件渲染确认气泡。
- 操作列必须设置稳定宽度，避免初次渲染时按钮被覆盖，或只有窗口尺寸变化后才恢复。
- 新页面不要手写 `Popconfirm + Space + Button` 作为表格操作列。

## 基本使用

```typescript
import { useVbenVxeGrid } from '#/adapter/vxe-table';

const [BasicTable, tableApi] = useVbenVxeGrid({
  formOptions,   // 查询表单配置
  gridOptions,   // 表格配置
  gridEvents,    // 表格事件
});
```

## 完整示例

```typescript
const formOptions: VbenFormProps = {
  schema: querySchema(),
  wrapperClass: 'grid-cols-4',
};

const gridOptions: VxeGridProps = {
  columns: [
    { type: 'checkbox', width: 50 },
    { field: 'name', title: '名称' },
    { field: 'status', title: '状态', slots: { default: 'status' } },
    { field: 'createTime', title: '创建时间' },
    { field: 'action', title: '操作', slots: { default: 'action' } },
  ],
  proxyConfig: {
    ajax: {
      query: async ({ page }, formValues) => {
        return await userList({
          SkipCount: page.currentPage,
          MaxResultCount: page.pageSize,
          ...formValues,
        });
      },
    },
  },
};
```

## 插槽使用

### 工具栏插槽

```vue
<BasicTable>
  <template #toolbar-actions>
    <Button type="primary" @click="handleAdd">新增</Button>
  </template>
  <template #toolbar-tools>
    <Button @click="tableApi.query()">刷新</Button>
  </template>
</BasicTable>
```

### 列插槽

**模板方式：**

```vue
<BasicTable>
  <template #status="{ row }">
    <Tag :color="row.status === '0' ? 'red' : 'green'">
      {{ row.status === '0' ? '禁用' : '启用' }}
    </Tag>
  </template>
</BasicTable>

<script setup>
const columns = [
  { field: 'status', title: '状态', slots: { default: 'status' } },
];
</script>
```

**TSX 方式：**

```typescript
import { renderDict } from '#/utils/render';
import { DictEnum } from '#/constants';

const columns = [
  {
    field: 'status',
    title: '状态',
    slots: {
      default: ({ row }) => renderDict(row.state, DictEnum.SYS_NORMAL_DISABLE),
    },
  },
];
```

## 表格操作

### 操作列

`data.ts` 中建议为操作列设置固定宽度：

```typescript
{
  field: 'action',
  fixed: 'right',
  slots: { default: 'action' },
  title: '操作',
  width: 200,
}
```

页面中通过 `VbenTableAction` 渲染：

```vue
<template #action="{ row }">
  <VbenTableAction
    :actions="[
      {
        auth: 'system:user:edit',
        onClick: () => handleEdit(row),
        text: '编辑',
      },
      {
        auth: 'system:user:remove',
        danger: true,
        popConfirm: {
          title: '确认删除？',
          confirm: () => handleDelete(row),
        },
        text: '删除',
      },
    ]"
    align="center"
  />
</template>
```

当操作超过可展示数量时，由公共组件处理“更多”菜单。不要在业务页面额外混用图标按钮和“更多”菜单。

```typescript
// 刷新表格
tableApi.query();

// 获取选中行
const rows = tableApi.grid.getCheckboxRecords();

// 重置查询条件并刷新
tableApi.reload();

// 获取表单 API
tableApi.formApi.getValues();
```

## 排序功能

```typescript
const gridOptions: VxeGridProps = {
  sortConfig: {
    remote: true,  // 远程排序
  },
  proxyConfig: {
    ajax: {
      query: async ({ page, sort }, formValues) => {
        const params: any = { ...formValues };
        if (!isEmpty(sort)) {
          params.orderByColumn = sort.field;
          params.isAsc = sort.order;
        }
        return await userList(params);
      },
    },
  },
};

const gridEvents = {
  sortChange: () => tableApi.query(),
};
```

## 自定义列

保存列配置到本地存储：

```typescript
const gridOptions: VxeGridProps = {
  id: 'user-list',  // 全局唯一 ID
  customConfig: {
    storage: true,  // 开启本地存储
  },
};
```

## 相关文档

- [表单组件](/guide/frontend/components/form) - 表单使用
- [字典功能](/guide/frontend/features/dict) - 字典渲染
