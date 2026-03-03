# 路由和菜单

## 路由配置

路由配置位于 `apps/web-antd/src/router/routes/` 目录下。

## 菜单管理

菜单通过后端接口动态加载，支持：

- 菜单树形结构
- 权限绑定
- 动态路由

## 权限控制

路由权限通过 `meta.permission` 配置：

```typescript
{
  path: '/user',
  name: 'User',
  meta: {
    permission: 'system:user:list',
  },
}
```

## 相关文档

- [本地开发](/guide/frontend/development) - 了解开发环境
- [API 调用](/guide/frontend/api) - 了解 API 调用方式

