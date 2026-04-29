# 菜单管理

Yi.Mini 菜单分两类数据：系统菜单管理页使用 `system/menu` 接口维护原始菜单数据；登录后的动态路由使用 `/account/router` 返回前端可直接转换的路由树。

## 菜单管理模型

菜单管理页的模型位于 `apps/web-antd/src/api/system/menu/model.d.ts`：

```typescript
interface Menu {
  id: string;
  parentId: string;
  orderNum: number;
  state: boolean;
  menuName: string;
  routerName?: string | null;
  menuType: string;
  permissionCode?: string | null;
  menuIcon?: string | null;
  router?: string | null;
  isLink: boolean;
  isCache: boolean;
  isShow: boolean;
  component?: string | null;
  query?: string | null;
  children?: Menu[];
}
```

常用字段说明：

| 字段 | 说明 |
| --- | --- |
| `menuName` | 菜单显示名称 |
| `menuType` | 菜单类型，通常为目录、菜单、按钮 |
| `permissionCode` | 按钮或菜单绑定的权限码 |
| `routerName` | Vue Router name，需要保持唯一 |
| `router` | 路由路径 |
| `component` | 页面组件路径或特殊组件值 |
| `menuIcon` | Iconify 图标名称 |
| `isShow` | 是否在菜单中显示 |
| `isCache` | 是否缓存页面 |
| `isLink` | 是否外链 |
| `query` | JSON 字符串形式的路由参数 |

## 动态路由模型

登录后前端调用 `apps/web-antd/src/api/core/menu.ts` 中的 `getAllMenusApi()`：

```typescript
export async function getAllMenusApi() {
  return requestClient.get<Menu[]>('/account/router');
}
```

该接口返回的是路由树，字段包括 `name`、`path`、`component`、`hidden`、`meta`、`children`。转换逻辑在 `apps/web-antd/src/router/access.ts` 的 `backMenuToVbenMenu()`。

## 权限绑定

权限码格式推荐与后端 2.0 自动权限保持一致：

```text
{module}:{entity}:{action}
```

示例：

```text
system:user:list
system:user:add
system:user:edit
system:user:delete
```

按钮权限：

```vue
<template>
  <a-button v-access:code="['system:user:add']">新增</a-button>
</template>
```

::: warning 兼容说明
历史种子数据中可能存在 `remove`、`query`、`resetPwd` 等权限码。新增菜单建议使用 `list`、`detail`、`add`、`edit`、`delete`、`export`、`import` 等标准动作；需要兼容旧码时，由后端显式映射。
:::

## 图标配置

直接在 [Iconify 图标库](https://icon-sets.iconify.design/) 搜索图标，复制图标名称到菜单配置即可。

```typescript
{
  menuIcon: 'ant-design:user-outlined',
}
```

常用图标库：

| 图标集 | 前缀 | 示例 |
|--------|------|------|
| Ant Design | `ant-design:` | `ant-design:home-outlined` |
| Material Design | `mdi:` | `mdi:account` |
| Carbon | `carbon:` | `carbon:user` |

::: warning 注意
添加本地 SVG 图标后需要重启 Vite 才能生效。
:::

## 相关文档

- [路由配置](/guide/frontend/features/route) - 路由详细配置
- [权限](/guide/in-depth/access) - 权限码和角色控制
