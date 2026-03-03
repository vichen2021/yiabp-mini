# 构建与部署

## 构建

```bash
# 构建项目
pnpm build:antd

# 构建测试环境
pnpm build:antd:test
```

## 构建产物

构建产物位于 `apps/web-antd/dist/` 目录。

## 部署

将构建产物部署到 Web 服务器（如 Nginx、IIS）即可。

## 环境变量

通过 `.env` 文件配置环境变量：

- `.env.development` - 开发环境
- `.env.production` - 生产环境
- `.env.test` - 测试环境

## 相关文档

- [本地开发](/guide/frontend/development) - 了解开发环境

