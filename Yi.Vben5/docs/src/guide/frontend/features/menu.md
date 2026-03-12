# 菜单管理

菜单通过后端接口动态加载，支持树形结构和权限绑定。

## 菜单结构

```typescript
interface Menu {
  id: string;                    // 菜单ID
  parentId: string;              // 父菜单ID
  menuName: string;              // 菜单名称
  routerName?: string | null;    // 路由名称
  router?: string | null;        // 路由地址
  component?: string | null;     // 组件路径
  menuIcon?: string | null;      // 菜单图标
  menuType: string;              // 菜单类型（M目录/C菜单/F按钮）
  permissionCode?: string | null; // 权限标识
  orderNum: number;              // 排序号
  state: boolean;                // 状态
  isShow: boolean;               // 是否显示
  isLink: boolean;               // 是否外链
  isCache: boolean;              // 是否缓存
  query?: string | null;         // 查询参数
  remark?: string | null;        // 备注
  children?: Menu[];             // 子菜单
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

### 在线图标（推荐）

直接在 [Iconify 图标库](https://icon-sets.iconify.design/) 搜索图标，复制图标名称到菜单配置即可。

```typescript
// 格式：图标集名称:图标名称
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
| Iconify | `iconify:` | `iconify:mdi:home` |

::: tip 提示
在线图标需要网络连接，如果部署在内网环境，请使用离线图标或本地 SVG 图标。
:::

### 离线图标

如需离线使用图标，需要安装对应的图标包：

```bash
cd packages/icons
pnpm add @iconify/icons-图标集 -D
```

然后在 `packages/icons/src/iconify-offline/menu-icons.ts` 中添加：

```typescript
// 以 ic:baseline-15mp 为例
import baseline15mp from '@iconify/icons-ic/baseline-15mp';

addIcon('ic:baseline-15mp', baseline15mp);
```

### 本地 SVG 图标

将 SVG 文件放入 `packages/icons/src/svg/icons` 目录，然后在 `index.ts` 中注册：

```typescript
// 格式：svg:图标名称（不带扩展名）
const SvgCustomIcon = createIconifyIcon('svg:custom-icon');

export {
  // ...
  SvgCustomIcon,
};
```

在菜单中配置：

```typescript
{
  menuIcon: 'svg:custom-icon',
}
```

::: warning 注意
添加本地 SVG 图标后需要**重启 Vite** 才能生效！
:::

## 相关文档

- [路由配置](/guide/frontend/features/route) - 路由详细配置
- [Iconify 图标库](https://icon-sets.iconify.design/) - 在线搜索图标
- [Vben 图标文档](https://doc.vben.pro/guide/essentials/icons) - 官方图标使用说明
