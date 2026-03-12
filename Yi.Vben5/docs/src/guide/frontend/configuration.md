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

::: danger 重要
修改偏好设置后需要清空浏览器缓存！
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
