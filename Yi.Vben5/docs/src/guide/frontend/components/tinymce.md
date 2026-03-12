# 富文本编辑器

使用 TinyMCE 作为富文本编辑器。

## 导入使用

```typescript
import { Tinymce } from '#/components/tinymce';
```

## 基本使用

```vue
<template>
  <Tinymce v-model="content" :height="400" />
</template>
```

## 配置项

| 属性 | 说明 | 类型 | 默认值 |
|------|------|------|--------|
| v-model | 绑定值 | `string` | - |
| height | 高度 | `number` | `400` |
| width | 宽度 | `string \| number` | `auto` |
| options | TinyMCE 配置 | `object` | `{}` |
| plugins | 插件列表 | `string[]` | 默认插件 |
| toolbar | 工具栏配置 | `string[]` | 默认工具栏 |
| showImageUpload | 显示图片上传 | `boolean` | `true` |

## 只读模式

```vue
<Tinymce v-model="content" :options="{ readonly: true }" />
```

## 注意事项

- 默认支持图片上传（点击、粘贴、拖拽）
- 使用本地自托管模式，打包文件较大
- 如需 CDN 模式，请修改 `editor.vue` 中的 `tinymceScriptSrc`

## 相关文档

- [表单组件](/guide/frontend/components/form) - 表单使用
