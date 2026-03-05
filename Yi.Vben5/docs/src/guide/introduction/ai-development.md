# 全栈+AI开发指南

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

充分利用项目集成的 Claude Skills（模块生成器、CRUD 生成器等），可以快速生成完整的业务模块代码，让你**更专注于业务逻辑**，无需担心基础的 CRUD 代码编写。

::: warning 注意
使用 AI 工具时，请选择高质量的模型（如 Claude、GPT-5 等），**不要使用垃圾模型**，否则生成的代码质量无法保证，反而会增加调试成本，甚至不如古法编程😄。
:::

### 可用的 Skills

- **Module Generator**：自动生成完整的 ABP 框架模块结构
- **CRUD Generator**：初始化完整的业务模块脚手架（后端+前端）
- **Field Sync**：同步实体字段变更到整个代码库
- **Skill Creator**：创建自定义技能的指南

详细使用说明请查看 [Claude Skills 文档](/guide/skills/module-generator)。

## 4. 理解项目架构再开发 📐

在开始开发前，建议先了解项目的整体架构：

- **后端**：基于 ABP Framework + SqlSugar 的分层架构
  - Domain.Shared：共享领域模型
  - Domain：领域层（实体、仓储接口）
  - Application.Contracts：应用服务接口和 DTO
  - Application：应用服务实现
  - SqlSugarCore：数据访问层

- **前端**：基于 Vben5 + Ant Design Vue 的模块化架构
  - API 层：`api/{module-name}/{entity-name}/`
  - 视图层：`views/{module-name}/{entity-name}/`
  - 组件层：可复用的业务组件

- **数据流**：Entity → DTO → Service → Controller → API → Frontend

理解架构后，AI 工具能更准确地生成符合项目规范的代码。

## 5. 善用 AI 工具但保持代码审查 🔍

虽然 AI 工具可以快速生成代码，但**务必进行代码审查**：

- ✅ 检查生成的代码是否符合项目规范
- ✅ 验证业务逻辑是否正确
- ✅ 确保没有引入安全漏洞
- ✅ 运行构建和测试确保功能正常

::: info 提醒
AI 是辅助工具，最终代码质量的责任在开发者。建议在提交代码前进行充分的测试和审查。
:::

## 6. 开发流程建议 💡

### 使用 Module Generator 创建新模块

当你需要在 `src/WebApi/module` 目录下创建新的 ABP 模块时，可以使用 Module Generator 快速生成完整的模块结构。

1. **确定模块名称**：确定新模块的名称（如 "ContentManagement"、"OrderManagement"）
2. **使用 Skill**：使用 Module Generator 生成模块结构
3. **验证生成**：检查生成的 5 个项目层是否正确
   - Domain.Shared：共享领域模型
   - Domain：领域层
   - Application.Contracts：应用服务接口和 DTO
   - Application：应用服务实现
   - SqlSugarCore：数据访问层
4. **构建验证**：运行 `dotnet build` 确保模块结构正确
5. **后续开发**：在生成的模块基础上使用 CRUD Generator 创建业务实体

### 使用 CRUD Generator 生成所有基础代码

1. **明确需求**：确定实体名称、属性和业务规则
2. **使用 Skill**：使用 CRUD Generator 生成完整模块
3. **代码审查**：检查生成的代码是否符合规范
4. **构建验证**：运行 `dotnet build` 和前端类型检查
5. **功能测试**：测试 CRUD 功能是否正常
6. **业务完善**：添加业务逻辑和特殊处理

### 使用 Field Sync 修改实体

1. **修改实体**：在 `AggregateRoot.cs` 中添加或修改字段
2. **使用 Skill**：使用 Field Sync 同步字段变更
3. **验证同步**：检查 DTO、服务、前端是否同步更新
4. **构建测试**：确保所有修改通过构建和类型检查

## 相关文档

- [快速开始](/guide/introduction/quick-start) - 项目启动指南
- [后端开发](/guide/backend/architecture) - 后端架构说明
- [前端开发](/guide/frontend/development) - 前端开发指南
- [Claude Skills](/guide/skills/module-generator) - AI 辅助工具使用说明

