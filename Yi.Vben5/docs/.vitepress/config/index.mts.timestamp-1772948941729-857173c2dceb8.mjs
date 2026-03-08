// .vitepress/config/index.mts
import { withPwa } from "file:///D:/Users/develop/YiABP/yiabp-mini/Yi.Vben5/node_modules/.pnpm/@vite-pwa+vitepress@1.1.0_v_9b6441e4ef62150688f6008868f6cbeb/node_modules/@vite-pwa/vitepress/dist/index.mjs";
import { defineConfigWithTheme } from "file:///D:/Users/develop/YiABP/yiabp-mini/Yi.Vben5/node_modules/.pnpm/vitepress@1.6.4_@algolia+cl_05345280283d788b6be7eba3b6aabfd6/node_modules/vitepress/dist/node/index.js";

// .vitepress/config/shared.mts
import { resolve } from "node:path";
import {
  viteArchiverPlugin,
  viteVxeTableImportsPlugin
} from "file:///D:/Users/develop/YiABP/yiabp-mini/Yi.Vben5/internal/vite-config/dist/index.mjs";
import {
  GitChangelog,
  GitChangelogMarkdownSection
} from "file:///D:/Users/develop/YiABP/yiabp-mini/Yi.Vben5/node_modules/.pnpm/@nolebase+vitepress-plugin-_2519c030b091cce7c9be785c513df7b8/node_modules/@nolebase/vitepress-plugin-git-changelog/dist/vite/index.mjs";
import tailwind from "file:///D:/Users/develop/YiABP/yiabp-mini/Yi.Vben5/node_modules/.pnpm/tailwindcss@3.4.19_yaml@2.8.2/node_modules/tailwindcss/lib/index.js";
import { defineConfig as defineConfig2, postcssIsolateStyles } from "file:///D:/Users/develop/YiABP/yiabp-mini/Yi.Vben5/node_modules/.pnpm/vitepress@1.6.4_@algolia+cl_05345280283d788b6be7eba3b6aabfd6/node_modules/vitepress/dist/node/index.js";
import {
  groupIconMdPlugin,
  groupIconVitePlugin
} from "file:///D:/Users/develop/YiABP/yiabp-mini/Yi.Vben5/node_modules/.pnpm/vitepress-plugin-group-icon_77011aa2910490de1f4cbb48265289a7/node_modules/vitepress-plugin-group-icons/dist/index.js";

// .vitepress/config/plugins/demo-preview.ts
import crypto from "node:crypto";
import { readdirSync } from "node:fs";
import { join } from "node:path";
var rawPathRegexp = (
  // eslint-disable-next-line regexp/no-super-linear-backtracking, regexp/strict
  /^(.+?(?:\.([\da-z]+))?)(#[\w-]+)?(?: ?{(\d+(?:[,-]\d+)*)? ?(\S+)?})? ?(?:\[(.+)])?$/
);
function rawPathToToken(rawPath) {
  const [
    filepath = "",
    extension = "",
    region = "",
    lines = "",
    lang = "",
    rawTitle = ""
  ] = (rawPathRegexp.exec(rawPath) || []).slice(1);
  const title = rawTitle || filepath.split("/").pop() || "";
  return { extension, filepath, lang, lines, region, title };
}
var demoPreviewPlugin = (md) => {
  md.core.ruler.after("inline", "demo-preview", (state) => {
    const insertComponentImport = (importString) => {
      const index = state.tokens.findIndex(
        (i) => i.type === "html_block" && i.content.match(/<script setup>/g)
      );
      if (index === -1) {
        const importComponent = new state.Token("html_block", "", 0);
        importComponent.content = `<script setup>
${importString}
</script>
`;
        state.tokens.splice(0, 0, importComponent);
      } else {
        if (state.tokens[index]) {
          const content = state.tokens[index].content;
          state.tokens[index].content = content.replace(
            "</script>",
            `${importString}
</script>`
          );
        }
      }
    };
    const regex = /<DemoPreview[^>]*\sdir="([^"]*)"/g;
    state.src = state.src.replaceAll(regex, (_match, dir) => {
      const componentDir = join(process.cwd(), "src", dir).replaceAll(
        "\\",
        "/"
      );
      let childFiles = [];
      let dirExists = true;
      try {
        childFiles = readdirSync(componentDir, {
          encoding: "utf8",
          recursive: false,
          withFileTypes: false
        }) || [];
      } catch {
        dirExists = false;
      }
      if (!dirExists) {
        return "";
      }
      const uniqueWord = generateContentHash(componentDir);
      const ComponentName = `DemoComponent_${uniqueWord}`;
      insertComponentImport(
        `import ${ComponentName} from '${componentDir}/index.vue'`
      );
      const { path: _path } = state.env;
      const index = state.tokens.findIndex((i) => i.content.match(regex));
      if (!state.tokens[index]) {
        return "";
      }
      const firstString = "index.vue";
      childFiles = childFiles.sort((a, b) => {
        if (a === firstString) return -1;
        if (b === firstString) return 1;
        return a.localeCompare(b, "en", { sensitivity: "base" });
      });
      state.tokens[index].content = `<DemoPreview files="${encodeURIComponent(JSON.stringify(childFiles))}" ><${ComponentName}/>
        `;
      const _dummyToken = new state.Token("", "", 0);
      const tokenArray = [];
      childFiles.forEach((filename) => {
        const templateStart = new state.Token("html_inline", "", 0);
        templateStart.content = `<template #${filename}>`;
        tokenArray.push(templateStart);
        const resolvedPath = join(componentDir, filename);
        const { extension, filepath, lang, lines, title } = rawPathToToken(resolvedPath);
        const token = new state.Token("fence", "code", 0);
        token.info = `${lang || extension}${lines ? `{${lines}}` : ""}${title ? `[${title}]` : ""}`;
        token.content = `<<< ${filepath}`;
        token.src = [resolvedPath];
        tokenArray.push(token);
        const templateEnd = new state.Token("html_inline", "", 0);
        templateEnd.content = "</template>";
        tokenArray.push(templateEnd);
      });
      const endTag = new state.Token("html_inline", "", 0);
      endTag.content = "</DemoPreview>";
      tokenArray.push(endTag);
      state.tokens.splice(index + 1, 0, ...tokenArray);
      return "";
    });
  });
};
function generateContentHash(input, length = 10) {
  const hash = crypto.createHash("sha256").update(input).digest("hex");
  return Number.parseInt(hash, 16).toString(36).slice(0, length);
}

// .vitepress/config/zh.mts
import { defineConfig } from "file:///D:/Users/develop/YiABP/yiabp-mini/Yi.Vben5/node_modules/.pnpm/vitepress@1.6.4_@algolia+cl_05345280283d788b6be7eba3b6aabfd6/node_modules/vitepress/dist/node/index.js";
var zh = defineConfig({
  description: "Yi.Mini - \u57FA\u4E8E ABP Framework \u548C Vben5 \u7684\u7CBE\u7B80\u7248 RBAC \u6743\u9650\u7BA1\u7406\u6846\u67B6",
  lang: "zh-Hans",
  themeConfig: {
    darkModeSwitchLabel: "\u4E3B\u9898",
    darkModeSwitchTitle: "\u5207\u6362\u5230\u6DF1\u8272\u6A21\u5F0F",
    docFooter: {
      next: "\u4E0B\u4E00\u9875",
      prev: "\u4E0A\u4E00\u9875"
    },
    editLink: {
      pattern: "https://gitee.com/vichen2021/yiabp-mini/edit/main/Yi.Vben5/docs/src/:path",
      text: "\u5728 Gitee \u4E0A\u7F16\u8F91\u6B64\u9875\u9762"
    },
    footer: {
      copyright: `Copyright \xA9 ${(/* @__PURE__ */ new Date()).getFullYear()} Yi.Mini`,
      message: "\u57FA\u4E8E MIT \u8BB8\u53EF\u53D1\u5E03."
    },
    langMenuLabel: "\u591A\u8BED\u8A00",
    lastUpdated: {
      formatOptions: {
        dateStyle: "short",
        timeStyle: "medium"
      },
      text: "\u6700\u540E\u66F4\u65B0\u4E8E"
    },
    lightModeSwitchTitle: "\u5207\u6362\u5230\u6D45\u8272\u6A21\u5F0F",
    nav: nav(),
    outline: {
      label: "\u9875\u9762\u5BFC\u822A"
    },
    returnToTopLabel: "\u56DE\u5230\u9876\u90E8",
    sidebar: {
      "/commercial/": { base: "/commercial/", items: sidebarCommercial() },
      "/components/": { base: "/components/", items: sidebarComponents() },
      "/guide/": { base: "/guide/", items: sidebarGuide() }
    },
    sidebarMenuLabel: "\u83DC\u5355"
  }
});
function sidebarGuide() {
  return [
    {
      collapsed: false,
      text: "\u7B80\u4ECB",
      items: [
        {
          link: "introduction/about",
          text: "\u5173\u4E8E Yi.Mini"
        },
        {
          link: "introduction/quick-start",
          text: "\u5FEB\u901F\u5F00\u59CB"
        },
        {
          link: "introduction/features",
          text: "\u6838\u5FC3\u7279\u6027"
        },
        {
          link: "introduction/ai-development",
          text: "\u5168\u6808+AI\u5F00\u53D1\u6307\u5357"
        }
      ]
    },
    {
      text: "\u540E\u7AEF\u5F00\u53D1",
      items: [
        { link: "backend/tech-stack", text: "\u540E\u7AEF\u6280\u672F\u6808" },
        { link: "backend/startup", text: "\u542F\u52A8\u9879\u76EE" },
        { link: "backend/architecture", text: "\u67B6\u6784\u8BBE\u8BA1" },
        { link: "backend/naming", text: "\u547D\u540D\u89C4\u8303" },
        { link: "backend/entity", text: "\u5B9E\u4F53\u5B9A\u4E49" },
        { link: "backend/enum", text: "\u679A\u4E3E/\u7CFB\u7EDF\u5B57\u5178" },
        { link: "backend/service", text: "\u5E94\u7528\u670D\u52A1" },
        { link: "backend/api", text: "API \u6A21\u5F0F" },
        { link: "backend/permission", text: "\u6743\u9650\u4E0E\u65E5\u5FD7" },
        { link: "backend/query", text: "\u67E5\u8BE2\u6A21\u5F0F" },
        { link: "backend/module", text: "\u6A21\u5757\u5F00\u53D1" }
      ]
    },
    {
      text: "\u524D\u7AEF\u5F00\u53D1",
      items: [
        { link: "frontend/tech-stack", text: "\u524D\u7AEF\u6280\u672F\u6808" },
        { link: "frontend/development", text: "\u672C\u5730\u5F00\u53D1" },
        { link: "frontend/route", text: "\u8DEF\u7531\u548C\u83DC\u5355" },
        { link: "frontend/api", text: "API \u8C03\u7528" },
        { link: "frontend/components", text: "\u7EC4\u4EF6\u4F7F\u7528" },
        { link: "frontend/build", text: "\u6784\u5EFA\u4E0E\u90E8\u7F72" }
      ]
    },
    {
      text: "Claude Skills",
      items: [
        { link: "skills/what-is-skill", text: "\u4EC0\u4E48\u662F Skill\uFF1F" },
        { link: "skills/module-generator", text: "\u6A21\u5757\u751F\u6210\u5668" },
        { link: "skills/crud-generator", text: "CRUD \u751F\u6210\u5668" },
        { link: "skills/field-sync", text: "\u5B57\u6BB5\u540C\u6B65\u5668" },
        { link: "skills/skill-creator", text: "\u6280\u80FD\u521B\u5EFA\u5668" }
      ]
    },
    {
      text: "\u5F00\u53D1\u89C4\u8303",
      items: [
        { link: "standards/coding", text: "\u7F16\u7801\u89C4\u8303" },
        { link: "standards/git", text: "Git \u63D0\u4EA4\u89C4\u8303" },
        { link: "standards/testing", text: "\u6D4B\u8BD5\u89C4\u8303" }
      ]
    },
    {
      text: "\u5176\u4ED6",
      items: [
        { link: "other/faq", text: "\u5E38\u89C1\u95EE\u9898" },
        { link: "other/contact", text: "\u8054\u7CFB\u65B9\u5F0F" }
      ]
    }
  ];
}
function sidebarCommercial() {
  return [
    {
      link: "community",
      text: "\u4EA4\u6D41\u7FA4"
    },
    {
      link: "technical-support",
      text: "\u6280\u672F\u652F\u6301"
    },
    {
      link: "customized",
      text: "\u5B9A\u5236\u5F00\u53D1"
    }
  ];
}
function sidebarComponents() {
  return [
    {
      text: "\u7EC4\u4EF6",
      items: [
        {
          link: "introduction",
          text: "\u4ECB\u7ECD"
        }
      ]
    },
    {
      collapsed: false,
      text: "\u5E03\u5C40\u7EC4\u4EF6",
      items: [
        {
          link: "layout-ui/page",
          text: "Page \u9875\u9762"
        }
      ]
    },
    {
      collapsed: false,
      text: "\u901A\u7528\u7EC4\u4EF6",
      items: [
        {
          link: "common-ui/vben-api-component",
          text: "ApiComponent Api\u7EC4\u4EF6\u5305\u88C5\u5668"
        },
        {
          link: "common-ui/vben-alert",
          text: "Alert \u8F7B\u91CF\u63D0\u793A\u6846"
        },
        {
          link: "common-ui/vben-modal",
          text: "Modal \u6A21\u6001\u6846"
        },
        {
          link: "common-ui/vben-drawer",
          text: "Drawer \u62BD\u5C49"
        },
        {
          link: "common-ui/vben-form",
          text: "Form \u8868\u5355"
        },
        {
          link: "common-ui/vben-vxe-table",
          text: "Vxe Table \u8868\u683C"
        },
        {
          link: "common-ui/vben-count-to-animator",
          text: "CountToAnimator \u6570\u5B57\u52A8\u753B"
        },
        {
          link: "common-ui/vben-ellipsis-text",
          text: "EllipsisText \u7701\u7565\u6587\u672C"
        }
      ]
    }
  ];
}
function nav() {
  return [
    {
      activeMatch: "^/guide/",
      text: "\u6587\u6863",
      link: "/guide/introduction/about"
    },
    {
      text: "\u5728\u7EBF\u9884\u89C8",
      link: "https://yi.wjys.top"
    },
    {
      text: "\u6B22\u8FCE Star \u2B50\uFE0F",
      link: "https://gitee.com/vichen2021/yiabp-mini"
    }
  ];
}
var search = {
  root: {
    placeholder: "\u641C\u7D22\u6587\u6863",
    translations: {
      button: {
        buttonAriaLabel: "\u641C\u7D22\u6587\u6863",
        buttonText: "\u641C\u7D22\u6587\u6863"
      },
      modal: {
        errorScreen: {
          helpText: "\u4F60\u53EF\u80FD\u9700\u8981\u68C0\u67E5\u4F60\u7684\u7F51\u7EDC\u8FDE\u63A5",
          titleText: "\u65E0\u6CD5\u83B7\u53D6\u7ED3\u679C"
        },
        footer: {
          closeText: "\u5173\u95ED",
          navigateText: "\u5207\u6362",
          searchByText: "\u641C\u7D22\u63D0\u4F9B\u8005",
          selectText: "\u9009\u62E9"
        },
        noResultsScreen: {
          noResultsText: "\u65E0\u6CD5\u627E\u5230\u76F8\u5173\u7ED3\u679C",
          reportMissingResultsLinkText: "\u70B9\u51FB\u53CD\u9988",
          reportMissingResultsText: "\u4F60\u8BA4\u4E3A\u8BE5\u67E5\u8BE2\u5E94\u8BE5\u6709\u7ED3\u679C\uFF1F",
          suggestedQueryText: "\u4F60\u53EF\u4EE5\u5C1D\u8BD5\u67E5\u8BE2"
        },
        searchBox: {
          cancelButtonAriaLabel: "\u53D6\u6D88",
          cancelButtonText: "\u53D6\u6D88",
          resetButtonAriaLabel: "\u6E05\u9664\u67E5\u8BE2\u6761\u4EF6",
          resetButtonTitle: "\u6E05\u9664\u67E5\u8BE2\u6761\u4EF6"
        },
        startScreen: {
          favoriteSearchesTitle: "\u6536\u85CF",
          noRecentSearchesText: "\u6CA1\u6709\u641C\u7D22\u5386\u53F2",
          recentSearchesTitle: "\u641C\u7D22\u5386\u53F2",
          removeFavoriteSearchButtonTitle: "\u4ECE\u6536\u85CF\u4E2D\u79FB\u9664",
          removeRecentSearchButtonTitle: "\u4ECE\u641C\u7D22\u5386\u53F2\u4E2D\u79FB\u9664",
          saveRecentSearchButtonTitle: "\u4FDD\u5B58\u81F3\u641C\u7D22\u5386\u53F2"
        }
      }
    }
  }
};

