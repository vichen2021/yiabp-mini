# 外部模块

除了项目默认引入的外部模块，有时我们还需要引入其他外部模块。当前 antdv 组件底座使用 `antdv-next`，新增 UI 组件时应优先沿用现有适配器和手动导入方式。

## 安装依赖

::: tip 安装依赖到指定包

- 由于项目采用了 [pnpm](https://pnpm.io/) 作为包管理工具，所以我们需要使用 `pnpm` 命令来安装依赖。
- 通过采用了 Monorepo 模块来管理项目，所以我们需要在指定包下安装依赖。安装依赖前请确保已经进入到指定包目录下。

:::

```bash
# cd /path/to/your/package
pnpm add your-package-name
```

## 使用

### 全局引入

```ts
import { createApp } from 'vue';
import App from './App';
import SomePlugin from 'your-package-name';

const app = createApp(App);

app.use(SomePlugin).mount('#app');
```

#### 使用

```vue
<template>
  <SomeComponent>text</SomeComponent>
</template>
```

### 局部引入

```vue
<script setup lang="ts">
import { Button } from 'antdv-next';
</script>

<template>
  <Button>text</Button>
</template>
```

::: warning 注意

- 如果组件有依赖样式，则需要按组件库文档再引入样式文件
- 不要在业务代码中新增旧 antd 底座依赖或旧标签组件写法

:::
