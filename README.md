# Yi.Mini

## 📖简介

Yi.Mini 是基于橙子老哥的 [Yi.Admin](https://gitee.com/ccnetcore/Yi) 框架开发的精简版 RBAC 权限管理框架，使用Vben5 Admin重写前端，采用若依风格设计。

本框架专注于 RBAC（基于角色的访问控制）功能，去除了其他业务模块，更适合快速搭建权限管理系统。

## 📚 框架文档

**👉 [查看框架文档](http://docs.wjys.top/) 👈**

> 详细的开发文档、API 说明、使用教程等，请访问文档站点。

## 📌后端

- **模块精简**：Yi.Mini 只保留了 Yi.Admin 的 **rbac** 相关模块，删除了 **ai-stock、bbs、chat-hub、digital-collectibles** 等模块

- **接口优化**：新增部门树表、菜单树表等接口，新增文件管理模块

- **前端重构**：使用vben5 & ant-design-vue 重写前端页面


  

## 🎨关于前端

前端基于 [ruoyi-plus-vben5](https://gitee.com/dapppp/ruoyi-plus-vben5) 仓库二次开发，采用 [Vben5](https://github.com/vbenjs/vue-vben-admin) 和 [Ant Design Vue](https://antdv.com/) 构建的 RuoYi-Vue-Plus 前端项目。

### 🔧环境要求

- **Node.js**：需要 20.15.0 及以上版本，推荐使用[nvm](https://github.com/nvm-sh/nvm) 进行 Node.js 版本管理
- **包管理器**：使用 [pnpm](https://pnpm.io/)

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

## 🤖 引入Claude Skills

本项目集成了四个 Claude Skills，帮助开发者快速生成代码和模块结构。这些 Skills 位于 `.claude/skills/` 目录下。

### 1. Module Generator（模块生成器）

**功能说明**：自动生成完整的 ABP 框架模块结构，包括所有必要的项目文件、模块类、目录结构，并自动更新主模块引用。

**使用场景**：当你需要在 `src/WebApi/module` 目录下创建新的模块时使用。

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
- **👀 [在线预览](https://yi.wjys.top/)** - 体验系统功能



## 🚀系统截图

![image-20260101175759249](/resource/image-20260101175759249.png)

![image-20260101175912025](/resource/image-20260101175912025.png)

![image-20260101180006771](/resource/image-20260101180006771.png)

## 🙏感谢

感谢以下开源项目的支持：

- [Yi.Admin](https://gitee.com/ccnetcore/Yi) - 橙子老哥的优秀框架
- [ruoyi-plus-vben5](https://gitee.com/dapppp/ruoyi-plus-vben5) - 基于 Vben5 的 RuoYi-Plus 前端实现



## 📬联系方式

如有问题或建议，欢迎联系：

- **QQ**：1363332824
- **微信**：vichen2022