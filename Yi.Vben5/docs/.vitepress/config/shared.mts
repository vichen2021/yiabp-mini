import type { PwaOptions } from '@vite-pwa/vitepress';
import type { HeadConfig } from 'vitepress';

import { resolve } from 'node:path';

import {
  viteArchiverPlugin,
  viteDayjsPlugin,
  viteVxeTableImportsPlugin,
} from '@vben/vite-config';

import {
  GitChangelog,
  GitChangelogMarkdownSection,
} from '@nolebase/vitepress-plugin-git-changelog/vite';
import tailwindcss from '@tailwindcss/vite';
import { defineConfig, postcssIsolateStyles } from 'vitepress';
import {
  groupIconMdPlugin,
  groupIconVitePlugin,
} from 'vitepress-plugin-group-icons';

import { demoPreviewPlugin } from './plugins/demo-preview';
import { search as zhSearch } from './zh.mts';

const giteeIcon =
  '<svg role="img" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path fill="currentColor" d="M11.984 0A12 12 0 0 0 0 12a12 12 0 0 0 12 12 12 12 0 0 0 12-12A12 12 0 0 0 12 0a12 12 0 0 0-.016 0zm6.09 5.333c.328 0 .593.266.592.593v1.482a.594.594 0 0 1-.593.592H9.777c-.982 0-1.778.796-1.778 1.778v5.63c0 .327.266.592.593.592h5.63c.327 0 .593-.265.593-.593v-1.481a.593.593 0 0 0-.593-.593h-3.556a.593.593 0 0 1-.593-.593V9.778c0-.327.266-.593.593-.593h5.63c.327 0 .593.266.593.593v6.815a2.37 2.37 0 0 1-2.37 2.37H6.518a.593.593 0 0 1-.593-.593V9.778a4.444 4.444 0 0 1 4.444-4.445h7.705z"/></svg>';

export const shared = defineConfig({
  appearance: 'dark',
  head: head(),
  markdown: {
    preConfig(md) {
      md.use(demoPreviewPlugin);
      md.use(groupIconMdPlugin);
    },
  },
  pwa: pwa(),
  srcDir: 'src',
  themeConfig: {
    logo: 'https://unpkg.com/@vbenjs/static-source@0.1.7/source/logo-v1.webp',
    search: {
      options: {
        locales: {
          ...zhSearch,
        },
      },
      provider: 'local',
    },
    siteTitle: 'Yi.Mini',
    socialLinks: [
      {
        ariaLabel: 'Gitee',
        icon: { svg: giteeIcon },
        link: 'https://gitee.com/vichen2021/yiabp-mini',
      },
    ],
  },
  title: 'Yi.Mini',
  vite: {
    build: {
      chunkSizeWarningLimit: Infinity,
      minify: 'terser',
    },
    css: {
      postcss: {
        plugins: [postcssIsolateStyles({ includeFiles: [/vp-doc\.css/] })],
      },
      preprocessorOptions: {
        scss: {
          api: 'modern',
        },
      },
    },
    json: {
      stringify: true,
    },
    plugins: [
      viteDayjsPlugin(),
      tailwindcss(),
      GitChangelog({
        mapAuthors: [],
        repoURL: () => 'https://gitee.com/vichen2021/yiabp-mini',
      }) as any,
      GitChangelogMarkdownSection() as any,
      viteArchiverPlugin({ outputDir: '.vitepress' }) as any,
      groupIconVitePlugin() as any,
      (await viteVxeTableImportsPlugin()) as any,
    ],
    server: {
      fs: {
        allow: ['../..'],
      },
      host: true,
      port: 6173,
    },

    ssr: {
      external: ['@vue/repl'],
    },
  },
});

function head(): HeadConfig[] {
  return [
    ['meta', { content: 'Yi.Mini Team', name: 'author' }],
    [
      'meta',
      {
        content:
          'Yi.Mini, YiMini, yimini, yi mini, yiabp-mini, yiabp, abp, .net10, abp10, vue3, vben5, rbac, 权限管理, AI Skills',
        name: 'keywords',
      },
    ],
    [
      'meta',
      {
        content:
          'Yi.Mini（YiMini/yimini）是基于 .NET 10、ABP10、Vben5.7 的轻量化 RBAC 权限管理系统，集成 AI Skills 快速开发工作流。',
        name: 'description',
      },
    ],
    ['meta', { content: 'Yi.Mini / YiMini 权限管理系统', property: 'og:title' }],
    [
      'meta',
      {
        content:
          'YiMini 是基于 .NET 10 + Vben5.7 的轻量化权限管理系统，内置 RBAC、多租户、文件管理、OSS 配置和 AI Skills。',
        property: 'og:description',
      },
    ],
    ['link', { href: '/favicon.ico', rel: 'icon', type: 'image/svg+xml' }],
    [
      'meta',
      {
        content:
          'width=device-width,initial-scale=1,minimum-scale=1.0,maximum-scale=1.0,user-scalable=no',
        name: 'viewport',
      },
    ],
    ['link', { href: '/favicon.ico', rel: 'icon' }],
    // [
    //   'script',
    //   {
    //     src: 'https://cdn.tailwindcss.com',
    //   },
    // ],
  ];
}

function pwa(): PwaOptions {
  return {
    includeManifestIcons: false,
    manifest: {
      description:
        'Yi.Mini 是基于 ABP Framework 和 Vben5 的精简版 RBAC 权限管理框架',
      icons: [
        {
          sizes: '192x192',
          src: 'https://unpkg.com/@vbenjs/static-source@0.1.7/source/pwa-icon-192.png',
          type: 'image/png',
        },
        {
          sizes: '512x512',
          src: 'https://unpkg.com/@vbenjs/static-source@0.1.7/source/pwa-icon-512.png',
          type: 'image/png',
        },
      ],
      id: '/',
      name: 'Yi.Mini 文档',
      short_name: 'yi_mini_doc',
      theme_color: '#ffffff',
    },
    outDir: resolve(process.cwd(), '.vitepress/dist'),
    registerType: 'autoUpdate',
    workbox: {
      globPatterns: ['**/*.{css,js,html,svg,png,ico,txt,woff2}'],
      maximumFileSizeToCacheInBytes: 5 * 1024 * 1024,
    },
  };
}