// .vitepress/config/shared.mts
var shared = defineConfig2({
  appearance: "dark",
  head: head(),
  markdown: {
    preConfig(md) {
      md.use(demoPreviewPlugin);
      md.use(groupIconMdPlugin);
    }
  },
  pwa: pwa(),
  srcDir: "src",
  themeConfig: {
    logo: "https://unpkg.com/@vbenjs/static-source@0.1.7/source/logo-v1.webp",
    search: {
      options: {
        locales: {
          ...search
        }
      },
      provider: "local"
    },
    siteTitle: "Yi.Mini",
    socialLinks: [
      { icon: "gitee", link: "https://gitee.com/vichen2021/yiabp-mini" }
    ]
  },
  title: "Yi.Mini",
  vite: {
    build: {
      chunkSizeWarningLimit: Infinity,
      minify: "terser"
    },
    css: {
      postcss: {
        plugins: [
          tailwind(),
          postcssIsolateStyles({ includeFiles: [/vp-doc\.css/] })
        ]
      },
      preprocessorOptions: {
        scss: {
          api: "modern"
        }
      }
    },
    json: {
      stringify: true
    },
    plugins: [
      GitChangelog({
        mapAuthors: [],
        repoURL: () => "https://gitee.com/vichen2021/yiabp-mini"
      }),
      GitChangelogMarkdownSection(),
      viteArchiverPlugin({ outputDir: ".vitepress" }),
      groupIconVitePlugin(),
      await viteVxeTableImportsPlugin()
    ],
    server: {
      fs: {
        allow: ["../.."]
      },
      host: true,
      port: 6173
    },
    ssr: {
      external: ["@vue/repl"]
    }
  }
});
function head() {
  return [
    ["meta", { content: "Yi.Mini Team", name: "author" }],
    [
      "meta",
      {
        content: "yiabp, abp, .net, vue3, vben5, rbac, \u6743\u9650\u7BA1\u7406",
        name: "keywords"
      }
    ],
    ["link", { href: "/favicon.ico", rel: "icon", type: "image/svg+xml" }],
    [
      "meta",
      {
        content: "width=device-width,initial-scale=1,minimum-scale=1.0,maximum-scale=1.0,user-scalable=no",
        name: "viewport"
      }
    ],
    ["meta", { content: "Yi.Mini \u6587\u6863", name: "keywords" }],
    ["link", { href: "/favicon.ico", rel: "icon" }]
    // [
    //   'script',
    //   {
    //     src: 'https://cdn.tailwindcss.com',
    //   },
    // ],
  ];
}
function pwa() {
  return {
    includeManifestIcons: false,
    manifest: {
      description: "Yi.Mini \u662F\u57FA\u4E8E ABP Framework \u548C Vben5 \u7684\u7CBE\u7B80\u7248 RBAC \u6743\u9650\u7BA1\u7406\u6846\u67B6",
      icons: [
        {
          sizes: "192x192",
          src: "https://unpkg.com/@vbenjs/static-source@0.1.7/source/pwa-icon-192.png",
          type: "image/png"
        },
        {
          sizes: "512x512",
          src: "https://unpkg.com/@vbenjs/static-source@0.1.7/source/pwa-icon-512.png",
          type: "image/png"
        }
      ],
      id: "/",
      name: "Yi.Mini \u6587\u6863",
      short_name: "yi_mini_doc",
      theme_color: "#ffffff"
    },
    outDir: resolve(process.cwd(), ".vitepress/dist"),
    registerType: "autoUpdate",
    workbox: {
      globPatterns: ["**/*.{css,js,html,svg,png,ico,txt,woff2}"],
      maximumFileSizeToCacheInBytes: 5 * 1024 * 1024
    }
  };
}

