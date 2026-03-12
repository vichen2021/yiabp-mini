# 上传组件

文件上传和图片上传组件。

## 导入使用

```typescript
import { FileUpload, ImageUpload } from '#/components/upload';
```

## 基本使用

```vue
<template>
  <!-- 图片上传 -->
  <ImageUpload v-model:value="imageIds" :max-count="5" />
  
  <!-- 文件上传 -->
  <FileUpload v-model:value="fileIds" :max-count="3" accept=".pdf,.doc" />
</template>
```

## 配置项

| 属性 | 说明 | 类型 | 默认值 |
|------|------|------|--------|
| v-model:value | 绑定值（ossId） | `string \| string[]` | - |
| maxCount | 最大上传数量 | `number` | `1` |
| maxSize | 文件最大大小（MB） | `number` | `5` |
| accept | 可接受文件类型 | `string` | - |
| multiple | 是否支持多选 | `boolean` | `false` |
| disabled | 是否禁用 | `boolean` | `false` |
| helpMessage | 是否显示提示 | `boolean` | `true` |
| removeConfirm | 删除前确认 | `boolean` | `false` |

## 注意事项

### 绑定值类型

- `maxCount=1` 时，绑定值为 `string`
- `maxCount>1` 时，绑定值为 `string[]`

### string/string[] 转换

后端返回 `string,string,string` 格式时：

```typescript
// 赋值前转换
formApi.setFieldValue('images', record.images.split(','));

// 提交前转换
const data = cloneDeep(await formApi.getValues());
data.images = data.images.join(',');
```

### 预览自定义

```vue
<FileUpload
  :preview="(file) => {
    // 自定义预览逻辑，如下载
    window.open(file.url);
  }"
/>
```

## 相关文档

- [表单组件](/guide/frontend/components/form) - 表单使用
