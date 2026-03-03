# 本地开发

## 环境要求

- **Node.js**：需要 20.15.0 及以上版本，推荐使用 [nvm](https://github.com/nvm-sh/nvm) 进行 Node.js 版本管理
- **包管理器**：使用 [pnpm](https://pnpm.io/)

## 安装依赖

```bash
# 进入前端项目目录
cd Yi.Vben5

# 安装依赖
pnpm install
```

## 运行项目

```bash
# 运行项目
pnpm run dev:antd
```

前端服务默认运行在 `http://localhost:5666`

## 打包项目

```bash
# 打包项目
pnpm build:antd
```

## 项目结构

前端项目采用 Monorepo 架构，使用 Turborepo + pnpm 管理：

```
Yi.Vben5/
├── apps/
│   └── web-antd/          # 主应用
├── packages/              # 共享包
└── playground/            # 开发测试
```

## 相关文档

- [路由和菜单](/guide/frontend/route) - 了解路由配置
- [API 调用](/guide/frontend/api) - 了解 API 调用方式
- [组件使用](/guide/frontend/components) - 了解组件使用

