---
outline: deep
---

# 权限

Yi.Mini 默认使用后端访问控制。后端负责返回用户可访问的菜单和权限码，前端负责把菜单转换为路由，并用权限码控制按钮显示。

## 当前流程

登录成功后，`apps/web-antd/src/store/auth.ts` 会执行：

1. `loginApi()` 获取 token。
2. `getUserInfoApi()` 请求 `/account` 获取用户信息。
3. 从响应中读取 `permissionCodes`、`roleCodes`、`user`。
4. 写入 Vben 用户信息和权限仓储：

```typescript
const { permissionCodes = [], roleCodes = [], user } = backUserInfo;

const userInfo = {
  permissions: permissionCodes,
  roles: roleCodes,
  userId: user.userId,
  username: user.userName,
};

userStore.setUserInfo(userInfo);
accessStore.setAccessCodes(userInfo.permissions);
```

菜单路由由 `apps/web-antd/src/router/access.ts` 请求 `/account/router` 后动态生成。

## 后端访问控制

`apps/web-antd/src/preferences.ts` 中保持：

```typescript
export const overridesPreferences = defineOverridesPreferences({
  app: {
    accessMode: 'backend',
  },
});
```

后端菜单接口需要返回 `name`、`path`、`component`、`meta`、`children` 等路由字段。前端通过 `backMenuToVbenMenu()` 转换后注册到路由表。

## 权限码

2.0 推荐权限码格式：

```text
{module}:{entity}:{action}
```

示例：

```text
system:user:list
system:user:add
system:user:edit
system:user:delete
monitor:audit-log:list
```

后端 `Yi.Framework.Operation` 会根据模块、实体和动作自动推断权限码，也可以通过 `[Permission]` 或配置映射显式指定。

::: warning 兼容说明
历史菜单数据中可能仍有 `remove`、`query`、`resetPwd` 等权限码。新增功能建议使用标准动作；兼容旧菜单时，需要后端显式映射或在服务方法上指定权限。
:::

## 按钮权限

### 组件方式

```vue
<script lang="ts" setup>
import { AccessControl } from '@vben/access';
</script>

<template>
  <AccessControl :codes="['system:user:add']" type="code">
    <a-button>新增用户</a-button>
  </AccessControl>
</template>
```

### API 方式

```vue
<script lang="ts" setup>
import { useAccess } from '@vben/access';

const { hasAccessByCodes } = useAccess();
</script>

<template>
  <a-button v-if="hasAccessByCodes(['system:user:edit'])">
    编辑用户
  </a-button>
</template>
```

### 指令方式

```vue
<template>
  <a-button v-access:code="'system:user:add'">新增用户</a-button>
  <a-button v-access:code="['system:user:delete']">删除用户</a-button>
</template>
```

## 角色控制

角色码来自 `/account` 响应中的 `roleCodes`，适合控制少量纯前端入口或特殊展示。

```vue
<script lang="ts" setup>
import { useAccess } from '@vben/access';

const { hasAccessByRoles } = useAccess();
</script>

<template>
  <a-button v-if="hasAccessByRoles(['admin'])">管理员可见</a-button>
  <a-button v-access:role="['admin', 'super']">管理角色可见</a-button>
</template>
```

## 前端访问控制

Vben 仍支持 `frontend` 和 `mixed` 模式，但 Yi.Mini 默认不推荐把业务菜单权限写死在前端。只有登录页、异常页、本地辅助页等基础路由建议放在 `localMenuList` 或本地静态路由中。

如果确实需要前端模式，可在路由 `meta.authority` 中写角色码：

```typescript
{
  meta: {
    authority: ['admin'],
  },
}
```
