---
name: unibest-coding-standards
description: 用于 unibest / uni-app Vue3 TypeScript 项目的开发规范、代码生成、代码审查和问题排查。用户开发或修改页面、业务逻辑、tabbar、pages.config.ts、manifest.config.ts、vite-plugin-uni-pages、UnoCSS、Pinia、请求、多语言、wot-ui v2、App 白屏、HBuilderX 打包等内容时，应主动使用本 skill。强制优先使用 wot-ui v2，遵循 unibest 自动生成配置入口，避免手动修改会被覆盖的文件。
---

# unibest 开发规范

当用户在 `unibest` / `uni-app` / Vue3 / TypeScript 项目中进行页面开发、业务开发、代码审查、问题排查或配置调整时，使用本 skill。

本 skill 的作用是把 unibest 官方规范转化为执行约束。官网文档只作为生成本规范时的参考依据，不在 skill 正文中维护外部文档链接索引；处理任务时应优先读取项目现有代码和配置。wot-ui v2 组件的完整本地参考文档已随本 skill 放在 `llms-full.md`，需要精确组件 API、示例或实现细节时可读取该文件。

## 总体原则

- 优先遵循当前项目已有结构、命名、封装、类型、请求层、状态层和样式习惯。
- 不要臆造项目不存在的目录、别名、组件、API、hook、store 或工具函数。
- 修改代码前先确认权威配置入口，尤其是页面、路由、manifest、tabbar、请求和状态管理相关入口。
- 发现跨端风险时明确指出，尤其是 H5、小程序、App 差异。
- 输出应简洁，重点给出可执行修改、审查结论或排查路径。

## 硬性规则

- UI 组件库默认且优先使用 `wot-ui v2`。
- 未经用户明确允许，不要改用 uni-app 官方 UI 组件库或其他第三方 UI 组件库。
- 需要查询、选择或生成 wot-ui 组件代码时，优先调用 `wot-ui-v2` skill。
- 需要处理 `@wot-ui/cli` 时，优先调用 `wot-ui-cli` skill。
- 需要安装、配置或排查 `@wot-ui/unocss-preset` 时，优先调用 `wot-ui-unocss-preset-guide` skill。
- 需要生成 wot-ui 单文件主题 SCSS 时，优先调用 `create-wot-ui-theme` skill。
- 如果 wot-ui 相关 skills 不可用，应提示用户安装 wot-ui skills，而不是凭记忆生成复杂组件细节。
- 不要手动修改 `pages.json`，它由 unibest 工具链自动生成。
- 不要手动修改 `manifest.json`，应用配置应写入 `manifest.config.ts`。
- 修改会影响自动生成配置的内容后，应提醒用户重新运行开发命令。
- 页面局部状态优先使用 `ref` / `reactive`；只有跨页面、全局共享或需要持久化时才使用 Pinia。
- App 端不要在模块顶层调用 `useXxxStore()`，避免 Pinia 未初始化导致白屏。
- 不要启用官方文档中已标记弃用或不建议使用的能力。

## 技术栈认知

将 unibest 视为基于 uni-app 官方 Vite + Vue3 + TypeScript 模板增强而来的脚手架。常见默认能力包括：

- Vue 3
- Vite
- TypeScript
- pnpm
- UnoCSS
- UnoCSS Icons
- Pinia
- `pinia-plugin-persistedstate`，并适配 uni storage API
- `@uni-helper/vite-plugin-uni-pages`
- `@uni-helper/vite-plugin-uni-layouts`
- `@uni-helper/vite-plugin-uni-manifest`
- `vite-plugin-uni-platform`
- 请求封装与请求拦截
- 路由拦截
- 多环境配置
- ESLint / Stylelint / Prettier / husky / lint-staged / commitlint
- `App.ku.vue` 根组件挂载能力

## 环境与初始化规范

