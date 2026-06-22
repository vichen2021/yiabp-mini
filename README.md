# Yi.Mini

## 📖简介

Yi.Mini 是基于 [Yi.Admin](https://gitee.com/ccnetcore/Yi) 开发的轻量化 RBAC 权限管理框架，使用 Vben5 Admin 重写前端，采用若依风格设计。

本框架聚焦权限管理、租户管理、参数配置、审计日志、文件管理等后台系统高频基础能力，去除了非通用业务模块，启动更轻、结构更清晰，更适合作为新业务系统的二开底座。

**[演示站点](https://yi.wjys.top/)** https://yi.wjys.top/

## 📚 框架文档

**👉 [查看框架文档](http://docs.wjys.top/) 👈**

> 详细的开发文档、API 说明、使用教程等，请访问文档站点。

## 🤖 AI 快速开发指南

项目支持结合 AI 工具和 Skills 快速完成模块设计、开发文档输出、模块结构生成、CRUD 脚手架生成以及字段同步，适合用于快速构建业务模块。

**👉 [查看 AI 快速开发指南](https://docs.wjys.top/guide/introduction/ai-development.html) 👈**

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

## 🔍 本框架与 Yi.Admin 的功能差异

Yi.Mini 不是简单删减版，而是面向真实项目二开的轻量化重构版本：保留后台系统最常用的基础能力，升级前后端技术栈，统一前端体验，并内置 AI 开发工作流，让新项目更容易启动、扩展和长期维护。

| 对比项 | Yi.Mini | Yi.Admin |
| --- | --- | --- |
| 项目定位 | 聚焦 RBAC 与通用后台基础能力，适合作为新业务系统起点 | 功能生态更完整，适合学习 Yi 全生态和参考多模块实现 |
| 后端版本 | 升级到 .NET 10 + ABP10 + SqlSugar | .NET 8 + ABP + SqlSugar |
| 仓库体量 | 只保留核心后端和 Vben5 管理端 | 包含后端、多套管理端、BBS 前端、文档与 Docker 示例，内容更全但体量更大 |
| 基础模块 | 以 `rbac` 为核心，保留租户、设置、审计等基础模块，并新增文件管理 | 包含更多业务演示模块，适合参考完整生态 |
| 权限控制 | 支持基于 Service 元数据自动推断权限码 | 不支持自动推断，需手动声明权限 |
| 操作记录 | 操作记录复用 Service Action 元数据，可通过 `[OperLogEntity]` 自动推断 | 不支持自动推断，需手动声明操作记录 |
| 多租户套餐 | 新增租户套餐能力，可定义租户可用菜单范围，并在租户初始化或同步时自动下发菜单和管理员权限 | 无 |
| 文件管理 | 独立文件管理模块，记录文件元数据，支持上传、下载、URL 解析、Provider 来源展示等通用能力 | 不支持完整文件管理 |
| OSS 配置 | 基于 ABP Blob Storing 支持本地文件与阿里云 OSS 切换，支持全局默认配置和租户级 OSS 覆盖 | 不支持 OSS 文件存储 |
| 前端选择 | 统一使用 Vben5.7 + Ant Design Vue + 若依风格，减少多前端维护成本 | 同时提供 Vben5、RuoYi、Pure Vue、BBS 等多套前端 |
| AI 开发 | 内置 Module Generator、CRUD Generator、Field Sync、Skill Creator 等 Skills，并提供 Agents 协作规范 | 无 |
| 代码生成 | 推荐通过 AI Skills 生成模块、CRUD 和字段同步 | 提供独立 Tool 项目与原有代码生成思路 |


### 🔧环境要求

- **Node.js**：需要 `^22.18.0 || ^24.0.0`，推荐使用[nvm](https://github.com/nvm-sh/nvm) 进行 Node.js 版本管理
- **包管理器**：使用 [pnpm](https://pnpm.io/) `>=11.0.0`

#### 🏃启动命令

```bash
# 进入项目目录
cd Yi.Vben5

# 安装依赖
pnpm install

# 运行项目
pnpm run dev:antd

# 打包项目
pnpm build:antd
```

## 🤖 引入 AI Skills

本项目集成了四个 AI Skills，帮助开发者快速生成代码和模块结构。这些 Skills 位于 `.claude/skills/` 目录下。

### 1. Module Generator（模块生成器）

**功能说明**：自动生成完整的 ABP 框架模块结构，包括所有必要的项目文件、模块类、目录结构，并自动更新主模块引用。业务模块使用 `Yi.Module.{Module}` 前缀，`Yi.Framework.*` 仅用于框架基础设施项目。

**使用场景**：当你需要在 `Yi.Abp/module` 目录下创建新的业务模块时使用。

### 2. CRUD Generator（CRUD代码生成器）

**功能说明**：初始化完整的业务模块脚手架，包括后端（实体类、DTOs、服务接口、服务实现、菜单种子数据）和前端（API 模型、API 函数、视图组件、表单配置）。自动生成全栈 CRUD 功能所需的所有文件，并完成构建验证。

**使用场景**：当你需要创建新的业务功能（如部门管理、用户管理）或任何需要全栈实现的 CRUD 模块时使用。

### 3. Field Sync（字段同步器）

**功能说明**：同步实体字段变更到整个代码库，包括 DTOs、服务实现、前端 API 和视图、以及字典种子数据。

**使用场景**：当你需要添加、删除或修改实体字段时使用。

### 4. Skill Creator（技能创建器）

**功能说明**：创建有效技能的指南。当你想创建新技能（或更新现有技能）来扩展 Claude 的专业知识、工作流程或工具集成时使用。

**使用场景**：当你需要为特定领域、任务或开发流程创建自定义 Skill 时使用。


## 🔗 快速链接

- **📚 [框架文档](http://docs.wjys.top/)** - 完整的开发文档和使用指南
- **🤖 [AI 快速开发指南](https://docs.wjys.top/guide/introduction/ai-development.html)** - 使用 AI 工具和 Skills 快速开发业务模块
- **👀 [在线预览](https://yi.wjys.top/)** - 体验系统功能



## 🚀系统截图

![image-20260101175759249](/resource/image-20260101175759249.png)

![image-20260101175912025](/resource/image-20260101175912025.png)

![image-20260101180006771](/resource/image-20260101180006771.png)

## 🙏感谢

感谢以下开源项目的支持：

- [Yi.Admin](https://gitee.com/ccnetcore/Yi) - 橙子老哥的优秀框架
- [ruoyi-plus-vben5](https://gitee.com/dapppp/ruoyi-plus-vben5) - 基于 Vben5 的 RuoYi-Plus 前端实现



## 📬联系方式 & 交流群
![微信交流群](resource/622-629wx.jpg)
如有问题或建议，欢迎联系：

- **QQ**：1363332824
- **微信**：vichen2022
