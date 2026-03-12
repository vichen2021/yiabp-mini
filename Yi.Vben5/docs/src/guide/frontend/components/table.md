# 表格组件

基于 VXE Table 的 Grid 配置式表格封装。

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
    <a-button type="primary" @click="handleAdd">新增</a-button>
  </template>
  <template #toolbar-tools>
    <a-button @click="tableApi.query()">刷新</a-button>
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
import { DictEnum } from '@vben/constants';

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