- 新建或维护 unibest 项目时，优先确认 Node.js、pnpm、Git、HBuilderX 版本是否满足项目要求。
- Node.js 通常要求 `>= 20`，遇到微信小程序编译异常时优先检查是否需要升级到 Node 22+。
- pnpm 通常要求 `>= 9`，遇到 workspace 配置异常时可检查 pnpm 版本，必要时升级到 pnpm 10。
- App 运行和发布离不开 HBuilderX；排查 App 问题时要同时考虑 HBuilderX 版本和 uni-app SDK 版本。
- 创建项目优先使用 `pnpm create unibest my-project`，不要手工拼装 unibest 项目结构。
- 添加 i18n、login、图表等 feature 时，优先使用 unibest CLI 的 add 能力或项目已有 feature 机制。
- 首次运行或自动导入 API 报错时，优先尝试重新执行依赖安装和开发命令，让生成文件与自动导入刷新。

## 项目结构与配置规范

- 用户项目通常以 `src/` 为源码目录。
- 页面由 `vite-plugin-uni-pages` 自动收集并生成 `pages.json`。
- 新代码中，页面级配置优先使用 `definePage`。
- 如果旧项目仍存在 route block，先尊重现状；但不要在新代码中主动引入已被替代的 route block 写法。
- 全局页面配置写在 `pages.config.ts`。
- `manifest.json` 由 `vite-plugin-uni-manifest` 自动生成。
- 应用配置写在 `manifest.config.ts`。
- `src/layouts` 下的 Vue 文件会被自动识别为布局。
- 默认布局通常是 `src/layouts/default.vue`。
- 需要切换布局时，使用页面配置，例如 `definePage({ layout: 'xxx' })`。
- 需要全局挂载根组件时，使用 `src/App.ku.vue`。

## unibest 插件机制规范

unibest 的页面、布局、manifest 与根组件能力主要依赖 uni-helper 生态插件。处理相关任务时，要先判断是哪个插件负责生成或接管该能力，再选择正确入口，避免直接修改生成物。

### `vite-plugin-uni-pages`

- 该插件提供约定式路由 / 文件路由能力。
- `src/pages` 目录下的每个 `.vue` 文件通常都会被识别为页面路由。
- 新增页面时，优先在 `src/pages` 下创建页面文件，而不是手写 `pages.json`。
- `pages.json` 是自动生成文件，不要手动修改。
- 全局页面配置写入 `pages.config.ts`。
- 页面级配置写入对应页面的 `definePage`。
- 设置首页时，通过页面配置设置 `type: 'home'`，并确保项目中只有一个首页配置。
- 如果多个页面配置为首页，最终顺序可能按字母序或生成顺序决定，容易产生非预期首页。
- 如果 `src/pages` 下存在不应生成页面的 Vue 文件，应检查 `vite.config.ts` 中 `UniPages({ exclude })` 配置。
- 分包应通过 `vite.config.ts` 中 `UniPages({ subPackages })` 配置。
- `subPackages` 是数组，可以配置多个分包目录。
- 分包目录不应是 `src/pages` 内部子目录，通常使用类似 `src/pages-sub` 的独立目录。

示例：

```ts
UniPages({
  exclude: ['**/components/**/**.*'],
  subPackages: ['src/pages-sub'],
})
```

### `vite-plugin-uni-layouts`

- 该插件提供布局自动识别和切换能力。
- `src/layouts` 下的 Vue 文件会被识别为布局。
- 默认布局通常是 `src/layouts/default.vue`。
- 布局文件通过 `<slot />` 承载页面内容。
- 需要切换页面布局时，在页面 `definePage` 中指定 `layout`。

示例：

```ts
definePage({
  layout: 'demo',
  style: {
    navigationBarTitleText: '关于',
  },
})
```

### `vite-plugin-uni-manifest`

- 该插件允许使用 TypeScript 编写应用 manifest 配置。
- `manifest.json` 是自动生成文件，不要手动修改。
- AppId、H5 路由基础路径、App 模块、权限、插件等应用级配置应写入 `manifest.config.ts`。
- 如果用户在 HBuilderX 中临时修改了 App 相关配置，要提醒迁移回 `manifest.config.ts`，否则再次生成时可能丢失。

### `App.ku.vue` 根组件挂载