// .vitepress/config/index.mts
var index_default = withPwa(
  defineConfigWithTheme({
    ...shared,
    ...zh
  })
);
export {
  index_default as default
};
//# sourceMappingURL=data:application/json;base64,ewogICJ2ZXJzaW9uIjogMywKICAic291cmNlcyI6IFsiLnZpdGVwcmVzcy9jb25maWcvaW5kZXgubXRzIiwgIi52aXRlcHJlc3MvY29uZmlnL3NoYXJlZC5tdHMiLCAiLnZpdGVwcmVzcy9jb25maWcvcGx1Z2lucy9kZW1vLXByZXZpZXcudHMiLCAiLnZpdGVwcmVzcy9jb25maWcvemgubXRzIl0sCiAgInNvdXJjZXNDb250ZW50IjogWyJjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZGlybmFtZSA9IFwiRDpcXFxcVXNlcnNcXFxcZGV2ZWxvcFxcXFxZaUFCUFxcXFx5aWFicC1taW5pXFxcXFlpLlZiZW41XFxcXGRvY3NcXFxcLnZpdGVwcmVzc1xcXFxjb25maWdcIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZmlsZW5hbWUgPSBcIkQ6XFxcXFVzZXJzXFxcXGRldmVsb3BcXFxcWWlBQlBcXFxceWlhYnAtbWluaVxcXFxZaS5WYmVuNVxcXFxkb2NzXFxcXC52aXRlcHJlc3NcXFxcY29uZmlnXFxcXGluZGV4Lm10c1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9pbXBvcnRfbWV0YV91cmwgPSBcImZpbGU6Ly8vRDovVXNlcnMvZGV2ZWxvcC9ZaUFCUC95aWFicC1taW5pL1lpLlZiZW41L2RvY3MvLnZpdGVwcmVzcy9jb25maWcvaW5kZXgubXRzXCI7aW1wb3J0IHsgd2l0aFB3YSB9IGZyb20gJ0B2aXRlLXB3YS92aXRlcHJlc3MnO1xuaW1wb3J0IHsgZGVmaW5lQ29uZmlnV2l0aFRoZW1lIH0gZnJvbSAndml0ZXByZXNzJztcblxuaW1wb3J0IHsgc2hhcmVkIH0gZnJvbSAnLi9zaGFyZWQubXRzJztcbmltcG9ydCB7IHpoIH0gZnJvbSAnLi96aC5tdHMnO1xuXG5leHBvcnQgZGVmYXVsdCB3aXRoUHdhKFxuICBkZWZpbmVDb25maWdXaXRoVGhlbWUoe1xuICAgIC4uLnNoYXJlZCxcbiAgICAuLi56aCxcbiAgfSksXG4pO1xuIiwgImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJEOlxcXFxVc2Vyc1xcXFxkZXZlbG9wXFxcXFlpQUJQXFxcXHlpYWJwLW1pbmlcXFxcWWkuVmJlbjVcXFxcZG9jc1xcXFwudml0ZXByZXNzXFxcXGNvbmZpZ1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9maWxlbmFtZSA9IFwiRDpcXFxcVXNlcnNcXFxcZGV2ZWxvcFxcXFxZaUFCUFxcXFx5aWFicC1taW5pXFxcXFlpLlZiZW41XFxcXGRvY3NcXFxcLnZpdGVwcmVzc1xcXFxjb25maWdcXFxcc2hhcmVkLm10c1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9pbXBvcnRfbWV0YV91cmwgPSBcImZpbGU6Ly8vRDovVXNlcnMvZGV2ZWxvcC9ZaUFCUC95aWFicC1taW5pL1lpLlZiZW41L2RvY3MvLnZpdGVwcmVzcy9jb25maWcvc2hhcmVkLm10c1wiO2ltcG9ydCB0eXBlIHsgUHdhT3B0aW9ucyB9IGZyb20gJ0B2aXRlLXB3YS92aXRlcHJlc3MnO1xuaW1wb3J0IHR5cGUgeyBIZWFkQ29uZmlnIH0gZnJvbSAndml0ZXByZXNzJztcblxuaW1wb3J0IHsgcmVzb2x2ZSB9IGZyb20gJ25vZGU6cGF0aCc7XG5cbmltcG9ydCB7XG4gIHZpdGVBcmNoaXZlclBsdWdpbixcbiAgdml0ZVZ4ZVRhYmxlSW1wb3J0c1BsdWdpbixcbn0gZnJvbSAnQHZiZW4vdml0ZS1jb25maWcnO1xuXG5pbXBvcnQge1xuICBHaXRDaGFuZ2Vsb2csXG4gIEdpdENoYW5nZWxvZ01hcmtkb3duU2VjdGlvbixcbn0gZnJvbSAnQG5vbGViYXNlL3ZpdGVwcmVzcy1wbHVnaW4tZ2l0LWNoYW5nZWxvZy92aXRlJztcbmltcG9ydCB0YWlsd2luZCBmcm9tICd0YWlsd2luZGNzcyc7XG5pbXBvcnQgeyBkZWZpbmVDb25maWcsIHBvc3Rjc3NJc29sYXRlU3R5bGVzIH0gZnJvbSAndml0ZXByZXNzJztcbmltcG9ydCB7XG4gIGdyb3VwSWNvbk1kUGx1Z2luLFxuICBncm91cEljb25WaXRlUGx1Z2luLFxufSBmcm9tICd2aXRlcHJlc3MtcGx1Z2luLWdyb3VwLWljb25zJztcblxuaW1wb3J0IHsgZGVtb1ByZXZpZXdQbHVnaW4gfSBmcm9tICcuL3BsdWdpbnMvZGVtby1wcmV2aWV3JztcbmltcG9ydCB7IHNlYXJjaCBhcyB6aFNlYXJjaCB9IGZyb20gJy4vemgubXRzJztcblxuZXhwb3J0IGNvbnN0IHNoYXJlZCA9IGRlZmluZUNvbmZpZyh7XG4gIGFwcGVhcmFuY2U6ICdkYXJrJyxcbiAgaGVhZDogaGVhZCgpLFxuICBtYXJrZG93bjoge1xuICAgIHByZUNvbmZpZyhtZCkge1xuICAgICAgbWQudXNlKGRlbW9QcmV2aWV3UGx1Z2luKTtcbiAgICAgIG1kLnVzZShncm91cEljb25NZFBsdWdpbik7XG4gICAgfSxcbiAgfSxcbiAgcHdhOiBwd2EoKSxcbiAgc3JjRGlyOiAnc3JjJyxcbiAgdGhlbWVDb25maWc6IHtcbiAgICBsb2dvOiAnaHR0cHM6Ly91bnBrZy5jb20vQHZiZW5qcy9zdGF0aWMtc291cmNlQDAuMS43L3NvdXJjZS9sb2dvLXYxLndlYnAnLFxuICAgIHNlYXJjaDoge1xuICAgICAgb3B0aW9uczoge1xuICAgICAgICBsb2NhbGVzOiB7XG4gICAgICAgICAgLi4uemhTZWFyY2gsXG4gICAgICAgIH0sXG4gICAgICB9LFxuICAgICAgcHJvdmlkZXI6ICdsb2NhbCcsXG4gICAgfSxcbiAgICBzaXRlVGl0bGU6ICdZaS5NaW5pJyxcbiAgICBzb2NpYWxMaW5rczogW1xuICAgICAgeyBpY29uOiAnZ2l0ZWUnLCBsaW5rOiAnaHR0cHM6Ly9naXRlZS5jb20vdmljaGVuMjAyMS95aWFicC1taW5pJyB9LFxuICAgIF0sXG4gIH0sXG4gIHRpdGxlOiAnWWkuTWluaScsXG4gIHZpdGU6IHtcbiAgICBidWlsZDoge1xuICAgICAgY2h1bmtTaXplV2FybmluZ0xpbWl0OiBJbmZpbml0eSxcbiAgICAgIG1pbmlmeTogJ3RlcnNlcicsXG4gICAgfSxcbiAgICBjc3M6IHtcbiAgICAgIHBvc3Rjc3M6IHtcbiAgICAgICAgcGx1Z2luczogW1xuICAgICAgICAgIHRhaWx3aW5kKCksXG4gICAgICAgICAgcG9zdGNzc0lzb2xhdGVTdHlsZXMoeyBpbmNsdWRlRmlsZXM6IFsvdnAtZG9jXFwuY3NzL10gfSksXG4gICAgICAgIF0sXG4gICAgICB9LFxuICAgICAgcHJlcHJvY2Vzc29yT3B0aW9uczoge1xuICAgICAgICBzY3NzOiB7XG4gICAgICAgICAgYXBpOiAnbW9kZXJuJyxcbiAgICAgICAgfSxcbiAgICAgIH0sXG4gICAgfSxcbiAgICBqc29uOiB7XG4gICAgICBzdHJpbmdpZnk6IHRydWUsXG4gICAgfSxcbiAgICBwbHVnaW5zOiBbXG4gICAgICBHaXRDaGFuZ2Vsb2coe1xuICAgICAgICBtYXBBdXRob3JzOiBbXSxcbiAgICAgICAgcmVwb1VSTDogKCkgPT4gJ2h0dHBzOi8vZ2l0ZWUuY29tL3ZpY2hlbjIwMjEveWlhYnAtbWluaScsXG4gICAgICB9KSBhcyBhbnksXG4gICAgICBHaXRDaGFuZ2Vsb2dNYXJrZG93blNlY3Rpb24oKSBhcyBhbnksXG4gICAgICB2aXRlQXJjaGl2ZXJQbHVnaW4oeyBvdXRwdXREaXI6ICcudml0ZXByZXNzJyB9KSBhcyBhbnksXG4gICAgICBncm91cEljb25WaXRlUGx1Z2luKCkgYXMgYW55LFxuICAgICAgKGF3YWl0IHZpdGVWeGVUYWJsZUltcG9ydHNQbHVnaW4oKSkgYXMgYW55LFxuICAgIF0sXG4gICAgc2VydmVyOiB7XG4gICAgICBmczoge1xuICAgICAgICBhbGxvdzogWycuLi8uLiddLFxuICAgICAgfSxcbiAgICAgIGhvc3Q6IHRydWUsXG4gICAgICBwb3J0OiA2MTczLFxuICAgIH0sXG5cbiAgICBzc3I6IHtcbiAgICAgIGV4dGVybmFsOiBbJ0B2dWUvcmVwbCddLFxuICAgIH0sXG4gIH0sXG59KTtcblxuZnVuY3Rpb24gaGVhZCgpOiBIZWFkQ29uZmlnW10ge1xuICByZXR1cm4gW1xuICAgIFsnbWV0YScsIHsgY29udGVudDogJ1lpLk1pbmkgVGVhbScsIG5hbWU6ICdhdXRob3InIH1dLFxuICAgIFtcbiAgICAgICdtZXRhJyxcbiAgICAgIHtcbiAgICAgICAgY29udGVudDogJ3lpYWJwLCBhYnAsIC5uZXQsIHZ1ZTMsIHZiZW41LCByYmFjLCBcdTY3NDNcdTk2NTBcdTdCQTFcdTc0MDYnLFxuICAgICAgICBuYW1lOiAna2V5d29yZHMnLFxuICAgICAgfSxcbiAgICBdLFxuICAgIFsnbGluaycsIHsgaHJlZjogJy9mYXZpY29uLmljbycsIHJlbDogJ2ljb24nLCB0eXBlOiAnaW1hZ2Uvc3ZnK3htbCcgfV0sXG4gICAgW1xuICAgICAgJ21ldGEnLFxuICAgICAge1xuICAgICAgICBjb250ZW50OlxuICAgICAgICAgICd3aWR0aD1kZXZpY2Utd2lkdGgsaW5pdGlhbC1zY2FsZT0xLG1pbmltdW0tc2NhbGU9MS4wLG1heGltdW0tc2NhbGU9MS4wLHVzZXItc2NhbGFibGU9bm8nLFxuICAgICAgICBuYW1lOiAndmlld3BvcnQnLFxuICAgICAgfSxcbiAgICBdLFxuICAgIFsnbWV0YScsIHsgY29udGVudDogJ1lpLk1pbmkgXHU2NTg3XHU2ODYzJywgbmFtZTogJ2tleXdvcmRzJyB9XSxcbiAgICBbJ2xpbmsnLCB7IGhyZWY6ICcvZmF2aWNvbi5pY28nLCByZWw6ICdpY29uJyB9XSxcbiAgICAvLyBbXG4gICAgLy8gICAnc2NyaXB0JyxcbiAgICAvLyAgIHtcbiAgICAvLyAgICAgc3JjOiAnaHR0cHM6Ly9jZG4udGFpbHdpbmRjc3MuY29tJyxcbiAgICAvLyAgIH0sXG4gICAgLy8gXSxcbiAgXTtcbn1cblxuZnVuY3Rpb24gcHdhKCk6IFB3YU9wdGlvbnMge1xuICByZXR1cm4ge1xuICAgIGluY2x1ZGVNYW5pZmVzdEljb25zOiBmYWxzZSxcbiAgICBtYW5pZmVzdDoge1xuICAgICAgZGVzY3JpcHRpb246XG4gICAgICAgICdZaS5NaW5pIFx1NjYyRlx1NTdGQVx1NEU4RSBBQlAgRnJhbWV3b3JrIFx1NTQ4QyBWYmVuNSBcdTc2ODRcdTdDQkVcdTdCODBcdTcyNDggUkJBQyBcdTY3NDNcdTk2NTBcdTdCQTFcdTc0MDZcdTY4NDZcdTY3QjYnLFxuICAgICAgaWNvbnM6IFtcbiAgICAgICAge1xuICAgICAgICAgIHNpemVzOiAnMTkyeDE5MicsXG4gICAgICAgICAgc3JjOiAnaHR0cHM6Ly91bnBrZy5jb20vQHZiZW5qcy9zdGF0aWMtc291cmNlQDAuMS43L3NvdXJjZS9wd2EtaWNvbi0xOTIucG5nJyxcbiAgICAgICAgICB0eXBlOiAnaW1hZ2UvcG5nJyxcbiAgICAgICAgfSxcbiAgICAgICAge1xuICAgICAgICAgIHNpemVzOiAnNTEyeDUxMicsXG4gICAgICAgICAgc3JjOiAnaHR0cHM6Ly91bnBrZy5jb20vQHZiZW5qcy9zdGF0aWMtc291cmNlQDAuMS43L3NvdXJjZS9wd2EtaWNvbi01MTIucG5nJyxcbiAgICAgICAgICB0eXBlOiAnaW1hZ2UvcG5nJyxcbiAgICAgICAgfSxcbiAgICAgIF0sXG4gICAgICBpZDogJy8nLFxuICAgICAgbmFtZTogJ1lpLk1pbmkgXHU2NTg3XHU2ODYzJyxcbiAgICAgIHNob3J0X25hbWU6ICd5aV9taW5pX2RvYycsXG4gICAgICB0aGVtZV9jb2xvcjogJyNmZmZmZmYnLFxuICAgIH0sXG4gICAgb3V0RGlyOiByZXNvbHZlKHByb2Nlc3MuY3dkKCksICcudml0ZXByZXNzL2Rpc3QnKSxcbiAgICByZWdpc3RlclR5cGU6ICdhdXRvVXBkYXRlJyxcbiAgICB3b3JrYm94OiB7XG4gICAgICBnbG9iUGF0dGVybnM6IFsnKiovKi57Y3NzLGpzLGh0bWwsc3ZnLHBuZyxpY28sdHh0LHdvZmYyfSddLFxuICAgICAgbWF4aW11bUZpbGVTaXplVG9DYWNoZUluQnl0ZXM6IDUgKiAxMDI0ICogMTAyNCxcbiAgICB9LFxuICB9O1xufVxuIiwgImNvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9kaXJuYW1lID0gXCJEOlxcXFxVc2Vyc1xcXFxkZXZlbG9wXFxcXFlpQUJQXFxcXHlpYWJwLW1pbmlcXFxcWWkuVmJlbjVcXFxcZG9jc1xcXFwudml0ZXByZXNzXFxcXGNvbmZpZ1xcXFxwbHVnaW5zXCI7Y29uc3QgX192aXRlX2luamVjdGVkX29yaWdpbmFsX2ZpbGVuYW1lID0gXCJEOlxcXFxVc2Vyc1xcXFxkZXZlbG9wXFxcXFlpQUJQXFxcXHlpYWJwLW1pbmlcXFxcWWkuVmJlbjVcXFxcZG9jc1xcXFwudml0ZXByZXNzXFxcXGNvbmZpZ1xcXFxwbHVnaW5zXFxcXGRlbW8tcHJldmlldy50c1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9pbXBvcnRfbWV0YV91cmwgPSBcImZpbGU6Ly8vRDovVXNlcnMvZGV2ZWxvcC9ZaUFCUC95aWFicC1taW5pL1lpLlZiZW41L2RvY3MvLnZpdGVwcmVzcy9jb25maWcvcGx1Z2lucy9kZW1vLXByZXZpZXcudHNcIjtpbXBvcnQgdHlwZSB7IE1hcmtkb3duRW52LCBNYXJrZG93blJlbmRlcmVyIH0gZnJvbSAndml0ZXByZXNzJztcblxuaW1wb3J0IGNyeXB0byBmcm9tICdub2RlOmNyeXB0byc7XG5pbXBvcnQgeyByZWFkZGlyU3luYyB9IGZyb20gJ25vZGU6ZnMnO1xuaW1wb3J0IHsgam9pbiB9IGZyb20gJ25vZGU6cGF0aCc7XG5cbmV4cG9ydCBjb25zdCByYXdQYXRoUmVnZXhwID1cbiAgLy8gZXNsaW50LWRpc2FibGUtbmV4dC1saW5lIHJlZ2V4cC9uby1zdXBlci1saW5lYXItYmFja3RyYWNraW5nLCByZWdleHAvc3RyaWN0XG4gIC9eKC4rPyg/OlxcLihbXFxkYS16XSspKT8pKCNbXFx3LV0rKT8oPzogP3soXFxkKyg/OlssLV1cXGQrKSopPyA/KFxcUyspP30pPyA/KD86XFxbKC4rKV0pPyQvO1xuXG5mdW5jdGlvbiByYXdQYXRoVG9Ub2tlbihyYXdQYXRoOiBzdHJpbmcpIHtcbiAgY29uc3QgW1xuICAgIGZpbGVwYXRoID0gJycsXG4gICAgZXh0ZW5zaW9uID0gJycsXG4gICAgcmVnaW9uID0gJycsXG4gICAgbGluZXMgPSAnJyxcbiAgICBsYW5nID0gJycsXG4gICAgcmF3VGl0bGUgPSAnJyxcbiAgXSA9IChyYXdQYXRoUmVnZXhwLmV4ZWMocmF3UGF0aCkgfHwgW10pLnNsaWNlKDEpO1xuXG4gIGNvbnN0IHRpdGxlID0gcmF3VGl0bGUgfHwgZmlsZXBhdGguc3BsaXQoJy8nKS5wb3AoKSB8fCAnJztcblxuICByZXR1cm4geyBleHRlbnNpb24sIGZpbGVwYXRoLCBsYW5nLCBsaW5lcywgcmVnaW9uLCB0aXRsZSB9O1xufVxuXG5leHBvcnQgY29uc3QgZGVtb1ByZXZpZXdQbHVnaW4gPSAobWQ6IE1hcmtkb3duUmVuZGVyZXIpID0+IHtcbiAgbWQuY29yZS5ydWxlci5hZnRlcignaW5saW5lJywgJ2RlbW8tcHJldmlldycsIChzdGF0ZSkgPT4ge1xuICAgIGNvbnN0IGluc2VydENvbXBvbmVudEltcG9ydCA9IChpbXBvcnRTdHJpbmc6IHN0cmluZykgPT4ge1xuICAgICAgY29uc3QgaW5kZXggPSBzdGF0ZS50b2tlbnMuZmluZEluZGV4KFxuICAgICAgICAoaSkgPT4gaS50eXBlID09PSAnaHRtbF9ibG9jaycgJiYgaS5jb250ZW50Lm1hdGNoKC88c2NyaXB0IHNldHVwPi9nKSxcbiAgICAgICk7XG4gICAgICBpZiAoaW5kZXggPT09IC0xKSB7XG4gICAgICAgIGNvbnN0IGltcG9ydENvbXBvbmVudCA9IG5ldyBzdGF0ZS5Ub2tlbignaHRtbF9ibG9jaycsICcnLCAwKTtcbiAgICAgICAgaW1wb3J0Q29tcG9uZW50LmNvbnRlbnQgPSBgPHNjcmlwdCBzZXR1cD5cXG4ke2ltcG9ydFN0cmluZ31cXG48L3NjcmlwdD5cXG5gO1xuICAgICAgICBzdGF0ZS50b2tlbnMuc3BsaWNlKDAsIDAsIGltcG9ydENvbXBvbmVudCk7XG4gICAgICB9IGVsc2Uge1xuICAgICAgICBpZiAoc3RhdGUudG9rZW5zW2luZGV4XSkge1xuICAgICAgICAgIGNvbnN0IGNvbnRlbnQgPSBzdGF0ZS50b2tlbnNbaW5kZXhdLmNvbnRlbnQ7XG4gICAgICAgICAgc3RhdGUudG9rZW5zW2luZGV4XS5jb250ZW50ID0gY29udGVudC5yZXBsYWNlKFxuICAgICAgICAgICAgJzwvc2NyaXB0PicsXG4gICAgICAgICAgICBgJHtpbXBvcnRTdHJpbmd9XFxuPC9zY3JpcHQ+YCxcbiAgICAgICAgICApO1xuICAgICAgICB9XG4gICAgICB9XG4gICAgfTtcbiAgICAvLyBEZWZpbmUgdGhlIHJlZ3VsYXIgZXhwcmVzc2lvbiB0byBtYXRjaCB0aGUgZGVzaXJlZCBwYXR0ZXJuXG4gICAgY29uc3QgcmVnZXggPSAvPERlbW9QcmV2aWV3W14+XSpcXHNkaXI9XCIoW15cIl0qKVwiL2c7XG4gICAgLy8gSXRlcmF0ZSB0aHJvdWdoIHRoZSBNYXJrZG93biBjb250ZW50IGFuZCByZXBsYWNlIHRoZSBwYXR0ZXJuXG4gICAgc3RhdGUuc3JjID0gc3RhdGUuc3JjLnJlcGxhY2VBbGwocmVnZXgsIChfbWF0Y2gsIGRpcikgPT4ge1xuICAgICAgY29uc3QgY29tcG9uZW50RGlyID0gam9pbihwcm9jZXNzLmN3ZCgpLCAnc3JjJywgZGlyKS5yZXBsYWNlQWxsKFxuICAgICAgICAnXFxcXCcsXG4gICAgICAgICcvJyxcbiAgICAgICk7XG5cbiAgICAgIGxldCBjaGlsZEZpbGVzOiBzdHJpbmdbXSA9IFtdO1xuICAgICAgbGV0IGRpckV4aXN0cyA9IHRydWU7XG5cbiAgICAgIHRyeSB7XG4gICAgICAgIGNoaWxkRmlsZXMgPVxuICAgICAgICAgIHJlYWRkaXJTeW5jKGNvbXBvbmVudERpciwge1xuICAgICAgICAgICAgZW5jb2Rpbmc6ICd1dGY4JyxcbiAgICAgICAgICAgIHJlY3Vyc2l2ZTogZmFsc2UsXG4gICAgICAgICAgICB3aXRoRmlsZVR5cGVzOiBmYWxzZSxcbiAgICAgICAgICB9KSB8fCBbXTtcbiAgICAgIH0gY2F0Y2gge1xuICAgICAgICBkaXJFeGlzdHMgPSBmYWxzZTtcbiAgICAgIH1cblxuICAgICAgaWYgKCFkaXJFeGlzdHMpIHtcbiAgICAgICAgcmV0dXJuICcnO1xuICAgICAgfVxuXG4gICAgICBjb25zdCB1bmlxdWVXb3JkID0gZ2VuZXJhdGVDb250ZW50SGFzaChjb21wb25lbnREaXIpO1xuXG4gICAgICBjb25zdCBDb21wb25lbnROYW1lID0gYERlbW9Db21wb25lbnRfJHt1bmlxdWVXb3JkfWA7XG4gICAgICBpbnNlcnRDb21wb25lbnRJbXBvcnQoXG4gICAgICAgIGBpbXBvcnQgJHtDb21wb25lbnROYW1lfSBmcm9tICcke2NvbXBvbmVudERpcn0vaW5kZXgudnVlJ2AsXG4gICAgICApO1xuICAgICAgY29uc3QgeyBwYXRoOiBfcGF0aCB9ID0gc3RhdGUuZW52IGFzIE1hcmtkb3duRW52O1xuXG4gICAgICBjb25zdCBpbmRleCA9IHN0YXRlLnRva2Vucy5maW5kSW5kZXgoKGkpID0+IGkuY29udGVudC5tYXRjaChyZWdleCkpO1xuXG4gICAgICBpZiAoIXN0YXRlLnRva2Vuc1tpbmRleF0pIHtcbiAgICAgICAgcmV0dXJuICcnO1xuICAgICAgfVxuICAgICAgY29uc3QgZmlyc3RTdHJpbmcgPSAnaW5kZXgudnVlJztcbiAgICAgIGNoaWxkRmlsZXMgPSBjaGlsZEZpbGVzLnNvcnQoKGEsIGIpID0+IHtcbiAgICAgICAgaWYgKGEgPT09IGZpcnN0U3RyaW5nKSByZXR1cm4gLTE7XG4gICAgICAgIGlmIChiID09PSBmaXJzdFN0cmluZykgcmV0dXJuIDE7XG4gICAgICAgIHJldHVybiBhLmxvY2FsZUNvbXBhcmUoYiwgJ2VuJywgeyBzZW5zaXRpdml0eTogJ2Jhc2UnIH0pO1xuICAgICAgfSk7XG4gICAgICBzdGF0ZS50b2tlbnNbaW5kZXhdLmNvbnRlbnQgPVxuICAgICAgICBgPERlbW9QcmV2aWV3IGZpbGVzPVwiJHtlbmNvZGVVUklDb21wb25lbnQoSlNPTi5zdHJpbmdpZnkoY2hpbGRGaWxlcykpfVwiID48JHtDb21wb25lbnROYW1lfS8+XG4gICAgICAgIGA7XG5cbiAgICAgIGNvbnN0IF9kdW1teVRva2VuID0gbmV3IHN0YXRlLlRva2VuKCcnLCAnJywgMCk7XG4gICAgICBjb25zdCB0b2tlbkFycmF5OiBBcnJheTx0eXBlb2YgX2R1bW15VG9rZW4+ID0gW107XG4gICAgICBjaGlsZEZpbGVzLmZvckVhY2goKGZpbGVuYW1lKSA9PiB7XG4gICAgICAgIC8vIGNvbnN0IHNsb3ROYW1lID0gZmlsZW5hbWUucmVwbGFjZShleHRuYW1lKGZpbGVuYW1lKSwgJycpO1xuXG4gICAgICAgIGNvbnN0IHRlbXBsYXRlU3RhcnQgPSBuZXcgc3RhdGUuVG9rZW4oJ2h0bWxfaW5saW5lJywgJycsIDApO1xuICAgICAgICB0ZW1wbGF0ZVN0YXJ0LmNvbnRlbnQgPSBgPHRlbXBsYXRlICMke2ZpbGVuYW1lfT5gO1xuICAgICAgICB0b2tlbkFycmF5LnB1c2godGVtcGxhdGVTdGFydCk7XG5cbiAgICAgICAgY29uc3QgcmVzb2x2ZWRQYXRoID0gam9pbihjb21wb25lbnREaXIsIGZpbGVuYW1lKTtcblxuICAgICAgICBjb25zdCB7IGV4dGVuc2lvbiwgZmlsZXBhdGgsIGxhbmcsIGxpbmVzLCB0aXRsZSB9ID1cbiAgICAgICAgICByYXdQYXRoVG9Ub2tlbihyZXNvbHZlZFBhdGgpO1xuICAgICAgICAvLyBBZGQgY29kZSB0b2tlbnMgZm9yIGVhY2ggbGluZVxuICAgICAgICBjb25zdCB0b2tlbiA9IG5ldyBzdGF0ZS5Ub2tlbignZmVuY2UnLCAnY29kZScsIDApO1xuICAgICAgICB0b2tlbi5pbmZvID0gYCR7bGFuZyB8fCBleHRlbnNpb259JHtsaW5lcyA/IGB7JHtsaW5lc319YCA6ICcnfSR7XG4gICAgICAgICAgdGl0bGUgPyBgWyR7dGl0bGV9XWAgOiAnJ1xuICAgICAgICB9YDtcblxuICAgICAgICB0b2tlbi5jb250ZW50ID0gYDw8PCAke2ZpbGVwYXRofWA7XG4gICAgICAgICh0b2tlbiBhcyBhbnkpLnNyYyA9IFtyZXNvbHZlZFBhdGhdO1xuICAgICAgICB0b2tlbkFycmF5LnB1c2godG9rZW4pO1xuXG4gICAgICAgIGNvbnN0IHRlbXBsYXRlRW5kID0gbmV3IHN0YXRlLlRva2VuKCdodG1sX2lubGluZScsICcnLCAwKTtcbiAgICAgICAgdGVtcGxhdGVFbmQuY29udGVudCA9ICc8L3RlbXBsYXRlPic7XG4gICAgICAgIHRva2VuQXJyYXkucHVzaCh0ZW1wbGF0ZUVuZCk7XG4gICAgICB9KTtcbiAgICAgIGNvbnN0IGVuZFRhZyA9IG5ldyBzdGF0ZS5Ub2tlbignaHRtbF9pbmxpbmUnLCAnJywgMCk7XG4gICAgICBlbmRUYWcuY29udGVudCA9ICc8L0RlbW9QcmV2aWV3Pic7XG4gICAgICB0b2tlbkFycmF5LnB1c2goZW5kVGFnKTtcblxuICAgICAgc3RhdGUudG9rZW5zLnNwbGljZShpbmRleCArIDEsIDAsIC4uLnRva2VuQXJyYXkpO1xuXG4gICAgICAvLyBjb25zb2xlLmxvZyhcbiAgICAgIC8vICAgc3RhdGUubWQucmVuZGVyZXIucmVuZGVyKHN0YXRlLnRva2Vucywgc3RhdGU/Lm9wdGlvbnMgPz8gW10sIHN0YXRlLmVudiksXG4gICAgICAvLyApO1xuICAgICAgcmV0dXJuICcnO1xuICAgIH0pO1xuICB9KTtcbn07XG5cbmZ1bmN0aW9uIGdlbmVyYXRlQ29udGVudEhhc2goaW5wdXQ6IHN0cmluZywgbGVuZ3RoOiBudW1iZXIgPSAxMCk6IHN0cmluZyB7XG4gIC8vIFx1NEY3Rlx1NzUyOCBTSEEtMjU2IFx1NzUxRlx1NjIxMFx1NTRDOFx1NUUwQ1x1NTAzQ1xuICBjb25zdCBoYXNoID0gY3J5cHRvLmNyZWF0ZUhhc2goJ3NoYTI1NicpLnVwZGF0ZShpbnB1dCkuZGlnZXN0KCdoZXgnKTtcblxuICAvLyBcdTVDMDZcdTU0QzhcdTVFMENcdTUwM0NcdThGNkNcdTYzNjJcdTRFM0EgQmFzZTM2IFx1N0YxNlx1NzgwMVx1RkYwQ1x1NUU3Nlx1NTNENlx1NjMwN1x1NUI5QVx1OTU3Rlx1NUVBNlx1NzY4NFx1NUI1N1x1N0IyNlx1NEY1Q1x1NEUzQVx1N0VEM1x1Njc5Q1xuICByZXR1cm4gTnVtYmVyLnBhcnNlSW50KGhhc2gsIDE2KS50b1N0cmluZygzNikuc2xpY2UoMCwgbGVuZ3RoKTtcbn1cbiIsICJjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZGlybmFtZSA9IFwiRDpcXFxcVXNlcnNcXFxcZGV2ZWxvcFxcXFxZaUFCUFxcXFx5aWFicC1taW5pXFxcXFlpLlZiZW41XFxcXGRvY3NcXFxcLnZpdGVwcmVzc1xcXFxjb25maWdcIjtjb25zdCBfX3ZpdGVfaW5qZWN0ZWRfb3JpZ2luYWxfZmlsZW5hbWUgPSBcIkQ6XFxcXFVzZXJzXFxcXGRldmVsb3BcXFxcWWlBQlBcXFxceWlhYnAtbWluaVxcXFxZaS5WYmVuNVxcXFxkb2NzXFxcXC52aXRlcHJlc3NcXFxcY29uZmlnXFxcXHpoLm10c1wiO2NvbnN0IF9fdml0ZV9pbmplY3RlZF9vcmlnaW5hbF9pbXBvcnRfbWV0YV91cmwgPSBcImZpbGU6Ly8vRDovVXNlcnMvZGV2ZWxvcC9ZaUFCUC95aWFicC1taW5pL1lpLlZiZW41L2RvY3MvLnZpdGVwcmVzcy9jb25maWcvemgubXRzXCI7aW1wb3J0IHR5cGUgeyBEZWZhdWx0VGhlbWUgfSBmcm9tICd2aXRlcHJlc3MnO1xuXG5pbXBvcnQgeyBkZWZpbmVDb25maWcgfSBmcm9tICd2aXRlcHJlc3MnO1xuXG5pbXBvcnQgeyB2ZXJzaW9uIH0gZnJvbSAnLi4vLi4vLi4vcGFja2FnZS5qc29uJztcblxuZXhwb3J0IGNvbnN0IHpoID0gZGVmaW5lQ29uZmlnKHtcbiAgZGVzY3JpcHRpb246ICdZaS5NaW5pIC0gXHU1N0ZBXHU0RThFIEFCUCBGcmFtZXdvcmsgXHU1NDhDIFZiZW41IFx1NzY4NFx1N0NCRVx1N0I4MFx1NzI0OCBSQkFDIFx1Njc0M1x1OTY1MFx1N0JBMVx1NzQwNlx1Njg0Nlx1NjdCNicsXG4gIGxhbmc6ICd6aC1IYW5zJyxcbiAgdGhlbWVDb25maWc6IHtcbiAgICBkYXJrTW9kZVN3aXRjaExhYmVsOiAnXHU0RTNCXHU5ODk4JyxcbiAgICBkYXJrTW9kZVN3aXRjaFRpdGxlOiAnXHU1MjA3XHU2MzYyXHU1MjMwXHU2REYxXHU4MjcyXHU2QTIxXHU1RjBGJyxcbiAgICBkb2NGb290ZXI6IHtcbiAgICAgIG5leHQ6ICdcdTRFMEJcdTRFMDBcdTk4NzUnLFxuICAgICAgcHJldjogJ1x1NEUwQVx1NEUwMFx1OTg3NScsXG4gICAgfSxcbiAgICBlZGl0TGluazoge1xuICAgICAgcGF0dGVybjpcbiAgICAgICAgJ2h0dHBzOi8vZ2l0ZWUuY29tL3ZpY2hlbjIwMjEveWlhYnAtbWluaS9lZGl0L21haW4vWWkuVmJlbjUvZG9jcy9zcmMvOnBhdGgnLFxuICAgICAgdGV4dDogJ1x1NTcyOCBHaXRlZSBcdTRFMEFcdTdGMTZcdThGOTFcdTZCNjRcdTk4NzVcdTk3NjInLFxuICAgIH0sXG4gICAgZm9vdGVyOiB7XG4gICAgICBjb3B5cmlnaHQ6IGBDb3B5cmlnaHQgXHUwMEE5ICR7bmV3IERhdGUoKS5nZXRGdWxsWWVhcigpfSBZaS5NaW5pYCxcbiAgICAgIG1lc3NhZ2U6ICdcdTU3RkFcdTRFOEUgTUlUIFx1OEJCOFx1NTNFRlx1NTNEMVx1NUUwMy4nLFxuICAgIH0sXG4gICAgbGFuZ01lbnVMYWJlbDogJ1x1NTkxQVx1OEJFRFx1OEEwMCcsXG4gICAgbGFzdFVwZGF0ZWQ6IHtcbiAgICAgIGZvcm1hdE9wdGlvbnM6IHtcbiAgICAgICAgZGF0ZVN0eWxlOiAnc2hvcnQnLFxuICAgICAgICB0aW1lU3R5bGU6ICdtZWRpdW0nLFxuICAgICAgfSxcbiAgICAgIHRleHQ6ICdcdTY3MDBcdTU0MEVcdTY2RjRcdTY1QjBcdTRFOEUnLFxuICAgIH0sXG4gICAgbGlnaHRNb2RlU3dpdGNoVGl0bGU6ICdcdTUyMDdcdTYzNjJcdTUyMzBcdTZENDVcdTgyNzJcdTZBMjFcdTVGMEYnLFxuICAgIG5hdjogbmF2KCksXG5cbiAgICBvdXRsaW5lOiB7XG4gICAgICBsYWJlbDogJ1x1OTg3NVx1OTc2Mlx1NUJGQ1x1ODIyQScsXG4gICAgfSxcbiAgICByZXR1cm5Ub1RvcExhYmVsOiAnXHU1NkRFXHU1MjMwXHU5ODc2XHU5MEU4JyxcblxuICAgIHNpZGViYXI6IHtcbiAgICAgICcvY29tbWVyY2lhbC8nOiB7IGJhc2U6ICcvY29tbWVyY2lhbC8nLCBpdGVtczogc2lkZWJhckNvbW1lcmNpYWwoKSB9LFxuICAgICAgJy9jb21wb25lbnRzLyc6IHsgYmFzZTogJy9jb21wb25lbnRzLycsIGl0ZW1zOiBzaWRlYmFyQ29tcG9uZW50cygpIH0sXG4gICAgICAnL2d1aWRlLyc6IHsgYmFzZTogJy9ndWlkZS8nLCBpdGVtczogc2lkZWJhckd1aWRlKCkgfSxcbiAgICB9LFxuICAgIHNpZGViYXJNZW51TGFiZWw6ICdcdTgzRENcdTUzNTUnLFxuICB9LFxufSk7XG5cbmZ1bmN0aW9uIHNpZGViYXJHdWlkZSgpOiBEZWZhdWx0VGhlbWUuU2lkZWJhckl0ZW1bXSB7XG4gIHJldHVybiBbXG4gICAge1xuICAgICAgY29sbGFwc2VkOiBmYWxzZSxcbiAgICAgIHRleHQ6ICdcdTdCODBcdTRFQ0InLFxuICAgICAgaXRlbXM6IFtcbiAgICAgICAge1xuICAgICAgICAgIGxpbms6ICdpbnRyb2R1Y3Rpb24vYWJvdXQnLFxuICAgICAgICAgIHRleHQ6ICdcdTUxNzNcdTRFOEUgWWkuTWluaScsXG4gICAgICAgIH0sXG4gICAgICAgIHtcbiAgICAgICAgICBsaW5rOiAnaW50cm9kdWN0aW9uL3F1aWNrLXN0YXJ0JyxcbiAgICAgICAgICB0ZXh0OiAnXHU1RkVCXHU5MDFGXHU1RjAwXHU1OUNCJyxcbiAgICAgICAgfSxcbiAgICAgICAge1xuICAgICAgICAgIGxpbms6ICdpbnRyb2R1Y3Rpb24vZmVhdHVyZXMnLFxuICAgICAgICAgIHRleHQ6ICdcdTY4MzhcdTVGQzNcdTcyNzlcdTYwMjcnLFxuICAgICAgICB9LFxuICAgICAgICB7XG4gICAgICAgICAgbGluazogJ2ludHJvZHVjdGlvbi9haS1kZXZlbG9wbWVudCcsXG4gICAgICAgICAgdGV4dDogJ1x1NTE2OFx1NjgwOCtBSVx1NUYwMFx1NTNEMVx1NjMwN1x1NTM1NycsXG4gICAgICAgIH0sXG4gICAgICBdLFxuICAgIH0sXG4gICAge1xuICAgICAgdGV4dDogJ1x1NTQwRVx1N0FFRlx1NUYwMFx1NTNEMScsXG4gICAgICBpdGVtczogW1xuICAgICAgICB7IGxpbms6ICdiYWNrZW5kL3RlY2gtc3RhY2snLCB0ZXh0OiAnXHU1NDBFXHU3QUVGXHU2MjgwXHU2NzJGXHU2ODA4JyB9LFxuICAgICAgICB7IGxpbms6ICdiYWNrZW5kL3N0YXJ0dXAnLCB0ZXh0OiAnXHU1NDJGXHU1MkE4XHU5ODc5XHU3NkVFJyB9LFxuICAgICAgICB7IGxpbms6ICdiYWNrZW5kL2FyY2hpdGVjdHVyZScsIHRleHQ6ICdcdTY3QjZcdTY3ODRcdThCQkVcdThCQTEnIH0sXG4gICAgICAgIHsgbGluazogJ2JhY2tlbmQvbmFtaW5nJywgdGV4dDogJ1x1NTQ3RFx1NTQwRFx1ODlDNFx1ODMwMycgfSxcbiAgICAgICAgeyBsaW5rOiAnYmFja2VuZC9lbnRpdHknLCB0ZXh0OiAnXHU1QjlFXHU0RjUzXHU1QjlBXHU0RTQ5JyB9LFxuICAgICAgICB7IGxpbms6ICdiYWNrZW5kL2VudW0nLCB0ZXh0OiAnXHU2NzlBXHU0RTNFL1x1N0NGQlx1N0VERlx1NUI1N1x1NTE3OCcgfSxcbiAgICAgICAgeyBsaW5rOiAnYmFja2VuZC9zZXJ2aWNlJywgdGV4dDogJ1x1NUU5NFx1NzUyOFx1NjcwRFx1NTJBMScgfSxcbiAgICAgICAgeyBsaW5rOiAnYmFja2VuZC9hcGknLCB0ZXh0OiAnQVBJIFx1NkEyMVx1NUYwRicgfSxcbiAgICAgICAgeyBsaW5rOiAnYmFja2VuZC9wZXJtaXNzaW9uJywgdGV4dDogJ1x1Njc0M1x1OTY1MFx1NEUwRVx1NjVFNVx1NUZENycgfSxcbiAgICAgICAgeyBsaW5rOiAnYmFja2VuZC9xdWVyeScsIHRleHQ6ICdcdTY3RTVcdThCRTJcdTZBMjFcdTVGMEYnIH0sXG4gICAgICAgIHsgbGluazogJ2JhY2tlbmQvbW9kdWxlJywgdGV4dDogJ1x1NkEyMVx1NTc1N1x1NUYwMFx1NTNEMScgfSxcbiAgICAgIF0sXG4gICAgfSxcbiAgICB7XG4gICAgICB0ZXh0OiAnXHU1MjREXHU3QUVGXHU1RjAwXHU1M0QxJyxcbiAgICAgIGl0ZW1zOiBbXG4gICAgICAgIHsgbGluazogJ2Zyb250ZW5kL3RlY2gtc3RhY2snLCB0ZXh0OiAnXHU1MjREXHU3QUVGXHU2MjgwXHU2NzJGXHU2ODA4JyB9LFxuICAgICAgICB7IGxpbms6ICdmcm9udGVuZC9kZXZlbG9wbWVudCcsIHRleHQ6ICdcdTY3MkNcdTU3MzBcdTVGMDBcdTUzRDEnIH0sXG4gICAgICAgIHsgbGluazogJ2Zyb250ZW5kL3JvdXRlJywgdGV4dDogJ1x1OERFRlx1NzUzMVx1NTQ4Q1x1ODNEQ1x1NTM1NScgfSxcbiAgICAgICAgeyBsaW5rOiAnZnJvbnRlbmQvYXBpJywgdGV4dDogJ0FQSSBcdThDMDNcdTc1MjgnIH0sXG4gICAgICAgIHsgbGluazogJ2Zyb250ZW5kL2NvbXBvbmVudHMnLCB0ZXh0OiAnXHU3RUM0XHU0RUY2XHU0RjdGXHU3NTI4JyB9LFxuICAgICAgICB7IGxpbms6ICdmcm9udGVuZC9idWlsZCcsIHRleHQ6ICdcdTY3ODRcdTVFRkFcdTRFMEVcdTkwRThcdTdGNzInIH0sXG4gICAgICBdLFxuICAgIH0sXG4gICAge1xuICAgICAgdGV4dDogJ0NsYXVkZSBTa2lsbHMnLFxuICAgICAgaXRlbXM6IFtcbiAgICAgICAgeyBsaW5rOiAnc2tpbGxzL3doYXQtaXMtc2tpbGwnLCB0ZXh0OiAnXHU0RUMwXHU0RTQ4XHU2NjJGIFNraWxsXHVGRjFGJyB9LFxuICAgICAgICB7IGxpbms6ICdza2lsbHMvbW9kdWxlLWdlbmVyYXRvcicsIHRleHQ6ICdcdTZBMjFcdTU3NTdcdTc1MUZcdTYyMTBcdTU2NjgnIH0sXG4gICAgICAgIHsgbGluazogJ3NraWxscy9jcnVkLWdlbmVyYXRvcicsIHRleHQ6ICdDUlVEIFx1NzUxRlx1NjIxMFx1NTY2OCcgfSxcbiAgICAgICAgeyBsaW5rOiAnc2tpbGxzL2ZpZWxkLXN5bmMnLCB0ZXh0OiAnXHU1QjU3XHU2QkI1XHU1NDBDXHU2QjY1XHU1NjY4JyB9LFxuICAgICAgICB7IGxpbms6ICdza2lsbHMvc2tpbGwtY3JlYXRvcicsIHRleHQ6ICdcdTYyODBcdTgwRkRcdTUyMUJcdTVFRkFcdTU2NjgnIH0sXG4gICAgICBdLFxuICAgIH0sXG4gICAge1xuICAgICAgdGV4dDogJ1x1NUYwMFx1NTNEMVx1ODlDNFx1ODMwMycsXG4gICAgICBpdGVtczogW1xuICAgICAgICB7IGxpbms6ICdzdGFuZGFyZHMvY29kaW5nJywgdGV4dDogJ1x1N0YxNlx1NzgwMVx1ODlDNFx1ODMwMycgfSxcbiAgICAgICAgeyBsaW5rOiAnc3RhbmRhcmRzL2dpdCcsIHRleHQ6ICdHaXQgXHU2M0QwXHU0RUE0XHU4OUM0XHU4MzAzJyB9LFxuICAgICAgICB7IGxpbms6ICdzdGFuZGFyZHMvdGVzdGluZycsIHRleHQ6ICdcdTZENEJcdThCRDVcdTg5QzRcdTgzMDMnIH0sXG4gICAgICBdLFxuICAgIH0sXG4gICAge1xuICAgICAgdGV4dDogJ1x1NTE3Nlx1NEVENicsXG4gICAgICBpdGVtczogW1xuICAgICAgICB7IGxpbms6ICdvdGhlci9mYXEnLCB0ZXh0OiAnXHU1RTM4XHU4OUMxXHU5NUVFXHU5ODk4JyB9LFxuICAgICAgICB7IGxpbms6ICdvdGhlci9jb250YWN0JywgdGV4dDogJ1x1ODA1NFx1N0NGQlx1NjVCOVx1NUYwRicgfSxcbiAgICAgIF0sXG4gICAgfSxcbiAgXTtcbn1cblxuZnVuY3Rpb24gc2lkZWJhckNvbW1lcmNpYWwoKTogRGVmYXVsdFRoZW1lLlNpZGViYXJJdGVtW10ge1xuICByZXR1cm4gW1xuICAgIHtcbiAgICAgIGxpbms6ICdjb21tdW5pdHknLFxuICAgICAgdGV4dDogJ1x1NEVBNFx1NkQ0MVx1N0ZBNCcsXG4gICAgfSxcbiAgICB7XG4gICAgICBsaW5rOiAndGVjaG5pY2FsLXN1cHBvcnQnLFxuICAgICAgdGV4dDogJ1x1NjI4MFx1NjcyRlx1NjUyRlx1NjMwMScsXG4gICAgfSxcbiAgICB7XG4gICAgICBsaW5rOiAnY3VzdG9taXplZCcsXG4gICAgICB0ZXh0OiAnXHU1QjlBXHU1MjM2XHU1RjAwXHU1M0QxJyxcbiAgICB9LFxuICBdO1xufVxuXG5mdW5jdGlvbiBzaWRlYmFyQ29tcG9uZW50cygpOiBEZWZhdWx0VGhlbWUuU2lkZWJhckl0ZW1bXSB7XG4gIHJldHVybiBbXG4gICAge1xuICAgICAgdGV4dDogJ1x1N0VDNFx1NEVGNicsXG4gICAgICBpdGVtczogW1xuICAgICAgICB7XG4gICAgICAgICAgbGluazogJ2ludHJvZHVjdGlvbicsXG4gICAgICAgICAgdGV4dDogJ1x1NEVDQlx1N0VDRCcsXG4gICAgICAgIH0sXG4gICAgICBdLFxuICAgIH0sXG4gICAge1xuICAgICAgY29sbGFwc2VkOiBmYWxzZSxcbiAgICAgIHRleHQ6ICdcdTVFMDNcdTVDNDBcdTdFQzRcdTRFRjYnLFxuICAgICAgaXRlbXM6IFtcbiAgICAgICAge1xuICAgICAgICAgIGxpbms6ICdsYXlvdXQtdWkvcGFnZScsXG4gICAgICAgICAgdGV4dDogJ1BhZ2UgXHU5ODc1XHU5NzYyJyxcbiAgICAgICAgfSxcbiAgICAgIF0sXG4gICAgfSxcbiAgICB7XG4gICAgICBjb2xsYXBzZWQ6IGZhbHNlLFxuICAgICAgdGV4dDogJ1x1OTAxQVx1NzUyOFx1N0VDNFx1NEVGNicsXG4gICAgICBpdGVtczogW1xuICAgICAgICB7XG4gICAgICAgICAgbGluazogJ2NvbW1vbi11aS92YmVuLWFwaS1jb21wb25lbnQnLFxuICAgICAgICAgIHRleHQ6ICdBcGlDb21wb25lbnQgQXBpXHU3RUM0XHU0RUY2XHU1MzA1XHU4OEM1XHU1NjY4JyxcbiAgICAgICAgfSxcbiAgICAgICAge1xuICAgICAgICAgIGxpbms6ICdjb21tb24tdWkvdmJlbi1hbGVydCcsXG4gICAgICAgICAgdGV4dDogJ0FsZXJ0IFx1OEY3Qlx1OTFDRlx1NjNEMFx1NzkzQVx1Njg0NicsXG4gICAgICAgIH0sXG4gICAgICAgIHtcbiAgICAgICAgICBsaW5rOiAnY29tbW9uLXVpL3ZiZW4tbW9kYWwnLFxuICAgICAgICAgIHRleHQ6ICdNb2RhbCBcdTZBMjFcdTYwMDFcdTY4NDYnLFxuICAgICAgICB9LFxuICAgICAgICB7XG4gICAgICAgICAgbGluazogJ2NvbW1vbi11aS92YmVuLWRyYXdlcicsXG4gICAgICAgICAgdGV4dDogJ0RyYXdlciBcdTYyQkRcdTVDNDknLFxuICAgICAgICB9LFxuICAgICAgICB7XG4gICAgICAgICAgbGluazogJ2NvbW1vbi11aS92YmVuLWZvcm0nLFxuICAgICAgICAgIHRleHQ6ICdGb3JtIFx1ODg2OFx1NTM1NScsXG4gICAgICAgIH0sXG4gICAgICAgIHtcbiAgICAgICAgICBsaW5rOiAnY29tbW9uLXVpL3ZiZW4tdnhlLXRhYmxlJyxcbiAgICAgICAgICB0ZXh0OiAnVnhlIFRhYmxlIFx1ODg2OFx1NjgzQycsXG4gICAgICAgIH0sXG4gICAgICAgIHtcbiAgICAgICAgICBsaW5rOiAnY29tbW9uLXVpL3ZiZW4tY291bnQtdG8tYW5pbWF0b3InLFxuICAgICAgICAgIHRleHQ6ICdDb3VudFRvQW5pbWF0b3IgXHU2NTcwXHU1QjU3XHU1MkE4XHU3NTNCJyxcbiAgICAgICAgfSxcbiAgICAgICAge1xuICAgICAgICAgIGxpbms6ICdjb21tb24tdWkvdmJlbi1lbGxpcHNpcy10ZXh0JyxcbiAgICAgICAgICB0ZXh0OiAnRWxsaXBzaXNUZXh0IFx1NzcwMVx1NzU2NVx1NjU4N1x1NjcyQycsXG4gICAgICAgIH0sXG4gICAgICBdLFxuICAgIH0sXG4gIF07XG59XG5cbmZ1bmN0aW9uIG5hdigpOiBEZWZhdWx0VGhlbWUuTmF2SXRlbVtdIHtcbiAgcmV0dXJuIFtcbiAgICB7XG4gICAgICBhY3RpdmVNYXRjaDogJ14vZ3VpZGUvJyxcbiAgICAgIHRleHQ6ICdcdTY1ODdcdTY4NjMnLFxuICAgICAgbGluazogJy9ndWlkZS9pbnRyb2R1Y3Rpb24vYWJvdXQnLFxuICAgIH0sXG4gICAge1xuICAgICAgdGV4dDogJ1x1NTcyOFx1N0VCRlx1OTg4NFx1ODlDOCcsXG4gICAgICBsaW5rOiAnaHR0cHM6Ly95aS53anlzLnRvcCcsXG4gICAgfSxcbiAgICB7XG4gICAgICB0ZXh0OiAnXHU2QjIyXHU4RkNFIFN0YXIgXHUyQjUwXHVGRTBGJyxcbiAgICAgIGxpbms6ICdodHRwczovL2dpdGVlLmNvbS92aWNoZW4yMDIxL3lpYWJwLW1pbmknLFxuICAgIH0sXG4gIF07XG59XG5cbmV4cG9ydCBjb25zdCBzZWFyY2g6IERlZmF1bHRUaGVtZS5BbGdvbGlhU2VhcmNoT3B0aW9uc1snbG9jYWxlcyddID0ge1xuICByb290OiB7XG4gICAgcGxhY2Vob2xkZXI6ICdcdTY0MUNcdTdEMjJcdTY1ODdcdTY4NjMnLFxuICAgIHRyYW5zbGF0aW9uczoge1xuICAgICAgYnV0dG9uOiB7XG4gICAgICAgIGJ1dHRvbkFyaWFMYWJlbDogJ1x1NjQxQ1x1N0QyMlx1NjU4N1x1Njg2MycsXG4gICAgICAgIGJ1dHRvblRleHQ6ICdcdTY0MUNcdTdEMjJcdTY1ODdcdTY4NjMnLFxuICAgICAgfSxcbiAgICAgIG1vZGFsOiB7XG4gICAgICAgIGVycm9yU2NyZWVuOiB7XG4gICAgICAgICAgaGVscFRleHQ6ICdcdTRGNjBcdTUzRUZcdTgwRkRcdTk3MDBcdTg5ODFcdTY4QzBcdTY3RTVcdTRGNjBcdTc2ODRcdTdGNTFcdTdFRENcdThGREVcdTYzQTUnLFxuICAgICAgICAgIHRpdGxlVGV4dDogJ1x1NjVFMFx1NkNENVx1ODNCN1x1NTNENlx1N0VEM1x1Njc5QycsXG4gICAgICAgIH0sXG4gICAgICAgIGZvb3Rlcjoge1xuICAgICAgICAgIGNsb3NlVGV4dDogJ1x1NTE3M1x1OTVFRCcsXG4gICAgICAgICAgbmF2aWdhdGVUZXh0OiAnXHU1MjA3XHU2MzYyJyxcbiAgICAgICAgICBzZWFyY2hCeVRleHQ6ICdcdTY0MUNcdTdEMjJcdTYzRDBcdTRGOUJcdTgwMDUnLFxuICAgICAgICAgIHNlbGVjdFRleHQ6ICdcdTkwMDlcdTYyRTknLFxuICAgICAgICB9LFxuICAgICAgICBub1Jlc3VsdHNTY3JlZW46IHtcbiAgICAgICAgICBub1Jlc3VsdHNUZXh0OiAnXHU2NUUwXHU2Q0Q1XHU2MjdFXHU1MjMwXHU3NkY4XHU1MTczXHU3RUQzXHU2NzlDJyxcbiAgICAgICAgICByZXBvcnRNaXNzaW5nUmVzdWx0c0xpbmtUZXh0OiAnXHU3MEI5XHU1MUZCXHU1M0NEXHU5OTg4JyxcbiAgICAgICAgICByZXBvcnRNaXNzaW5nUmVzdWx0c1RleHQ6ICdcdTRGNjBcdThCQTRcdTRFM0FcdThCRTVcdTY3RTVcdThCRTJcdTVFOTRcdThCRTVcdTY3MDlcdTdFRDNcdTY3OUNcdUZGMUYnLFxuICAgICAgICAgIHN1Z2dlc3RlZFF1ZXJ5VGV4dDogJ1x1NEY2MFx1NTNFRlx1NEVFNVx1NUMxRFx1OEJENVx1NjdFNVx1OEJFMicsXG4gICAgICAgIH0sXG4gICAgICAgIHNlYXJjaEJveDoge1xuICAgICAgICAgIGNhbmNlbEJ1dHRvbkFyaWFMYWJlbDogJ1x1NTNENlx1NkQ4OCcsXG4gICAgICAgICAgY2FuY2VsQnV0dG9uVGV4dDogJ1x1NTNENlx1NkQ4OCcsXG4gICAgICAgICAgcmVzZXRCdXR0b25BcmlhTGFiZWw6ICdcdTZFMDVcdTk2NjRcdTY3RTVcdThCRTJcdTY3NjFcdTRFRjYnLFxuICAgICAgICAgIHJlc2V0QnV0dG9uVGl0bGU6ICdcdTZFMDVcdTk2NjRcdTY3RTVcdThCRTJcdTY3NjFcdTRFRjYnLFxuICAgICAgICB9LFxuICAgICAgICBzdGFydFNjcmVlbjoge1xuICAgICAgICAgIGZhdm9yaXRlU2VhcmNoZXNUaXRsZTogJ1x1NjUzNlx1ODVDRicsXG4gICAgICAgICAgbm9SZWNlbnRTZWFyY2hlc1RleHQ6ICdcdTZDQTFcdTY3MDlcdTY0MUNcdTdEMjJcdTUzODZcdTUzRjInLFxuICAgICAgICAgIHJlY2VudFNlYXJjaGVzVGl0bGU6ICdcdTY0MUNcdTdEMjJcdTUzODZcdTUzRjInLFxuICAgICAgICAgIHJlbW92ZUZhdm9yaXRlU2VhcmNoQnV0dG9uVGl0bGU6ICdcdTRFQ0VcdTY1MzZcdTg1Q0ZcdTRFMkRcdTc5RkJcdTk2NjQnLFxuICAgICAgICAgIHJlbW92ZVJlY2VudFNlYXJjaEJ1dHRvblRpdGxlOiAnXHU0RUNFXHU2NDFDXHU3RDIyXHU1Mzg2XHU1M0YyXHU0RTJEXHU3OUZCXHU5NjY0JyxcbiAgICAgICAgICBzYXZlUmVjZW50U2VhcmNoQnV0dG9uVGl0bGU6ICdcdTRGRERcdTVCNThcdTgxRjNcdTY0MUNcdTdEMjJcdTUzODZcdTUzRjInLFxuICAgICAgICB9LFxuICAgICAgfSxcbiAgICB9LFxuICB9LFxufTtcbiJdLAogICJtYXBwaW5ncyI6ICI7QUFBNlgsU0FBUyxlQUFlO0FBQ3JaLFNBQVMsNkJBQTZCOzs7QUNFdEMsU0FBUyxlQUFlO0FBRXhCO0FBQUEsRUFDRTtBQUFBLEVBQ0E7QUFBQSxPQUNLO0FBRVA7QUFBQSxFQUNFO0FBQUEsRUFDQTtBQUFBLE9BQ0s7QUFDUCxPQUFPLGNBQWM7QUFDckIsU0FBUyxnQkFBQUEsZUFBYyw0QkFBNEI7QUFDbkQ7QUFBQSxFQUNFO0FBQUEsRUFDQTtBQUFBLE9BQ0s7OztBQ2pCUCxPQUFPLFlBQVk7QUFDbkIsU0FBUyxtQkFBbUI7QUFDNUIsU0FBUyxZQUFZO0FBRWQsSUFBTTtBQUFBO0FBQUEsRUFFWDtBQUFBO0FBRUYsU0FBUyxlQUFlLFNBQWlCO0FBQ3ZDLFFBQU07QUFBQSxJQUNKLFdBQVc7QUFBQSxJQUNYLFlBQVk7QUFBQSxJQUNaLFNBQVM7QUFBQSxJQUNULFFBQVE7QUFBQSxJQUNSLE9BQU87QUFBQSxJQUNQLFdBQVc7QUFBQSxFQUNiLEtBQUssY0FBYyxLQUFLLE9BQU8sS0FBSyxDQUFDLEdBQUcsTUFBTSxDQUFDO0FBRS9DLFFBQU0sUUFBUSxZQUFZLFNBQVMsTUFBTSxHQUFHLEVBQUUsSUFBSSxLQUFLO0FBRXZELFNBQU8sRUFBRSxXQUFXLFVBQVUsTUFBTSxPQUFPLFFBQVEsTUFBTTtBQUMzRDtBQUVPLElBQU0sb0JBQW9CLENBQUMsT0FBeUI7QUFDekQsS0FBRyxLQUFLLE1BQU0sTUFBTSxVQUFVLGdCQUFnQixDQUFDLFVBQVU7QUFDdkQsVUFBTSx3QkFBd0IsQ0FBQyxpQkFBeUI7QUFDdEQsWUFBTSxRQUFRLE1BQU0sT0FBTztBQUFBLFFBQ3pCLENBQUMsTUFBTSxFQUFFLFNBQVMsZ0JBQWdCLEVBQUUsUUFBUSxNQUFNLGlCQUFpQjtBQUFBLE1BQ3JFO0FBQ0EsVUFBSSxVQUFVLElBQUk7QUFDaEIsY0FBTSxrQkFBa0IsSUFBSSxNQUFNLE1BQU0sY0FBYyxJQUFJLENBQUM7QUFDM0Qsd0JBQWdCLFVBQVU7QUFBQSxFQUFtQixZQUFZO0FBQUE7QUFBQTtBQUN6RCxjQUFNLE9BQU8sT0FBTyxHQUFHLEdBQUcsZUFBZTtBQUFBLE1BQzNDLE9BQU87QUFDTCxZQUFJLE1BQU0sT0FBTyxLQUFLLEdBQUc7QUFDdkIsZ0JBQU0sVUFBVSxNQUFNLE9BQU8sS0FBSyxFQUFFO0FBQ3BDLGdCQUFNLE9BQU8sS0FBSyxFQUFFLFVBQVUsUUFBUTtBQUFBLFlBQ3BDO0FBQUEsWUFDQSxHQUFHLFlBQVk7QUFBQTtBQUFBLFVBQ2pCO0FBQUEsUUFDRjtBQUFBLE1BQ0Y7QUFBQSxJQUNGO0FBRUEsVUFBTSxRQUFRO0FBRWQsVUFBTSxNQUFNLE1BQU0sSUFBSSxXQUFXLE9BQU8sQ0FBQyxRQUFRLFFBQVE7QUFDdkQsWUFBTSxlQUFlLEtBQUssUUFBUSxJQUFJLEdBQUcsT0FBTyxHQUFHLEVBQUU7QUFBQSxRQUNuRDtBQUFBLFFBQ0E7QUFBQSxNQUNGO0FBRUEsVUFBSSxhQUF1QixDQUFDO0FBQzVCLFVBQUksWUFBWTtBQUVoQixVQUFJO0FBQ0YscUJBQ0UsWUFBWSxjQUFjO0FBQUEsVUFDeEIsVUFBVTtBQUFBLFVBQ1YsV0FBVztBQUFBLFVBQ1gsZUFBZTtBQUFBLFFBQ2pCLENBQUMsS0FBSyxDQUFDO0FBQUEsTUFDWCxRQUFRO0FBQ04sb0JBQVk7QUFBQSxNQUNkO0FBRUEsVUFBSSxDQUFDLFdBQVc7QUFDZCxlQUFPO0FBQUEsTUFDVDtBQUVBLFlBQU0sYUFBYSxvQkFBb0IsWUFBWTtBQUVuRCxZQUFNLGdCQUFnQixpQkFBaUIsVUFBVTtBQUNqRDtBQUFBLFFBQ0UsVUFBVSxhQUFhLFVBQVUsWUFBWTtBQUFBLE1BQy9DO0FBQ0EsWUFBTSxFQUFFLE1BQU0sTUFBTSxJQUFJLE1BQU07QUFFOUIsWUFBTSxRQUFRLE1BQU0sT0FBTyxVQUFVLENBQUMsTUFBTSxFQUFFLFFBQVEsTUFBTSxLQUFLLENBQUM7QUFFbEUsVUFBSSxDQUFDLE1BQU0sT0FBTyxLQUFLLEdBQUc7QUFDeEIsZUFBTztBQUFBLE1BQ1Q7QUFDQSxZQUFNLGNBQWM7QUFDcEIsbUJBQWEsV0FBVyxLQUFLLENBQUMsR0FBRyxNQUFNO0FBQ3JDLFlBQUksTUFBTSxZQUFhLFFBQU87QUFDOUIsWUFBSSxNQUFNLFlBQWEsUUFBTztBQUM5QixlQUFPLEVBQUUsY0FBYyxHQUFHLE1BQU0sRUFBRSxhQUFhLE9BQU8sQ0FBQztBQUFBLE1BQ3pELENBQUM7QUFDRCxZQUFNLE9BQU8sS0FBSyxFQUFFLFVBQ2xCLHVCQUF1QixtQkFBbUIsS0FBSyxVQUFVLFVBQVUsQ0FBQyxDQUFDLE9BQU8sYUFBYTtBQUFBO0FBRzNGLFlBQU0sY0FBYyxJQUFJLE1BQU0sTUFBTSxJQUFJLElBQUksQ0FBQztBQUM3QyxZQUFNLGFBQXdDLENBQUM7QUFDL0MsaUJBQVcsUUFBUSxDQUFDLGFBQWE7QUFHL0IsY0FBTSxnQkFBZ0IsSUFBSSxNQUFNLE1BQU0sZUFBZSxJQUFJLENBQUM7QUFDMUQsc0JBQWMsVUFBVSxjQUFjLFFBQVE7QUFDOUMsbUJBQVcsS0FBSyxhQUFhO0FBRTdCLGNBQU0sZUFBZSxLQUFLLGNBQWMsUUFBUTtBQUVoRCxjQUFNLEVBQUUsV0FBVyxVQUFVLE1BQU0sT0FBTyxNQUFNLElBQzlDLGVBQWUsWUFBWTtBQUU3QixjQUFNLFFBQVEsSUFBSSxNQUFNLE1BQU0sU0FBUyxRQUFRLENBQUM7QUFDaEQsY0FBTSxPQUFPLEdBQUcsUUFBUSxTQUFTLEdBQUcsUUFBUSxJQUFJLEtBQUssTUFBTSxFQUFFLEdBQzNELFFBQVEsSUFBSSxLQUFLLE1BQU0sRUFDekI7QUFFQSxjQUFNLFVBQVUsT0FBTyxRQUFRO0FBQy9CLFFBQUMsTUFBYyxNQUFNLENBQUMsWUFBWTtBQUNsQyxtQkFBVyxLQUFLLEtBQUs7QUFFckIsY0FBTSxjQUFjLElBQUksTUFBTSxNQUFNLGVBQWUsSUFBSSxDQUFDO0FBQ3hELG9CQUFZLFVBQVU7QUFDdEIsbUJBQVcsS0FBSyxXQUFXO0FBQUEsTUFDN0IsQ0FBQztBQUNELFlBQU0sU0FBUyxJQUFJLE1BQU0sTUFBTSxlQUFlLElBQUksQ0FBQztBQUNuRCxhQUFPLFVBQVU7QUFDakIsaUJBQVcsS0FBSyxNQUFNO0FBRXRCLFlBQU0sT0FBTyxPQUFPLFFBQVEsR0FBRyxHQUFHLEdBQUcsVUFBVTtBQUsvQyxhQUFPO0FBQUEsSUFDVCxDQUFDO0FBQUEsRUFDSCxDQUFDO0FBQ0g7QUFFQSxTQUFTLG9CQUFvQixPQUFlLFNBQWlCLElBQVk7QUFFdkUsUUFBTSxPQUFPLE9BQU8sV0FBVyxRQUFRLEVBQUUsT0FBTyxLQUFLLEVBQUUsT0FBTyxLQUFLO0FBR25FLFNBQU8sT0FBTyxTQUFTLE1BQU0sRUFBRSxFQUFFLFNBQVMsRUFBRSxFQUFFLE1BQU0sR0FBRyxNQUFNO0FBQy9EOzs7QUM1SUEsU0FBUyxvQkFBb0I7QUFJdEIsSUFBTSxLQUFLLGFBQWE7QUFBQSxFQUM3QixhQUFhO0FBQUEsRUFDYixNQUFNO0FBQUEsRUFDTixhQUFhO0FBQUEsSUFDWCxxQkFBcUI7QUFBQSxJQUNyQixxQkFBcUI7QUFBQSxJQUNyQixXQUFXO0FBQUEsTUFDVCxNQUFNO0FBQUEsTUFDTixNQUFNO0FBQUEsSUFDUjtBQUFBLElBQ0EsVUFBVTtBQUFBLE1BQ1IsU0FDRTtBQUFBLE1BQ0YsTUFBTTtBQUFBLElBQ1I7QUFBQSxJQUNBLFFBQVE7QUFBQSxNQUNOLFdBQVcsbUJBQWUsb0JBQUksS0FBSyxHQUFFLFlBQVksQ0FBQztBQUFBLE1BQ2xELFNBQVM7QUFBQSxJQUNYO0FBQUEsSUFDQSxlQUFlO0FBQUEsSUFDZixhQUFhO0FBQUEsTUFDWCxlQUFlO0FBQUEsUUFDYixXQUFXO0FBQUEsUUFDWCxXQUFXO0FBQUEsTUFDYjtBQUFBLE1BQ0EsTUFBTTtBQUFBLElBQ1I7QUFBQSxJQUNBLHNCQUFzQjtBQUFBLElBQ3RCLEtBQUssSUFBSTtBQUFBLElBRVQsU0FBUztBQUFBLE1BQ1AsT0FBTztBQUFBLElBQ1Q7QUFBQSxJQUNBLGtCQUFrQjtBQUFBLElBRWxCLFNBQVM7QUFBQSxNQUNQLGdCQUFnQixFQUFFLE1BQU0sZ0JBQWdCLE9BQU8sa0JBQWtCLEVBQUU7QUFBQSxNQUNuRSxnQkFBZ0IsRUFBRSxNQUFNLGdCQUFnQixPQUFPLGtCQUFrQixFQUFFO0FBQUEsTUFDbkUsV0FBVyxFQUFFLE1BQU0sV0FBVyxPQUFPLGFBQWEsRUFBRTtBQUFBLElBQ3REO0FBQUEsSUFDQSxrQkFBa0I7QUFBQSxFQUNwQjtBQUNGLENBQUM7QUFFRCxTQUFTLGVBQTJDO0FBQ2xELFNBQU87QUFBQSxJQUNMO0FBQUEsTUFDRSxXQUFXO0FBQUEsTUFDWCxNQUFNO0FBQUEsTUFDTixPQUFPO0FBQUEsUUFDTDtBQUFBLFVBQ0UsTUFBTTtBQUFBLFVBQ04sTUFBTTtBQUFBLFFBQ1I7QUFBQSxRQUNBO0FBQUEsVUFDRSxNQUFNO0FBQUEsVUFDTixNQUFNO0FBQUEsUUFDUjtBQUFBLFFBQ0E7QUFBQSxVQUNFLE1BQU07QUFBQSxVQUNOLE1BQU07QUFBQSxRQUNSO0FBQUEsUUFDQTtBQUFBLFVBQ0UsTUFBTTtBQUFBLFVBQ04sTUFBTTtBQUFBLFFBQ1I7QUFBQSxNQUNGO0FBQUEsSUFDRjtBQUFBLElBQ0E7QUFBQSxNQUNFLE1BQU07QUFBQSxNQUNOLE9BQU87QUFBQSxRQUNMLEVBQUUsTUFBTSxzQkFBc0IsTUFBTSxpQ0FBUTtBQUFBLFFBQzVDLEVBQUUsTUFBTSxtQkFBbUIsTUFBTSwyQkFBTztBQUFBLFFBQ3hDLEVBQUUsTUFBTSx3QkFBd0IsTUFBTSwyQkFBTztBQUFBLFFBQzdDLEVBQUUsTUFBTSxrQkFBa0IsTUFBTSwyQkFBTztBQUFBLFFBQ3ZDLEVBQUUsTUFBTSxrQkFBa0IsTUFBTSwyQkFBTztBQUFBLFFBQ3ZDLEVBQUUsTUFBTSxnQkFBZ0IsTUFBTSx3Q0FBVTtBQUFBLFFBQ3hDLEVBQUUsTUFBTSxtQkFBbUIsTUFBTSwyQkFBTztBQUFBLFFBQ3hDLEVBQUUsTUFBTSxlQUFlLE1BQU0sbUJBQVM7QUFBQSxRQUN0QyxFQUFFLE1BQU0sc0JBQXNCLE1BQU0saUNBQVE7QUFBQSxRQUM1QyxFQUFFLE1BQU0saUJBQWlCLE1BQU0sMkJBQU87QUFBQSxRQUN0QyxFQUFFLE1BQU0sa0JBQWtCLE1BQU0sMkJBQU87QUFBQSxNQUN6QztBQUFBLElBQ0Y7QUFBQSxJQUNBO0FBQUEsTUFDRSxNQUFNO0FBQUEsTUFDTixPQUFPO0FBQUEsUUFDTCxFQUFFLE1BQU0sdUJBQXVCLE1BQU0saUNBQVE7QUFBQSxRQUM3QyxFQUFFLE1BQU0sd0JBQXdCLE1BQU0sMkJBQU87QUFBQSxRQUM3QyxFQUFFLE1BQU0sa0JBQWtCLE1BQU0saUNBQVE7QUFBQSxRQUN4QyxFQUFFLE1BQU0sZ0JBQWdCLE1BQU0sbUJBQVM7QUFBQSxRQUN2QyxFQUFFLE1BQU0sdUJBQXVCLE1BQU0sMkJBQU87QUFBQSxRQUM1QyxFQUFFLE1BQU0sa0JBQWtCLE1BQU0saUNBQVE7QUFBQSxNQUMxQztBQUFBLElBQ0Y7QUFBQSxJQUNBO0FBQUEsTUFDRSxNQUFNO0FBQUEsTUFDTixPQUFPO0FBQUEsUUFDTCxFQUFFLE1BQU0sd0JBQXdCLE1BQU0saUNBQWE7QUFBQSxRQUNuRCxFQUFFLE1BQU0sMkJBQTJCLE1BQU0saUNBQVE7QUFBQSxRQUNqRCxFQUFFLE1BQU0seUJBQXlCLE1BQU0sMEJBQVc7QUFBQSxRQUNsRCxFQUFFLE1BQU0scUJBQXFCLE1BQU0saUNBQVE7QUFBQSxRQUMzQyxFQUFFLE1BQU0sd0JBQXdCLE1BQU0saUNBQVE7QUFBQSxNQUNoRDtBQUFBLElBQ0Y7QUFBQSxJQUNBO0FBQUEsTUFDRSxNQUFNO0FBQUEsTUFDTixPQUFPO0FBQUEsUUFDTCxFQUFFLE1BQU0sb0JBQW9CLE1BQU0sMkJBQU87QUFBQSxRQUN6QyxFQUFFLE1BQU0saUJBQWlCLE1BQU0sK0JBQVc7QUFBQSxRQUMxQyxFQUFFLE1BQU0scUJBQXFCLE1BQU0sMkJBQU87QUFBQSxNQUM1QztBQUFBLElBQ0Y7QUFBQSxJQUNBO0FBQUEsTUFDRSxNQUFNO0FBQUEsTUFDTixPQUFPO0FBQUEsUUFDTCxFQUFFLE1BQU0sYUFBYSxNQUFNLDJCQUFPO0FBQUEsUUFDbEMsRUFBRSxNQUFNLGlCQUFpQixNQUFNLDJCQUFPO0FBQUEsTUFDeEM7QUFBQSxJQUNGO0FBQUEsRUFDRjtBQUNGO0FBRUEsU0FBUyxvQkFBZ0Q7QUFDdkQsU0FBTztBQUFBLElBQ0w7QUFBQSxNQUNFLE1BQU07QUFBQSxNQUNOLE1BQU07QUFBQSxJQUNSO0FBQUEsSUFDQTtBQUFBLE1BQ0UsTUFBTTtBQUFBLE1BQ04sTUFBTTtBQUFBLElBQ1I7QUFBQSxJQUNBO0FBQUEsTUFDRSxNQUFNO0FBQUEsTUFDTixNQUFNO0FBQUEsSUFDUjtBQUFBLEVBQ0Y7QUFDRjtBQUVBLFNBQVMsb0JBQWdEO0FBQ3ZELFNBQU87QUFBQSxJQUNMO0FBQUEsTUFDRSxNQUFNO0FBQUEsTUFDTixPQUFPO0FBQUEsUUFDTDtBQUFBLFVBQ0UsTUFBTTtBQUFBLFVBQ04sTUFBTTtBQUFBLFFBQ1I7QUFBQSxNQUNGO0FBQUEsSUFDRjtBQUFBLElBQ0E7QUFBQSxNQUNFLFdBQVc7QUFBQSxNQUNYLE1BQU07QUFBQSxNQUNOLE9BQU87QUFBQSxRQUNMO0FBQUEsVUFDRSxNQUFNO0FBQUEsVUFDTixNQUFNO0FBQUEsUUFDUjtBQUFBLE1BQ0Y7QUFBQSxJQUNGO0FBQUEsSUFDQTtBQUFBLE1BQ0UsV0FBVztBQUFBLE1BQ1gsTUFBTTtBQUFBLE1BQ04sT0FBTztBQUFBLFFBQ0w7QUFBQSxVQUNFLE1BQU07QUFBQSxVQUNOLE1BQU07QUFBQSxRQUNSO0FBQUEsUUFDQTtBQUFBLFVBQ0UsTUFBTTtBQUFBLFVBQ04sTUFBTTtBQUFBLFFBQ1I7QUFBQSxRQUNBO0FBQUEsVUFDRSxNQUFNO0FBQUEsVUFDTixNQUFNO0FBQUEsUUFDUjtBQUFBLFFBQ0E7QUFBQSxVQUNFLE1BQU07QUFBQSxVQUNOLE1BQU07QUFBQSxRQUNSO0FBQUEsUUFDQTtBQUFBLFVBQ0UsTUFBTTtBQUFBLFVBQ04sTUFBTTtBQUFBLFFBQ1I7QUFBQSxRQUNBO0FBQUEsVUFDRSxNQUFNO0FBQUEsVUFDTixNQUFNO0FBQUEsUUFDUjtBQUFBLFFBQ0E7QUFBQSxVQUNFLE1BQU07QUFBQSxVQUNOLE1BQU07QUFBQSxRQUNSO0FBQUEsUUFDQTtBQUFBLFVBQ0UsTUFBTTtBQUFBLFVBQ04sTUFBTTtBQUFBLFFBQ1I7QUFBQSxNQUNGO0FBQUEsSUFDRjtBQUFBLEVBQ0Y7QUFDRjtBQUVBLFNBQVMsTUFBOEI7QUFDckMsU0FBTztBQUFBLElBQ0w7QUFBQSxNQUNFLGFBQWE7QUFBQSxNQUNiLE1BQU07QUFBQSxNQUNOLE1BQU07QUFBQSxJQUNSO0FBQUEsSUFDQTtBQUFBLE1BQ0UsTUFBTTtBQUFBLE1BQ04sTUFBTTtBQUFBLElBQ1I7QUFBQSxJQUNBO0FBQUEsTUFDRSxNQUFNO0FBQUEsTUFDTixNQUFNO0FBQUEsSUFDUjtBQUFBLEVBQ0Y7QUFDRjtBQUVPLElBQU0sU0FBdUQ7QUFBQSxFQUNsRSxNQUFNO0FBQUEsSUFDSixhQUFhO0FBQUEsSUFDYixjQUFjO0FBQUEsTUFDWixRQUFRO0FBQUEsUUFDTixpQkFBaUI7QUFBQSxRQUNqQixZQUFZO0FBQUEsTUFDZDtBQUFBLE1BQ0EsT0FBTztBQUFBLFFBQ0wsYUFBYTtBQUFBLFVBQ1gsVUFBVTtBQUFBLFVBQ1YsV0FBVztBQUFBLFFBQ2I7QUFBQSxRQUNBLFFBQVE7QUFBQSxVQUNOLFdBQVc7QUFBQSxVQUNYLGNBQWM7QUFBQSxVQUNkLGNBQWM7QUFBQSxVQUNkLFlBQVk7QUFBQSxRQUNkO0FBQUEsUUFDQSxpQkFBaUI7QUFBQSxVQUNmLGVBQWU7QUFBQSxVQUNmLDhCQUE4QjtBQUFBLFVBQzlCLDBCQUEwQjtBQUFBLFVBQzFCLG9CQUFvQjtBQUFBLFFBQ3RCO0FBQUEsUUFDQSxXQUFXO0FBQUEsVUFDVCx1QkFBdUI7QUFBQSxVQUN2QixrQkFBa0I7QUFBQSxVQUNsQixzQkFBc0I7QUFBQSxVQUN0QixrQkFBa0I7QUFBQSxRQUNwQjtBQUFBLFFBQ0EsYUFBYTtBQUFBLFVBQ1gsdUJBQXVCO0FBQUEsVUFDdkIsc0JBQXNCO0FBQUEsVUFDdEIscUJBQXFCO0FBQUEsVUFDckIsaUNBQWlDO0FBQUEsVUFDakMsK0JBQStCO0FBQUEsVUFDL0IsNkJBQTZCO0FBQUEsUUFDL0I7QUFBQSxNQUNGO0FBQUEsSUFDRjtBQUFBLEVBQ0Y7QUFDRjs7O0FGcFBPLElBQU0sU0FBU0MsY0FBYTtBQUFBLEVBQ2pDLFlBQVk7QUFBQSxFQUNaLE1BQU0sS0FBSztBQUFBLEVBQ1gsVUFBVTtBQUFBLElBQ1IsVUFBVSxJQUFJO0FBQ1osU0FBRyxJQUFJLGlCQUFpQjtBQUN4QixTQUFHLElBQUksaUJBQWlCO0FBQUEsSUFDMUI7QUFBQSxFQUNGO0FBQUEsRUFDQSxLQUFLLElBQUk7QUFBQSxFQUNULFFBQVE7QUFBQSxFQUNSLGFBQWE7QUFBQSxJQUNYLE1BQU07QUFBQSxJQUNOLFFBQVE7QUFBQSxNQUNOLFNBQVM7QUFBQSxRQUNQLFNBQVM7QUFBQSxVQUNQLEdBQUc7QUFBQSxRQUNMO0FBQUEsTUFDRjtBQUFBLE1BQ0EsVUFBVTtBQUFBLElBQ1o7QUFBQSxJQUNBLFdBQVc7QUFBQSxJQUNYLGFBQWE7QUFBQSxNQUNYLEVBQUUsTUFBTSxTQUFTLE1BQU0sMENBQTBDO0FBQUEsSUFDbkU7QUFBQSxFQUNGO0FBQUEsRUFDQSxPQUFPO0FBQUEsRUFDUCxNQUFNO0FBQUEsSUFDSixPQUFPO0FBQUEsTUFDTCx1QkFBdUI7QUFBQSxNQUN2QixRQUFRO0FBQUEsSUFDVjtBQUFBLElBQ0EsS0FBSztBQUFBLE1BQ0gsU0FBUztBQUFBLFFBQ1AsU0FBUztBQUFBLFVBQ1AsU0FBUztBQUFBLFVBQ1QscUJBQXFCLEVBQUUsY0FBYyxDQUFDLGFBQWEsRUFBRSxDQUFDO0FBQUEsUUFDeEQ7QUFBQSxNQUNGO0FBQUEsTUFDQSxxQkFBcUI7QUFBQSxRQUNuQixNQUFNO0FBQUEsVUFDSixLQUFLO0FBQUEsUUFDUDtBQUFBLE1BQ0Y7QUFBQSxJQUNGO0FBQUEsSUFDQSxNQUFNO0FBQUEsTUFDSixXQUFXO0FBQUEsSUFDYjtBQUFBLElBQ0EsU0FBUztBQUFBLE1BQ1AsYUFBYTtBQUFBLFFBQ1gsWUFBWSxDQUFDO0FBQUEsUUFDYixTQUFTLE1BQU07QUFBQSxNQUNqQixDQUFDO0FBQUEsTUFDRCw0QkFBNEI7QUFBQSxNQUM1QixtQkFBbUIsRUFBRSxXQUFXLGFBQWEsQ0FBQztBQUFBLE1BQzlDLG9CQUFvQjtBQUFBLE1BQ25CLE1BQU0sMEJBQTBCO0FBQUEsSUFDbkM7QUFBQSxJQUNBLFFBQVE7QUFBQSxNQUNOLElBQUk7QUFBQSxRQUNGLE9BQU8sQ0FBQyxPQUFPO0FBQUEsTUFDakI7QUFBQSxNQUNBLE1BQU07QUFBQSxNQUNOLE1BQU07QUFBQSxJQUNSO0FBQUEsSUFFQSxLQUFLO0FBQUEsTUFDSCxVQUFVLENBQUMsV0FBVztBQUFBLElBQ3hCO0FBQUEsRUFDRjtBQUNGLENBQUM7QUFFRCxTQUFTLE9BQXFCO0FBQzVCLFNBQU87QUFBQSxJQUNMLENBQUMsUUFBUSxFQUFFLFNBQVMsZ0JBQWdCLE1BQU0sU0FBUyxDQUFDO0FBQUEsSUFDcEQ7QUFBQSxNQUNFO0FBQUEsTUFDQTtBQUFBLFFBQ0UsU0FBUztBQUFBLFFBQ1QsTUFBTTtBQUFBLE1BQ1I7QUFBQSxJQUNGO0FBQUEsSUFDQSxDQUFDLFFBQVEsRUFBRSxNQUFNLGdCQUFnQixLQUFLLFFBQVEsTUFBTSxnQkFBZ0IsQ0FBQztBQUFBLElBQ3JFO0FBQUEsTUFDRTtBQUFBLE1BQ0E7QUFBQSxRQUNFLFNBQ0U7QUFBQSxRQUNGLE1BQU07QUFBQSxNQUNSO0FBQUEsSUFDRjtBQUFBLElBQ0EsQ0FBQyxRQUFRLEVBQUUsU0FBUyx3QkFBYyxNQUFNLFdBQVcsQ0FBQztBQUFBLElBQ3BELENBQUMsUUFBUSxFQUFFLE1BQU0sZ0JBQWdCLEtBQUssT0FBTyxDQUFDO0FBQUE7QUFBQTtBQUFBO0FBQUE7QUFBQTtBQUFBO0FBQUEsRUFPaEQ7QUFDRjtBQUVBLFNBQVMsTUFBa0I7QUFDekIsU0FBTztBQUFBLElBQ0wsc0JBQXNCO0FBQUEsSUFDdEIsVUFBVTtBQUFBLE1BQ1IsYUFDRTtBQUFBLE1BQ0YsT0FBTztBQUFBLFFBQ0w7QUFBQSxVQUNFLE9BQU87QUFBQSxVQUNQLEtBQUs7QUFBQSxVQUNMLE1BQU07QUFBQSxRQUNSO0FBQUEsUUFDQTtBQUFBLFVBQ0UsT0FBTztBQUFBLFVBQ1AsS0FBSztBQUFBLFVBQ0wsTUFBTTtBQUFBLFFBQ1I7QUFBQSxNQUNGO0FBQUEsTUFDQSxJQUFJO0FBQUEsTUFDSixNQUFNO0FBQUEsTUFDTixZQUFZO0FBQUEsTUFDWixhQUFhO0FBQUEsSUFDZjtBQUFBLElBQ0EsUUFBUSxRQUFRLFFBQVEsSUFBSSxHQUFHLGlCQUFpQjtBQUFBLElBQ2hELGNBQWM7QUFBQSxJQUNkLFNBQVM7QUFBQSxNQUNQLGNBQWMsQ0FBQywwQ0FBMEM7QUFBQSxNQUN6RCwrQkFBK0IsSUFBSSxPQUFPO0FBQUEsSUFDNUM7QUFBQSxFQUNGO0FBQ0Y7OztBRHRKQSxJQUFPLGdCQUFRO0FBQUEsRUFDYixzQkFBc0I7QUFBQSxJQUNwQixHQUFHO0FBQUEsSUFDSCxHQUFHO0FBQUEsRUFDTCxDQUFDO0FBQ0g7IiwKICAibmFtZXMiOiBbImRlZmluZUNvbmZpZyIsICJkZWZpbmVDb25maWciXQp9Cg==
