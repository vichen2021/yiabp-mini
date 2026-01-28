# Yi.Mini

## 📖简介

Yi.Mini 是基于橙子老哥的 [Yi.Admin](https://gitee.com/ccnetcore/Yi) 框架开发的精简版 RBAC 权限管理框架，使用Vben5 Admin重写前端，采用若依风格设计。

本框架专注于 RBAC（基于角色的访问控制）功能，去除了其他业务模块，更适合快速搭建权限管理系统。


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

本项目集成了三个 Claude Skills，帮助开发者快速生成代码和模块结构。这些 Skills 位于 `.claude/skills/` 目录下。

### 1. Module Generator（模块生成器）

**功能说明**：自动生成完整的 ABP 框架模块结构，包括所有必要的项目文件、模块类、目录结构，并自动更新主模块引用。

**使用场景**：当你需要在 `src/WebApi/module` 目录下创建新的模块时使用。

**提示词示例**：

- `创建一个名为 ContentManagement 的新模块`
- `使用 module-generator 创建 OrderManagement 模块`

**生成内容**：
- 5 个项目层：Domain.Shared、Domain、Application.Contracts、Application、SqlSugarCore
- 模块类和项目文件
- 目录结构（Entities、Dtos、IServices、Services、Repositories）
- 自动更新主模块文件和解决方案文件

### 2. Business Module Initializer（业务模块初始化器）

**功能说明**：初始化完整的业务模块脚手架，包括后端（C# .NET with ABP framework）和前端（Vue3 + Vben5 + Ant Design Vue）。创建实体类、DTOs、服务接口、服务实现、仓储、API 文件和视图组件。

**使用场景**：当你需要创建新的业务功能（如部门管理、用户管理）或任何需要全栈实现的 CRUD 模块时使用。

**提示词示例**：

- `初始化 Product 业务模块，包含实体、服务和前端页面`
- `帮我生成 Order 模块的完整业务代码`
- `使用 business-module-initializer 创建用户管理功能`

**生成内容**：

**后端**：
- Entity 实体类（`Domain/Entities/`）
- DTO 类（`Application.Contracts/Dtos/`）
- 服务接口（`Application.Contracts/IServices/`）
- 服务实现（`Application/Services/`）
- 仓储接口和实现（如需要）

**前端**：
- API 文件（`api/system/{entity-name}/`）
- 视图文件（`views/system/{entity-name}/`）
- 表单和表格配置

### 3. Skill Creator（技能创建器）

**功能说明**：创建有效技能的指南。当你想创建新技能（或更新现有技能）来扩展 Claude 的专业知识、工作流程或工具集成时使用。

**使用场景**：当你需要为特定领域或任务创建自定义 Skill 时使用。

**提示词示例**：
- `我想创建一个 PDF 处理技能，应该怎么做？`
- `帮我创建一个用于处理 Excel 文件的技能`
- `使用 skill-creator 创建一个 API 文档生成技能`
- `如何创建一个自定义的业务规则验证技能？`

**创建流程**：
1. 理解技能的具体使用示例
2. 规划可重用的技能内容（脚本、参考资料、资源）
3. 初始化技能（运行 `init_skill.py`）
4. 编辑技能（实现资源和编写 SKILL.md）
5. 打包技能（运行 `package_skill.py`）
6. 根据实际使用迭代优化

**技能结构**：
```
skill-name/
├── SKILL.md (必需)
├── scripts/ (可选) - 可执行代码
├── references/ (可选) - 参考文档
└── assets/ (可选) - 输出资源文件
```


## 预览

[预览地址点这里](https://yi.wjys.top/)



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