# 路由配置

路由配置位于 `apps/web-antd/src/router/`。Yi.Mini 默认使用后端路由模式，登录后从后端菜单接口生成可访问路由。

## 访问模式

`apps/web-antd/src/preferences.ts` 中保持：

```typescript
export const overridesPreferences = defineOverridesPreferences({
  app: {
    accessMode: 'backend',
  },
});
```

## 后端路由加载

核心逻辑在 `apps/web-antd/src/router/access.ts`：

1. 调用 `getAllMenusApi()` 请求 `/account/router`。
2. 使用 `backMenuToVbenMenu()` 把后端菜单转为 Vben 路由。
3. 与 `localMenuList` 合并，保留登录页、异常页等本地基础路由。
4. 交给 `generateAccessible()` 注册到 Vue Router。

后端返回的组件值会被转换：

| 后端 component | 前端处理 |
| --- | --- |
| `Layout` | 转为 `BasicLayout` |
| `ParentView` | 作为多级菜单父节点，不渲染实际页面 |
| `InnerLink` | 转为 `IFrameView`，用于 iframe 内嵌 |
| `RootMenu` | 转为一级根菜单，并隐藏子菜单 |
| 业务页面路径 | 拼成 `/{component}`，例如 `system/user/index` |

## 菜单 Meta

后端路由的 `meta` 会映射为 Vben 路由元数据：

```typescript
meta: {
  hideInMenu: menu.hidden,
  icon: menu.meta?.icon,
  keepAlive: !menu.meta?.noCache,
  title: menu.meta?.title,
}
```

少量详情页需要补充 `activePath`、`requireHomeRedirect` 等前端专用信息时，在 `routeMetaMapping` 中按路由路径维护。

## 页面路径

菜单的业务页面组件不写 `views/` 和 `.vue`：

```text
system/user/index
```

对应文件：

```text
apps/web-antd/src/views/system/user/index.vue
```

## 权限控制

路由是否返回由后端控制。按钮级权限使用登录用户的 `permissionCodes`：

```vue
<template>
  <a-button v-access:code="['system:user:add']">新增</a-button>
</template>
```

## 注意事项

### 页面模板要求

页面必须使用单根元素，否则可能导致渲染异常：

```vue
<template>
  <div>
    <div>内容1</div>
    <div>内容2</div>
  </div>
</template>
```

### 路由 Name 唯一

后端菜单的 `name` 必须唯一，否则动态注册时会互相覆盖。

## 相关文档

- [菜单管理](/guide/frontend/features/menu) - 菜单配置
- [权限](/guide/in-depth/access) - 权限码和角色控制
