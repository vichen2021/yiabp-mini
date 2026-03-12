# 快速开始

## 环境要求

| 依赖 | 版本要求 | 说明 |
|------|----------|------|
| Node.js | >= 20.15.0 | 推荐使用 LTS 版本 |
| pnpm | >= 10.10.0 | 包管理器 |

推荐使用 [nvm](https://github.com/nvm-sh/nvm) 管理 Node.js 版本。

## 安装依赖

```bash
cd Yi.Vben5
pnpm install
```

## 启动项目

```bash
pnpm run dev:antd
```

前端服务默认运行在 `http://localhost:5666`

## 项目结构

```
Yi.Vben5/
├── apps/
│   └── web-antd/          # 主应用 - 开发目录
│       ├── src/
│       │   ├── api/       # API 接口
│       │   ├── views/     # 页面组件
│       │   ├── router/    # 路由配置
│       │   ├── store/     # 状态管理
│       │   └── utils/     # 工具函数
│       └── .env.*         # 环境变量
├── packages/              # 共享包
└── docs/                  # 项目文档
```

## 常用命令

| 命令 | 说明 |
|------|------|
| `pnpm run dev:antd` | 启动开发服务器 |
| `pnpm build:antd` | 构建生产版本 |
| `pnpm run check:type` | 类型检查 |
| `pnpm run lint` | 代码检查 |

## 相关文档

- [配置项](/guide/frontend/configuration) - 了解环境变量和偏好设置
- [组件](/guide/frontend/components/form) - 了解组件使用
- [常见问题](/guide/frontend/faq) - 常见问题解答
