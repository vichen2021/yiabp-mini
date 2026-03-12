# 路由配置

路由配置位于 `apps/web-antd/src/router/` 目录。

## 静态路由

```typescript
// routes/modules/system.ts
import type { RouteRecordRaw } from 'vue-router';

const routes: RouteRecordRaw[] = [
  {
    path: '/system',
    name: 'System',
    component: LAYOUT,
    redirect: '/system/user',
    children: [
      {
        path: 'user',
        name: 'SystemUser',
        component: () => import('#/views/system/user/index.vue'),
        meta: {
          title: '用户管理',
          icon: 'ant-design:user-outlined',
        },
      },
    ],
  },
];

export default routes;
```

## 动态路由

项目使用后端路由模式，菜单由后端接口返回：

```typescript
// preferences.ts
{
  app: {
    accessMode: 'backend',
  },
}
```

## 路由 Meta

```typescript
interface RouteMeta {
  title: string;           // 菜单标题
  icon?: string;           // 菜单图标
  hideMenu?: boolean;      // 隐藏菜单
  hideChildren?: boolean;  // 隐藏子菜单
  ignoreAuth?: boolean;    // 忽略权限
  authority?: string[];    // 权限码
  keepAlive?: boolean;     // 缓存页面
}
```

## 权限控制

```vue
<template>
  <!-- 按钮权限控制 -->
  <a-button v-access:code="['system:user:add']">新增</a-button>
</template>
```

## 注意事项

### 页面模板要求

页面必须使用**单根元素**，否则会导致白屏：

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

### 路由 Name 唯一

路由 `name` 必须唯一，否则后一个会覆盖前一个导致 404。

## 相关文档

- [菜单管理](/guide/frontend/features/menu) - 菜单配置