- `src/App.ku.vue` 用于全局挂载根组件。
- 当需要在所有页面可用的根级组件时，优先检查或使用 `App.ku.vue`。
- wot-ui 的 Toast、MessageBox、Dialog 等需要挂载或根级支持的组件，应结合项目布局和 `App.ku.vue` 现状判断。
- 不要在每个页面重复挂载全局组件，除非项目现有规范就是这样。

## 页面开发规范

生成或修改页面时：

- 使用 Vue3 `<script lang="ts" setup>`。
- 使用项目已有别名、类型、hooks、store、api 封装，不重复造轮子。
- 页面标题、布局、首页标识等写入 `definePage` 或当前项目已有页面配置机制。
- 首页应只存在一个 `type: 'home'` 配置。
- 兼容 H5、小程序、App，不使用明显只支持单端的 API；必须使用时加平台条件。
- 优先使用 UnoCSS 原子类完成布局和样式。
- 组件选择优先使用 wot-ui v2 组件。
- 不要直接依赖 `localStorage`、`window`、`document` 等浏览器 only API，除非明确限定 H5。
- uni-app 运行环境变量使用 `import.meta.env`，不要使用 `process.env`。

## definePage 规范

unibest v3.12.0 起，页面配置应优先使用 `definePage`。

对象形式：

```ts
definePage({
  style: {
    navigationBarTitleText: '首页',
  },
})
```

函数形式：

```ts
definePage(() => ({
  style: {
    navigationBarTitleText: computedTitle(),
  },
}))
```

异步函数形式：

```ts
definePage(async () => {
  const title = await fetchPageTitle()
  return {
    style: {
      navigationBarTitleText: title,
    },
  }
})
```

## 样式规范

- 优先使用 UnoCSS 原子类。
- 常见布局使用 `flex`、`items-center`、`justify-center`、`flex-1` 等。
- 边框使用 `border`、`border-2`、`border-solid` 等。
- 圆角使用 `rounded-full`、`rounded-6`、`rounded-sm` 等。
- 行高使用 `leading-*`。
- 小程序端和非小程序端 UnoCSS 预设可能不同，应尊重项目现有 UnoCSS 配置。
- 如果原子类动态生成，需考虑 UnoCSS 扫描限制，必要时使用 safelist 或显式静态引用。
- tabbar 图标等动态类名尤其要注意 safelist 或显式引用。
- 如需 wot-ui UnoCSS preset 细节，调用 `wot-ui-unocss-preset-guide` skill。
- `presetUno` 主要用于 H5，非 H5 端不要盲目套用 Web 预设。
- `presetApplet` 用于小程序兼容，避免默认 Web 预设中的选择器或语法在小程序端报错；该思路通常也适用于 App。
- `presetIcons` 用于 UnoCSS Icons，需要配套安装对应 iconify 图标库。
- `presetLegacyCompat` 用于兼容低端 App 对新颜色函数语法支持不足的问题。
- 可以通过 `shortcuts` 沉淀常用原子类组合，例如 `center => flex justify-center items-center`。
- 如需安全区样式，优先检查项目是否已有 `p-safe`、`pt-safe`、`pb-safe` 等规则。
- 传统 CSS 写法通常按 750 设计稿换算 rpx。
- UnoCSS 写法要尊重项目配置；常见约定是小程序端 `mt-4 => 32rpx == 16px`，不要在未确认配置时机械换算。
- 原子化 CSS 与传统 CSS 是互补关系，不要强行把所有复杂样式都改成原子类。

## 图标与 SVG 规范

