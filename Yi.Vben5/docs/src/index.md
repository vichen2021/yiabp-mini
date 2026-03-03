---
# https://vitepress.dev/reference/default-theme-home-page
layout: home
sidebar: false

hero:
  name: Yi.Mini
  text: 精简版 RBAC 权限管理框架
  tagline: 基于 ABP Framework 和 Vben5，开箱即用，简单高效
  image:
    src: https://unpkg.com/@vbenjs/static-source@0.1.7/source/logo-v1.webp
    alt: Yi.Mini
  actions:
    - theme: brand
      text: 快速开始 ->
      link: /guide/introduction/quick-start
    - theme: alt
      text: 在线预览
      link: https://yi.wjys.top
    - theme: alt
      text: 查看文档
      link: /guide/introduction/about

features:
  - icon: 🚀
    title: 最新技术栈
    details: 后端基于 .NET 10 + ABP Framework 10，前端基于 Vue 3 + Vben5 + Ant Design Vue 4
    link: /guide/backend/tech-stack
    linkText: 查看技术栈
  - icon: 🏗️
    title: DDD 分层架构
    details: 严格遵循领域驱动设计，5 层项目结构，清晰的模块边界和依赖关系
    link: /guide/backend/architecture
    linkText: 架构文档
  - icon: 🎯
    title: 专注 RBAC
    details: 精简版设计，专注于权限管理核心功能，去除冗余业务模块
    link: /guide/introduction/features
    linkText: 核心特性
  - icon: 📝
    title: 规范统一
    details: 统一的命名规范、编码规范和 Git 提交规范，保证代码质量
    link: /guide/standards/coding
    linkText: 开发规范
  - icon: 🤖
    title: Claude Skills
    details: 集成模块生成器、CRUD 生成器、字段同步器等 AI 辅助工具
    link: /guide/skills/module-generator
    linkText: 查看 Skills
  - icon: 🔧
    title: 开箱即用
    details: 完善的开发工具链，支持快速开发和部署
    link: /guide/frontend/development
    linkText: 开发指南
  - title: ABP Framework
    icon:
      src: /logos/vite.svg
    details: 基于 ABP Framework 10，提供企业级应用开发能力
    link: https://abp.io/
    linkText: 官方站点
  - title: Vben5
    icon:
      src: /logos/shadcn-ui.svg
    details: 基于 Vben Admin v5，现代化的前端管理模板
    link: https://github.com/vbenjs/vue-vben-admin
    linkText: 官方站点
  - title: SqlSugar
    icon:
      src: /logos/turborepo.svg
    details: 使用 SqlSugar 5 作为 ORM 框架，性能优异
    link: https://www.donet5.com/
    linkText: 官方站点
---
