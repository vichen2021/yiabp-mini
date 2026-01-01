# Yi.Mini

## 📖简介

Yi.Mini 是基于橙子老哥的 [Yi.Admin](https://gitee.com/ccnetcore/Yi) 框架开发的精简版 RBAC 权限管理框架，采用若依风格设计。

本框架专注于 RBAC（基于角色的访问控制）功能，去除了其他业务模块，更适合快速搭建权限管理系统。


## 📌后端与 Yi.Admin 差异

- **模块精简**：Yi.Mini 只保留 **rbac** 相关模块，删除了 **ai-stock、bbs、chat-hub、digital-collectibles** 模块

- **数据优化**：增加并优化了部分种子数据，适配若依前端的数据结构

- **接口扩展**：为兼容前端，新增了部门树表、菜单树表等接口

- **详细内容**：https://gitee.com/ccnetcore/Yi

  

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
```



## 🚀开发进度

- [x] ##### 系统管理模块对接

- [ ] 系统监控

- [ ] 代码生成



## 🙏感谢

感谢以下开源项目的支持：

- [Yi.Admin](https://gitee.com/ccnetcore/Yi) - 橙子老哥的优秀框架
- [ruoyi-plus-vben5](https://gitee.com/dapppp/ruoyi-plus-vben5) - 基于 Vben5 的 RuoYi-Plus 前端实现



## 📬联系方式

如有问题或建议，欢迎联系：

- **QQ**：1363332824
- **微信**：vichen2022