- 图标优先使用 wot-ui 图标或 UnoCSS Icons。
- UnoCSS Icons 动态图标名可能不会被扫描到，应避免无 safelist 的纯动态拼接。
- 小程序中 UnoCSS Icons 推荐使用中划线形式，例如 `i-carbon-user-avatar`，避免冒号形式带来的类名拆分问题。
- 可以使用 iconfont，但要遵循项目已有 iconfont 接入方式。
- 跨端 SVG 优先使用 `image + src` 方式。
- H5 专属 SVG 组件方式不能默认推广到小程序或 App，除非明确限定平台。
- wot-ui 图标颜色既可用 `color` 属性，也可用 UnoCSS 类；同时设置时通常 `color` 属性优先。
- UI 库图标的大小通常应使用组件 `size` 属性控制，不要依赖 UnoCSS 宽高类覆盖。
- iconfont 用于非 H5 端时要注意资源兼容性，项目设置通常需要 base64 或已有工程化处理。
- SVG 跨端可使用三种 `image + src` 来源：`static` 目录、相对导入、线上地址。
- `vite-svg-loader`、`vite-plugin-svg-icons`、SVG component、raw/component 导入等更偏 H5/Web，不要默认用于小程序或 App。

## tabbar 规范

unibest 常见 tabbar 策略：

- `NO_TABBAR = 0`：无 tabbar。
- `NATIVE_TABBAR = 1`：原生 tabbar，使用 `switchTab`，有缓存。
- `CUSTOM_TABBAR = 2`：自定义 tabbar，使用 `switchTab`，隐藏原生 tabbar，有缓存。

处理 tabbar 时：

- 原生 tabbar 配置 `nativeTabbarList`。
- 自定义 tabbar 配置 `customTabbarList`。
- 自定义 tabbar 可使用 `unocss`、`uiLib`、`iconfont`、`image` 等图标类型，具体以项目当前实现为准。
- 修改 tabbar 策略或配置后，提醒重新运行开发命令。
- 自定义 tabbar 首次点击闪烁是已知问题，不要承诺完全修复。
- 多语言 tabbar 标题应以当前项目和 unibest 版本能力为准，必要时使用 `uni.setTabBarItem`。

## 请求规范

- 优先使用项目已有请求封装。
- 简单项目可使用 unibest 简单版 http 封装。
- 如果项目已有 alova 或 vue-query，优先沿用项目现有方案。
- 请求能力通常包括普通请求、图片上传、多后台地址、header 传递和环境变量配置。
- 多后台地址、token/header、错误处理、登录失效跳转等应复用现有拦截器。
- 不要在页面中散落裸 `uni.request`，除非项目规范允许或只是临时验证。
- 普通接口建议集中放在 `api` 目录，按业务模块组织类型与请求函数。
- GET/POST 等方法、query、body、header 参数顺序要以项目已有 http 封装为准，不要凭记忆写签名。
- 图片上传优先检查项目是否已有 `useUpload` 或等价封装，不要重新写一套上传状态管理。
- 普通请求环境变量常见为 `VITE_SERVER_BASEURL`。
- 上传请求环境变量常见为 `VITE_UPLOAD_BASEURL`。
- 多后台地址场景中，可通过统一映射前缀处理，例如 `/cms`、`/ums`，但应放在请求拦截器或统一 http 层，不要散落在页面。

## 状态管理规范

- 局部页面状态优先使用 `ref` / `reactive`。
- 跨页面共享、全局用户信息、权限、主题、缓存数据等再使用 Pinia。
- Pinia 持久化通常通过 `pinia-plugin-persistedstate` 适配 `uni.getStorageSync` / `uni.setStorageSync`。
- `defineStore` 可配置 `persist: true`，但不要默认对所有 store 持久化。
- App 端禁止在模块顶层提前调用 store，避免 pinia 未初始化导致白屏。

## 多语言规范

- 多语言能力以项目当前 locale 封装为准。
- 非 H5 端使用 vue-i18n 传参可能有差异，应使用项目提供的格式化方案或兼容函数。
- 新增页面的导航栏标题在小程序端可能需要 `uni.setNavigationBarTitle`，并放在 `onShow` 中处理。
- tabbar 多语言标题以当前 unibest 版本能力为准；必要时手动使用 `uni.setTabBarItem`。
- 不要只验证 H5，要考虑小程序和 App 的多语言表现差异。
- 多语言格式化函数通常用于 `{name}`、`{detail.height}` 这类对象路径替换；不要假设它支持数组或复杂表达式。
- App 端多语言切换表现与小程序可能不同，Android 真机可能出现重启后语言生效的现象，排查时要按平台分别验证。

