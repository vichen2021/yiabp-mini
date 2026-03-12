# 关于 Yi.Mini

## 📖 简介

Yi.Mini 是基于橙子老哥的 [Yi.Admin](https://gitee.com/ccnetcore/Yi) 框架开发的精简版 RBAC 权限管理框架，使用 Vben5 Admin 重写前端，采用若依风格设计。

本框架专注于 RBAC（基于角色的访问控制）功能，去除了其他业务模块，更适合快速搭建权限管理系统。

## 🎯 设计理念

- **精简专注**：只保留 RBAC 相关模块，去除冗余业务
- **开箱即用**：完善的开发工具链和代码生成器
- **规范统一**：统一的编码规范和开发流程
- **易于扩展**：清晰的模块边界，便于二次开发

## 📌 主要特点

### 后端

- **模块精简**：Yi.Mini 只保留了 Yi.Admin 的 **rbac** 相关模块，删除了 **ai-stock、bbs、chat-hub、digital-collectibles** 等模块
- **接口优化**：新增部门树表、菜单树表等接口，新增文件管理模块
- **DDD 架构**：严格遵循领域驱动设计，5 层项目结构

### 前端

- **前端重构**：使用 vben5 & ant-design-vue 重写前端页面
- **现代化 UI**：基于 Vben5 和 Ant Design Vue 构建
- **若依风格**：采用若依风格设计，符合国内开发习惯

## 🚀 快速开始

查看 [快速开始](/guide/introduction/quick-start) 了解如何开始使用 Yi.Mini。

## 📚 文档导航

- [技术栈](/guide/backend/tech-stack) - 了解项目使用的技术栈
- [后端开发](/guide/backend/architecture) - 后端架构和开发指南
- [前端开发](/guide/frontend/quick-start) - 前端开发指南
- [开发规范](/guide/standards/coding) - 编码规范和最佳实践
- [Claude Skills](/guide/skills/module-generator) - AI 辅助开发工具

## 🙏 致谢

感谢以下开源项目的支持：

- [Yi.Admin](https://gitee.com/ccnetcore/Yi) - 橙子老哥的优秀框架
- [ruoyi-plus-vben5](https://gitee.com/dapppp/ruoyi-plus-vben5) - 基于 Vben5 的 RuoYi-Plus 前端实现
- [Vben5](https://github.com/vbenjs/vue-vben-admin) - 现代化的前端管理模板
- [ABP Framework](https://abp.io/) - 企业级应用开发框架

