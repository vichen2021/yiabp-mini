# 后端技术栈

## 核心框架

| 项目 | 技术 | 版本 |
|------|------|------|
| 框架 | .NET 10 + ABP Framework | 10.0.2 |
| ORM | SqlSugar | 5.1.4.211 |
| 数据库 | 默认SQLite，支持数据库类型查看[SqlSugar官网](https://www.donet5.com/Home/Doc) | - |
| 认证 | JWT Bearer | - |

## 基础设施

| 项目 | 技术 | 说明 |
|------|------|------|
| 后台任务 | Hangfire | 支持 Redis / Memory |
| 日志 | Serilog | - |
| DI 容器 | Autofac | - |
| 对象映射 | Mapster | 非 AutoMapper |
| 缓存 | FreeRedis / Memory Cache | - |
| 实时通信 | SignalR | - |
| 分布式锁 | Medallion | Redis |

## 重要说明

### ORM 选择

项目使用 **SqlSugar**（非 Entity Framework），仓储接口为 `ISqlSugarRepository<TEntity, TKey>`。

### 对象映射

项目使用 **Mapster**（非 AutoMapper），调用方式为 `entity.Adapt<TDto>()`。

### 解决方案文件

解决方案文件使用 `.slnx` 格式（XML 格式，非传统 `.sln`）。

### 版本管理

版本集中管理在 `version.props`，构建属性集中在 `common.props`。

## 相关文档

- [架构设计](/guide/backend/architecture) - 了解后端架构
- [实体定义](/guide/backend/entity) - 了解如何定义实体
- [模块开发](/guide/backend/module) - 了解应用服务开发