## 运行与发布规范

常见命令和输出目录：

- H5 开发：`pnpm dev:h5` 或 `pnpm dev`，默认访问 `http://localhost:9000/`。
- 微信小程序开发：`pnpm dev:mp-weixin`，导入 `dist/dev/mp-weixin`。
- App 开发：`pnpm dev:app`，用 HBuilderX 导入 `dist/dev/app`。
- H5 发布：`pnpm build:h5`，输出 `dist/build/h5`。
- 微信小程序发布：`pnpm build:mp-weixin`，输出 `dist/build/mp-weixin`。
- App 发布：`pnpm build:app`，用 HBuilderX 导入 `dist/build/app` 后云打包或本地打包。
- H5 非根目录部署时，检查 `manifest.config.ts` 中 `h5.router.base`。
- App 打包时，检查 AppId、`env/.env` 中 `VITE_UNI_APPID`、HBuilderX/SDK 版本匹配等。
- 云打包出现 Android 解析或兼容问题时，可检查 `minSdkVersion`，但最低不能低于平台允许值。
- uni-app SDK 版本应尽量匹配 HBuilderX 版本；遇到 App 白屏或构建异常时，把 SDK/HBuilderX 版本匹配作为重点排查项。
- Windows 可以安装多个 HBuilderX 版本，必要时用不同目录隔离版本。

## 常见问题处理规则

- `pages.json` 被覆盖：应改 `pages.config.ts` 或页面 `definePage`。
- `manifest.json` 被覆盖：应改 `manifest.config.ts`。
- 设置首页：页面配置中设置 `type: 'home'`，且只能有一个。
- 分包：检查 `vite.config.ts` 中 `UniPages` 的 `subPackages` 配置。
- 首次非 H5 端运行缺少 `src/manifest.json`：先执行依赖安装或生成流程。
- git commit 报错：检查 `commitlint.config.ts`；临时情况可提示用户使用 `--no-verify`，但不要默认绕过。
- 不想严格提交检测：由用户决定是否调整 husky，不应擅自删除。
- uni-app 中无法使用 `process.env`：改用 `import.meta.env`。
- wot-ui 的 Toast / MessageBox / Dialog 不生效：优先调用 `wot-ui-v2` skill 查询挂载或使用规范。
- uni-app 插件市场插件接入：以项目现状、unibest FAQ 和插件说明为准，避免直接复制不兼容用法。

## App 端专项规范

- 其他端正常但 App 白屏时，优先检查顶层 `useXxxStore()` 调用。
- HBuilderX 中修改过的 App 模块配置，应迁移到 `manifest.config.ts`，否则会被重新生成覆盖。
- App 热更新、Android、鸿蒙、iOS 模拟器差异应结合项目实际和 HBuilderX 行为判断。
- 原生插件打包应以 App 专题规范和 HBuilderX 实际配置为准。
- iOS 模拟器通常可导入 `dist/dev/app` 获得热更新体验。
- Android / 鸿蒙热更新通常需要把整个 unibest 项目导入 HBuilderX，而不是只导入 `dist/dev/app`。
- 原生插件配置应先在 HBuilderX / manifest 中确认，再迁移到 `manifest.config.ts` 的对应配置，避免生成时丢失。
- 原生插件、自定义基座等 App 专项流程不要简单套用 H5 或小程序构建命令。

## 扩展 FAQ 处理规则

