# 快速开始

## 环境要求

### 后端

- **.NET SDK**：需要 .NET 10 SDK
- **数据库**：默认SQLite，支持数据库类型查看[SqlSugar官网](https://www.donet5.com/Home/Doc)
- **缓存（可选）**：Redis

### 前端

- **Node.js**：需要 20.15.0 及以上版本，推荐使用 [nvm](https://github.com/nvm-sh/nvm) 进行 Node.js 版本管理
- **包管理器**：使用 [pnpm](https://pnpm.io/)

## 后端启动

### 1. 配置数据库

编辑 `Yi.Abp/src/Yi.Abp.Web/appsettings.json`，开发环境一般无需修改：

```json
{
  "ConnectionStrings": {
    "Default": "Data Source=yiabp.db;"
  }
}
```

### 2. 运行项目

```bash
# 进入后端项目目录
cd Yi.Abp/src/Yi.Abp.Web

# 还原依赖
dotnet restore

# 运行项目
dotnet run
```

后端服务默认运行在 `https://localhost:19002`

## 前端启动

### 1. 安装依赖

```bash
# 进入前端项目目录
cd Yi.Vben5

# 安装依赖
pnpm install
```

### 2. 运行项目

```bash
# 运行项目
pnpm run dev:antd
```

前端服务默认运行在 `http://localhost:5666`

### 3. 打包项目

```bash
# 打包项目
pnpm build:antd
```

## 默认账号

- **用户名**：cc
- **密码**：123456

## 下一步

- 查看 [技术栈文档](/guide/backend/tech-stack) 了解项目使用的技术
- 查看 [后端开发指南](/guide/backend/architecture) 开始后端开发
- 查看 [前端开发指南](/guide/frontend/quick-start) 开始前端开发
