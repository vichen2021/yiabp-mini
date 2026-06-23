# 关于 Yi.Mini

## 📖 简介

Yi.Mini(YiMini) 是基于 [Yi.Admin](https://gitee.com/ccnetcore/Yi) 开发的轻量化 RBAC 权限管理框架，使用 Vben5 Admin 重写前端，采用若依风格设计。

> 别名：YiMini、yimini、yi mini、Yi.Mini、yiabp-mini

本框架聚焦权限管理、租户管理、参数配置、审计日志、文件管理等后台系统高频基础能力，去除了非通用业务模块，启动更轻、结构更清晰，更适合作为新业务系统的二开底座。

## 📌项目亮点

- **版本升级**：将 .NET8 更新至 .NET10

- **模块精简**：Yi.Mini 以 **RBAC** 为核心，保留租户、设置、审计等基础能力，删除了 **ai-stock、bbs、chat-hub、digital-collectibles** 等非通用业务模块

- **前端重构**：前端基于 [ruoyi-plus-vben5](https://gitee.com/dapppp/ruoyi-plus-vben5) 仓库二次开发，使用 [Vben5.7](https://github.com/vbenjs/vue-vben-admin) 和 [Ant Design Vue](https://antdv.com/) 构建的 RuoYi-Vue-Plus 前端项目。

- **接口优化**：新增部门树表、菜单树表等接口，新增租户套餐功能，新增文件管理模块

- **权限增强**：支持基于 Service 元数据自动推断权限码，标准 CRUD 只需声明资源即可获得 `query/add/edit/remove` 等权限语义

- **操作记录增强**：操作记录复用 Service Action 元数据，可通过 `[OperLogEntity]` 自动推断日志标题和操作类型

- **多租户套餐**：新增租户套餐能力，可定义租户可用菜单范围，并在租户初始化或同步时自动下发菜单和管理员权限

- **文件与 OSS**：新增独立文件管理模块，基于 ABP Blob Storing 支持本地文件与阿里云 OSS 切换，并支持全局默认配置和租户级 OSS 覆盖

- **AI 开发工作流**：内置 Module Generator、CRUD Generator、Field Sync、Skill Creator 等 Skills，并提供 Agents 协作规范

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