- uni-app 官方升级可考虑 `npx @dcloudio/uvm@latest`，但要注意它可能额外安装依赖，需结合项目现状处理。
- 已加入 Git 管理的文件需要移出跟踪时，可提示使用 `git rm --cached` 思路，但不要擅自执行破坏性命令。
- 支付宝小程序运行异常时，可检查是否需要勾选“本地开发跳过 ES5 转译”。
- 当前 unibest 不默认支持 uni-app x，不要把 uni-app x 能力当成可用能力生成。
- `defineModel` 即使 Vue 版本满足，也可能只有 H5 可用，非 H5 端不要默认使用。
- 接入 uniCloud 时，可能需要在 HBuilderX 中重新识别项目类型，按项目实际流程处理。
- 微信小程序编译异常优先检查 Node 版本、开发者工具版本、lock 文件一致性。
- iOS 模拟器运行异常时，可检查 `esbuild` 版本是否与当前模板兼容。
- `@uni-helper/vite-plugin-uni-pages` 某些版本可能出现 JSON 解析问题；遇到 `[plugin:uni:mp-using-component] Unexpected token ...` 时，检查插件版本和 lock 文件。
- 不会 TypeScript 但项目允许 JS 时，可在 `tsconfig.json` 中考虑 `allowJs`，但不要擅自降低类型约束。
- 微信小程序 `INVALID_LOGIN` 可能来自游客模式或登录态问题，先让用户确认开发者工具登录状态。
- 钉钉小程序通常通过 package.json 的 `uni-app.scripts` 自定义平台脚本实现，不要把它当成默认内置命令。
- 插件市场插件若不支持 npm，应放入 `uni_modules` 并整理插件目录名；uni-app 插件通常依赖 easycom/uni_modules 规范自动识别。
- `vant-ui` 是 Web 端 UI 库，不适合直接用于 uni-app 多端项目。

## CLI 开发规范

- 区分 unibest 用户项目和 unibest CLI 源码仓库。
- `main` 分支通常包含 CLI 工具代码与模板源码；`base` 分支通常是用户项目克隆的纯净模板。
- CLI 源码通常在 `packages/cli`。
- CLI 开发环境通常要求 Node `>= 20`、pnpm `>= 9`。
- 本地 CLI 调试可关注 `pnpm dev`、`pnpm build`、`pnpm start -- my-test-project`、`LOCAL_TEMPLATE=true` 等流程。
- 新增 CLI feature 时，通常需要在 `features/` 下创建功能目录，提供 `hooks.js`、`package.json`，并在功能加载入口注册。
- 发布 CLI 到 npm 涉及登录、版本升级、构建、发布和 OTP，不要在未获得用户明确许可时执行发布类命令。

## wot-ui v2 协作规则

- wot-ui v2 支持微信小程序、支付宝小程序、钉钉小程序、H5、App 等多端。
- wot-ui v2 提供移动端组件、TypeScript 类型、国际化、主题定制、暗黑模式和 AI 友好文档。
- wot-ui 的 LLMs 结构化概览与完整文档是组件信息的重要依据，分别覆盖组件清单、文档入口、API、示例、事件、插槽、样式变量和实现细节。
- 本 skill 目录下的 `llms-full.md` 是 wot-ui v2 完整本地参考文档；当需要精确生成组件代码、核对组件 API、事件、插槽、样式变量、组合式 API 或排查组件行为时，应优先读取它。
- 当需要组件 API、示例、事件、插槽、样式变量时，优先调用 `wot-ui-v2` skill。
- 当需要 Toast、Dialog、Notify、ImagePreview 等函数式或挂载型组件时，要特别核对 wot-ui v2 的使用方式。
- 不要把 Web 端组件库写法套用到 uni-app / wot-ui。
- 如果用户明确要求使用非 wot-ui 组件库，必须先确认是否允许破坏本项目约束。
- wot-ui 常见能力包括表单、弹窗、Toast、Dialog、Notify、上传、图片预览、选择器、日历、表格、Tab、Tabbar、Navbar、暗黑模式、主题定制等；具体 API 不要凭空写，交给 `wot-ui-v2` skill 或官方结构化资料核对。
- 涉及 `useToast`、`useDialog`、`useNotify`、`useUpload`、`useImagePreview` 等组合式 API 时，优先查 wot-ui 规范。

## 回答风格

- 优先读取项目现有文件，遵循现有代码风格。
- 修改代码前先确认权威配置入口。
- 不确定时先调查，不要凭空编写。
- 发现跨端风险时明确指出。
- 需要 wot-ui 细节时调用对应 wot-ui skill。
- 输出简洁，重点给出可执行修改或审查结论。
