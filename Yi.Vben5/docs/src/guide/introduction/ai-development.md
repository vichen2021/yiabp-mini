# 全栈+AI开发正确姿势

## 1. 正确打开项目目录方式

使用 Claude 或 Cursor 等 AI 工具开发项目时，**应该打开项目根目录 `yiabp-mini`**，而不是 `Yi.Abp` 或 `Yi.Vben5` 子目录。

**正确方式**：
```
✅ 打开目录：D:\Users\develop\YiABP\yiabp-mini
```

**错误方式**：
```
❌ 打开目录：D:\Users\develop\YiABP\yiabp-mini\Yi.Abp
❌ 打开目录：D:\Users\develop\YiABP\yiabp-mini\Yi.Vben5
```

::: tip 提示
如果只专注开发后端，请将 `.claude` 文件夹迁移至 `Yi.Abp` 目录下
:::

![项目目录结构截图](/guide/ai-dev1.png)

![项目目录结构截图](/guide/ai-dev2.png)

## 2. 使用命令行运行项目 🚀

大部分场景下**无需打开 Visual Studio 或 Rider IDE**，直接使用命令行运行项目更高效。

### 后端运行

```bash
# 在项目根目录或 Yi.Abp 目录下执行
dotnet run --project src\Yi.Abp.Web\Yi.Abp.Web.csproj
```

或者使用完整路径：

```bash
dotnet run --project D:\Users\develop\YiABP\yiabp-mini\Yi.Abp\src\Yi.Abp.Web\Yi.Abp.Web.csproj
```

### 前端运行

```bash
# 进入前端目录
cd Yi.Vben5

# 运行开发服务器
pnpm run dev:antd
```

![命令行运行截图](/guide/ai-dev1.png)

## 3. 使用 Skill 快速开发 ⚡

正式开发前，建议先使用 `superpowers` skill 与 AI 充分沟通需求，并输出一份开发文档。开发文档用于记录业务目标、模块边界、实体设计、菜单权限、字典枚举、接口约定、前后端范围和验收标准。

::: tip 提示
系统没有内置 `Superpowers` skill。由于该 skill 更新频繁，且每个人使用的 IDE 和 AI 工具不同，需要根据自己的开发环境自行安装。
:::

::: warning 注意
使用 AI 工具时，请选择高质量的模型（如 Claude、GPT-5 等），**不要使用垃圾模型**，否则生成的代码质量无法保证，反而会增加调试成本，甚至不如古法编程😄。
:::

### 可用的 Skills

- **Superpowers**：与 AI 沟通需求、梳理方案、建立开发文档
- **Module Generator**：自动生成完整的 ABP 模块结构
- **CRUD Generator Plus**：基于实体生成基础业务模块脚手架（后端+前端+菜单/系统字典种子数据）
- **Field Sync**：同步实体字段变更到整个代码库
- **Skill Creator**：创建自定义技能的指南

详细使用说明请查看 [Claude Skills 文档](/guide/skills/module-generator)。

## 4. 正确开发项目姿势：以产品管理模块为例 📐

新增业务模块不要直接让 AI 写代码，推荐先沟通、再建文档、再生成模块、最后补业务逻辑。以 `Product` 产品管理模块为例，推荐流程如下。

### 第一步：使用 Superpowers 建立开发文档

先向 AI 描述产品管理模块需求，让 `superpowers` 帮你追问并形成开发文档。

开发文档一般包含：

- **业务目标**：产品管理要解决什么问题
- **模块名称**：例如 `Product`
- **实体设计**：产品名称、编码、分类、状态、价格、库存等字段
- **字典/枚举**：产品状态、产品类型等
- **菜单权限**：菜单、按钮、权限码范围
- **前端页面**：列表、表单、详情、导入导出等
- **后端服务**：CRUD、校验规则、特殊业务动作
- **验收标准**：构建通过、菜单可见、CRUD 可用、权限生效

### 第二步：使用 Module Generator 创建 Product 模块

开发文档确认后，再使用 `module-generator` skill 创建 `Product` 模块。

它会生成完整的 ABP 模块结构：

- **Domain.Shared**：共享领域模型、枚举、常量
- **Domain**：领域实体、领域服务
- **Application.Contracts**：应用服务接口和 DTO
- **Application**：应用服务实现
- **SqlSugarCore**：数据库上下文、种子数据、模块数据库配置

生成后先运行后端构建，确保模块结构正确。

### 第三步：使用 CRUD Generator Plus 生成基础业务脚手架

模块结构创建完成后，定义产品实体类，再使用 `crud-generator-plus` skill 生成基础业务代码。

`crud-generator-plus` 会根据实体生成：

- **后端代码**：实体、DTO、应用服务、接口、查询条件、映射关系
- **前端代码**：API、类型定义、列表页、表单配置、操作按钮
- **菜单种子数据**：模块菜单、列表菜单、按钮权限
- **系统字典种子数据**：枚举或字典字段对应的数据项

### 第四步：补充业务逻辑并验证

脚手架生成后，再补充产品管理的业务逻辑，例如：

- 产品编码唯一性校验
- 产品上下架状态变更
- 库存和价格校验
- 导入导出
- 权限动作和操作记录细化

完成后执行后端构建、前端类型检查和页面功能测试。

## 5. 善用 AI 工具但保持代码审查 🔍

虽然 AI 工具可以快速生成代码，但**务必进行代码审查**：

- ✅ 检查生成的代码是否符合项目规范
- ✅ 验证业务逻辑是否正确
- ✅ 确保没有引入安全漏洞
- ✅ 运行构建和测试确保功能正常

::: info 提醒
AI 是辅助工具，最终代码质量的责任在开发者。建议在提交代码前进行充分的测试和审查。
:::

## 6. 实体变更后的同步建议 💡

### 使用 Field Sync 修改实体

1. **修改实体**：在 `AggregateRoot.cs` 中添加或修改字段
2. **使用 Skill**：使用 Field Sync 同步字段变更
3. **验证同步**：检查 DTO、服务、前端是否同步更新
4. **构建测试**：确保所有修改通过构建和类型检查

## 相关文档

- [快速开始](/guide/introduction/quick-start) - 项目启动指南
- [后端开发](/guide/backend/architecture) - 后端架构说明
- [前端开发](/guide/frontend/quick-start) - 前端开发指南
- [Claude Skills](/guide/skills/module-generator) - AI 辅助工具使用说明

