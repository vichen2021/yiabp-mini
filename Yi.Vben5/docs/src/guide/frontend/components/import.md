# 组件导入

项目默认使用**手动导入**方式。

## 手动导入

```vue
<template>
  <div>
    <Input v-model:value="name" />
    <Select v-model:value="type" :options="options" />
  </div>
</template>

<script setup lang="ts">
import { Input, Select } from 'antdv-next';
</script>
```

## 全局组件

以下组件已全局注册，无需导入：

| 组件 | 说明 |
|------|------|
| `VbenButton` | Vben 通用按钮 |
| `VbenTableAction` | 表格操作列专用组件 |

```vue
<template>
  <VbenButton variant="default">按钮</VbenButton>
  <VbenTableAction :actions="actions" />
</template>
```

## 按需导入

当前项目不再建议通过 `AntDesignVueResolver` 自动注册 antd 组件。业务代码需要 antdv-next 组件时，直接从 `antdv-next` 手动导入；表单、表格、弹窗、抽屉等优先使用项目封装的 adapter 和 Vben 组件。

## 相关文档

- [表单组件](/guide/frontend/components/form) - 表单使用
- [表格组件](/guide/frontend/components/table) - 表格使用
