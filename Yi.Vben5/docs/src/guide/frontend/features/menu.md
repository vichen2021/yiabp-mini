# 菜单管理

菜单通过后端接口动态加载，支持树形结构和权限绑定。

## 菜单结构

```typescript
interface Menu {
  id: string;
  parentId: string;
  name: string;
  path: string;
  component: string;
  icon: string;
  sort: number;
  visible: boolean;
  children?: Menu[];
}
```

## 权限绑定

菜单与权限码绑定，用于控制按钮级别的权限：

```typescript
// 路由配置
{
  path: '/system/user',
  meta: {
    permission: 'system:user:list',
  },
}

// 按钮权限
<a-button v-access:code="['system:user:add']">新增</a-button>
```

## 图标配置

使用 Iconify 图标：

```typescript
// 在菜单数据中配置
{
  icon: 'ant-design:user-outlined',
}
```

常用图标库：
- `ant-design:` - Ant Design 图标
- `mdi:` - Material Design 图标
- `carbon:` - Carbon 图标

## 相关文档

- [路由配置](/guide/frontend/features/route) - 路由详细配置
