# 常见问题

## 页面白屏

### 原因

页面模板使用了多个根元素，与 Vue Transition 组件冲突。

### 解决方案

确保页面使用单根元素包裹：

```vue
<!-- ✅ 正确 -->
<template>
  <div>
    <div>内容1</div>
    <div>内容2</div>
  </div>
</template>

<!-- ❌ 错误 -->
<template>
  <div>内容1</div>
  <div>内容2</div>
</template>
```

## 路由 404

### 原因

路由 `name` 重复，后者覆盖前者。

### 解决方案

确保所有路由 `name` 唯一：

```typescript
// ❌ 错误 - name 重复
{ path: '/user', name: 'User', ... }
{ path: '/user2', name: 'User', ... }  // 会覆盖上一个

// ✅ 正确
{ path: '/user', name: 'SystemUser', ... }
{ path: '/user2', name: 'BusinessUser', ... }
```

## Select 浮层偏移

### 原因

浮层默认挂载在 `body` 下，滚动时不跟随。

### 解决方案

设置 `getPopupContainer` 属性：

```vue
<template>
  <Select :getPopupContainer="getPopupContainer" />
</template>

<script setup lang="ts">
import { getVxePopupContainer } from '@vben/utils';

// 挂载到父节点
const getPopupContainer = getVxePopupContainer;
</script>
```

## 字典不显示

### 原因

`getDictOptions` 是异步封装，直接使用会拿到空数组。

### 解决方案

使用 `computed` 或在组件挂载后获取：

```typescript
import { getDictOptions } from '#/utils/dict';
import { DictEnum } from '@vben/constants';

// ✅ 使用 computed
{
  componentProps: {
    options: computed(() => getDictOptions(DictEnum.SYS_NORMAL_DISABLE)),
  },
}
```

## 配置修改不生效

### 原因

浏览器缓存了旧配置。

### 解决方案

清空浏览器缓存后重新访问。

## 相关文档

- [快速开始](/guide/frontend/quick-start) - 开发入门
- [配置项](/guide/frontend/configuration) - 配置说明

## 外部文档

遇到组件使用问题时，建议查阅以下官方文档：

- [Ant Design Vue](https://www.antdv.com/components/overview-cn) - 组件库官方文档
- [Vben Admin](https://doc.vben.pro/) - Vben Admin 官方文档
