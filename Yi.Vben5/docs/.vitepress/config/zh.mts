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
        {
          link: 'introduction/ai-development',
          text: '全栈+AI开发指南',
        },
      ],
    },
    {
      text: '后端开发',
      items: [
        { link: 'backend/tech-stack', text: '后端技术栈' },
        { link: 'backend/startup', text: '启动项目' },
        { link: 'backend/architecture', text: '架构设计' },
        { link: 'backend/naming', text: '命名规范' },
        { link: 'backend/entity', text: '实体定义' },
        { link: 'backend/module', text: '模块开发' },
        { link: 'backend/enum', text: '枚举/系统字典' },
        { link: 'backend/permission', text: '权限与日志' },
      ],
    },
    {
      text: '前端开发',
      items: [
        { link: 'frontend/quick-start', text: '快速开始' },
        { link: 'frontend/configuration', text: '配置项' },
        { link: 'frontend/build', text: '构建部署' },
        { link: 'frontend/faq', text: '常见问题' },
      ],
    },
    {
      text: '前端组件',
      items: [
        { link: 'frontend/components/import', text: '组件导入' },
        { link: 'frontend/components/form', text: '表单组件' },
        { link: 'frontend/components/table', text: '表格组件' },
        { link: 'frontend/components/upload', text: '上传组件' },
        { link: 'frontend/components/tinymce', text: '富文本编辑器' },
      ],
    },
    {
      text: '前端功能',
      items: [
        { link: 'frontend/features/route', text: '路由配置' },
        { link: 'frontend/features/menu', text: '菜单管理' },
        { link: 'frontend/features/dict', text: '字典功能' },
      ],
    },
    {
      text: 'Claude Skills',
      items: [
        { link: 'skills/what-is-skill', text: '什么是 Skill？' },
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
      text: '欢迎 Star ⭐️',
      link: 'https://gitee.com/vichen2021/yiabp-mini',
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
