import type { DefaultTheme } from 'vitepress';

import { defineConfig } from 'vitepress';

import { version } from '../../../package.json';

export const zh = defineConfig({
  description: 'Yi.Mini - 基于 ABP Framework 和 Vben5 的精简版 RBAC 权限管理框架',
  lang: 'zh-Hans',
  themeConfig: {
    darkModeSwitchLabel: '主题',
    darkModeSwitchTitle: '切换到深色模式',
    docFooter: {
      next: '下一页',
      prev: '上一页',
    },
    editLink: {
      pattern:
        'https://gitee.com/vichen2021/yiabp-mini/edit/main/Yi.Vben5/docs/src/:path',
      text: '在 Gitee 上编辑此页面',
    },
    footer: {
      copyright: `Copyright © ${new Date().getFullYear()} Yi.Mini`,
      message: '基于 MIT 许可发布.',
    },
    langMenuLabel: '多语言',
    lastUpdated: {
      formatOptions: {
        dateStyle: 'short',
        timeStyle: 'medium',
      },
      text: '最后更新于',
    },
    lightModeSwitchTitle: '切换到浅色模式',
    nav: nav(),

    outline: {
      label: '页面导航',
    },
    returnToTopLabel: '回到顶部',

    sidebar: {
      '/commercial/': { base: '/commercial/', items: sidebarCommercial() },
      '/components/': { base: '/components/', items: sidebarComponents() },
      '/guide/': { base: '/guide/', items: sidebarGuide() },
    },
    sidebarMenuLabel: '菜单',
  },
});

function sidebarGuide(): DefaultTheme.SidebarItem[] {
  return [
    {
      collapsed: false,
      text: '简介',
      items: [
        {
          link: 'introduction/about',
          text: '关于 Yi.Mini',
        },
        {
          link: 'introduction/quick-start',
          text: '快速开始',
        },
        {
          link: 'introduction/features',
          text: '核心特性',
        },
      ],
    },
    {
      text: '技术栈',
      items: [
        { link: 'tech-stack/backend', text: '后端技术栈' },
        { link: 'tech-stack/frontend', text: '前端技术栈' },
      ],
    },
    {
      text: '后端开发',
      items: [
        { link: 'backend/architecture', text: '架构设计' },
        { link: 'backend/naming', text: '命名规范' },
        { link: 'backend/entity', text: '实体定义' },
        { link: 'backend/enum', text: '枚举使用' },
        { link: 'backend/service', text: '应用服务' },
        { link: 'backend/api', text: 'API 模式' },
        { link: 'backend/permission', text: '权限与日志' },
        { link: 'backend/query', text: '查询模式' },
        { link: 'backend/module', text: '模块开发' },
      ],
    },
    {
      text: '前端开发',
      items: [
        { link: 'frontend/development', text: '本地开发' },
        { link: 'frontend/route', text: '路由和菜单' },
        { link: 'frontend/api', text: 'API 调用' },
        { link: 'frontend/components', text: '组件使用' },
        { link: 'frontend/build', text: '构建与部署' },
      ],
    },
    {
      text: 'Claude Skills',
      items: [
        { link: 'skills/module-generator', text: '模块生成器' },
        { link: 'skills/crud-generator', text: 'CRUD 生成器' },
        { link: 'skills/field-sync', text: '字段同步器' },
        { link: 'skills/skill-creator', text: '技能创建器' },
      ],
    },
    {
      text: '开发规范',
      items: [
        { link: 'standards/coding', text: '编码规范' },
        { link: 'standards/git', text: 'Git 提交规范' },
        { link: 'standards/testing', text: '测试规范' },
      ],
    },
    {
      text: '其他',
      items: [
        { link: 'other/faq', text: '常见问题' },
        { link: 'other/contact', text: '联系方式' },
      ],
    },
  ];
}

function sidebarCommercial(): DefaultTheme.SidebarItem[] {
  return [
    {
      link: 'community',
      text: '交流群',
    },
    {
      link: 'technical-support',
      text: '技术支持',
    },
    {
      link: 'customized',
      text: '定制开发',
    },
  ];
}

function sidebarComponents(): DefaultTheme.SidebarItem[] {
  return [
    {
      text: '组件',
      items: [
        {
          link: 'introduction',
          text: '介绍',
        },
      ],
    },
    {
      collapsed: false,
      text: '布局组件',
      items: [
        {
          link: 'layout-ui/page',
          text: 'Page 页面',
        },
      ],
    },
    {
      collapsed: false,
      text: '通用组件',
      items: [
        {
          link: 'common-ui/vben-api-component',
          text: 'ApiComponent Api组件包装器',
        },
        {
          link: 'common-ui/vben-alert',
          text: 'Alert 轻量提示框',
        },
        {
          link: 'common-ui/vben-modal',
          text: 'Modal 模态框',
        },
        {
          link: 'common-ui/vben-drawer',
          text: 'Drawer 抽屉',
        },
        {
          link: 'common-ui/vben-form',
          text: 'Form 表单',
        },
        {
          link: 'common-ui/vben-vxe-table',
          text: 'Vxe Table 表格',
        },
        {
          link: 'common-ui/vben-count-to-animator',
          text: 'CountToAnimator 数字动画',
        },
        {
          link: 'common-ui/vben-ellipsis-text',
          text: 'EllipsisText 省略文本',
        },
      ],
    },
  ];
}

function nav(): DefaultTheme.NavItem[] {
  return [
    {
      activeMatch: '^/guide/',
      text: '文档',
      link: '/guide/introduction/about',
    },
    {
      text: '在线预览',
      link: 'https://yi.wjys.top',
    },
    {
      text: version,
      items: [
        {
          link: 'https://gitee.com/vichen2021/yiabp-mini',
          text: 'Gitee',
        },
      ],
    },
  ];
}

export const search: DefaultTheme.AlgoliaSearchOptions['locales'] = {
  root: {
    placeholder: '搜索文档',
    translations: {
      button: {
        buttonAriaLabel: '搜索文档',
        buttonText: '搜索文档',
      },
      modal: {
        errorScreen: {
          helpText: '你可能需要检查你的网络连接',
          titleText: '无法获取结果',
        },
        footer: {
          closeText: '关闭',
          navigateText: '切换',
          searchByText: '搜索提供者',
          selectText: '选择',
        },
        noResultsScreen: {
          noResultsText: '无法找到相关结果',
          reportMissingResultsLinkText: '点击反馈',
          reportMissingResultsText: '你认为该查询应该有结果？',
          suggestedQueryText: '你可以尝试查询',
        },
        searchBox: {
          cancelButtonAriaLabel: '取消',
          cancelButtonText: '取消',
          resetButtonAriaLabel: '清除查询条件',
          resetButtonTitle: '清除查询条件',
        },
        startScreen: {
          favoriteSearchesTitle: '收藏',
          noRecentSearchesText: '没有搜索历史',
          recentSearchesTitle: '搜索历史',
          removeFavoriteSearchButtonTitle: '从收藏中移除',
          removeRecentSearchButtonTitle: '从搜索历史中移除',
          saveRecentSearchButtonTitle: '保存至搜索历史',
        },
      },
    },
  },
};
