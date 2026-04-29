# 配置项

## 环境变量

环境变量文件位于 `apps/web-antd/` 目录下：

| 文件 | 说明 |
|------|------|
| `.env` | 公共配置 |
| `.env.development` | 开发环境 |
| `.env.production` | 生产环境 |
| `.env.test` | 测试环境 |

### 核心配置

```ini
# 应用标题
VITE_APP_TITLE=Yi Admin

# 应用命名空间
VITE_APP_NAMESPACE=vben-web-antd

# 端口号
VITE_PORT=5666

# 基础路径
VITE_BASE=/

# 后端 API 地址
VITE_GLOB_API_URL=/api

# SignalR Hub 地址
VITE_GLOB_HUB_URL=/hub

# 开启 SignalR
VITE_GLOB_SIGNALR_ENABLE=true

# 客户端 ID
VITE_GLOB_APP_CLIENT_ID=e5cd7e4891bf95d1d19206ce24a7b32e
```

### 应用命名空间

`VITE_APP_NAMESPACE` 用于区分浏览器缓存、Pinia 持久化数据等本地存储。当前 `apps/web-antd/src/main.ts` 会自动把应用版本和环境拼进命名空间：

```typescript
const namespace = `${import.meta.env.VITE_APP_NAMESPACE}-${appVersion}-${env}`;
```

`appVersion` 来自 `apps/web-antd/package.json`。前端版本升级到 `2.0.0` 后，最终命名空间会随版本变化，从而自然隔离旧版本缓存。

### 加密配置

```ini
# 全局加密开关
VITE_GLOB_ENABLE_ENCRYPT=false

# RSA 公钥（请求加密）
VITE_GLOB_RSA_PUBLIC_KEY=xxx

# RSA 私钥（响应解密）
VITE_GLOB_RSA_PRIVATE_KEY=xxx
```

::: warning 注意
RSA 公私钥需要两对：
- 请求加密（前端）→ 后端解密
- 响应解密（前端）← 后端加密
:::

## 偏好设置

偏好设置文件：`apps/web-antd/src/preferences.ts`

```typescript
import { defineOverridesPreferences } from '@vben/preferences';

export const overridesPreferences = defineOverridesPreferences({
  app: {
    accessMode: 'backend',        // 后端路由模式
    enableRefreshToken: false,    // 不需要 refresh token
    name: import.meta.env.VITE_APP_TITLE,
  },
  footer: {
    enable: false,                // 不显示 footer
  },
  tabbar: {
    persist: false,               // 标签页不持久化
  },
  theme: {
    semiDarkSidebar: false,       // 浅色侧边栏
  },
});
```

::: warning 注意
修改 `VITE_APP_NAMESPACE` 或应用版本会改变本地存储命名空间，相当于启用一套新的前端缓存。
:::

## 更换 Logo

在 `preferences.ts` 中配置：

```typescript
logo: {
  enable: true,
  source: '/logo.png',  // 本地 public 目录
  // source: 'https://example.com/logo.png',  // 网络图片
},
```

## 相关文档

- [快速开始](/guide/frontend/quick-start) - 开始开发
- [构建部署](/guide/frontend/build) - 构建和部署
