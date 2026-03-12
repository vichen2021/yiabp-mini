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
import { Input, Select } from 'ant-design-vue';
</script>
```

## 全局组件

以下组件已全局注册，无需导入：

| 组件 | 说明 |
|------|------|
| `a-button` | Ant Design 按钮 |
| `GhostButton` | 表格操作列专用按钮 |

```vue
<template>
  <a-button type="primary">按钮</a-button>
  <GhostButton>操作按钮</GhostButton>
</template>
```

## 按需导入（可选）

如需启用按需导入，在 `vite.config.mts` 中取消注释：

```typescript
import { AntDesignVueResolver } from 'unplugin-vue-components/resolvers';
import Components from 'unplugin-vue-components/vite';

export default defineConfig({
  vite: {
    plugins: [
      Components({
        dirs: [],
        dts: './types/components.d.ts',
        resolvers: [
          AntDesignVueResolver({
            importStyle: false,
          }),
        ],
      }),
    ],
  },
});
```

启用后可直接使用 `a-xxx` 组件：

```vue
<template>
  <a-input v-model:value="name" />
  <a-select v-model:value="type" />
</template>
```

## 相关文档

- [表单组件](/guide/frontend/components/form) - 表单使用
- [表格组件](/guide/frontend/components/table) - 表格使用
