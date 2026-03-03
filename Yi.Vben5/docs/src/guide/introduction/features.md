# 核心特性

## 🎯 专注 RBAC

Yi.Mini 专注于权限管理核心功能，提供完整的 RBAC（基于角色的访问控制）能力：

- **用户管理**：用户信息管理、密码重置、状态控制
- **角色管理**：角色创建、权限分配、角色继承
- **菜单管理**：菜单树形结构、权限绑定、动态路由
- **部门管理**：部门树形结构、用户归属
- **权限控制**：细粒度权限控制、按钮级权限

## 🏗️ DDD 分层架构

严格遵循领域驱动设计，5 层项目结构：

```
module/{模块名}/
├── Yi.Framework.{Module}.Domain.Shared/          # 共享层
├── Yi.Framework.{Module}.Domain/                 # 领域层
├── Yi.Framework.{Module}.Application.Contracts/  # 应用契约层
├── Yi.Framework.{Module}.Application/            # 应用层
└── Yi.Framework.{Module}.SqlSugarCore/           # 基础设施层
```

## 🔧 完善的开发工具

### Claude Skills

集成多个 AI 辅助开发工具：

- **模块生成器**：自动生成完整的 ABP 模块结构
- **CRUD 生成器**：初始化完整的业务模块脚手架
- **字段同步器**：同步实体字段变更到整个代码库
- **技能创建器**：创建自定义开发工具

## 📝 规范统一

- **命名规范**：统一的类、方法、变量命名规范
- **编码规范**：统一的代码风格和最佳实践
- **Git 规范**：遵循 Conventional Commits 规范
- **测试规范**：统一的测试命名和结构

## 🚀 现代化技术栈

### 后端

- .NET 10 + ABP Framework 10.0.2
- SqlSugar 5.1.4.211（ORM）
- FreeRedis / Memory Cache（缓存）
- Hangfire（后台任务）
- SignalR（实时通信）

### 前端

- Vue 3.5 + TypeScript 5.8
- Vben Admin v5.5.7
- Ant Design Vue 4.2
- VXE Table 4.13
- ECharts 5.6

## 🎨 开箱即用

- **动态 API**：应用服务自动映射为 REST 接口
- **权限控制**：内置权限码和操作日志
- **Excel 导入导出**：内置 Excel 处理能力
- **批量操作**：支持批量删除等操作

## 📦 模块化设计

- **清晰的模块边界**：禁止跨模块直接引用 Domain 层
- **防腐层适配器**：支持跨模块交互
- **集成事件**：支持分布式事件总线

