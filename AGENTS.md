# YiABP Agents 协作规范

本文档是仓库根目录的 AI Agent 协作入口。使用 Claude、Cursor、Windsurf 等 AI 工具处理本仓库任务时，应优先遵循本文档；进入后端 `Yi.Abp` 目录时，还应同时遵循 `Yi.Abp/AGENTS.md`。

## 工作区规则

- **优先打开仓库根目录**：默认以 `yiabp-mini` 为工作区，不要只打开 `Yi.Abp` 或 `Yi.Vben5` 子目录。
- **先识别任务范围**：开始修改前先判断任务属于后端、管理端前端、文档或跨端联动。
- **最小范围变更**：只修改与当前需求直接相关的文件，避免无关格式化、重构、依赖升级和配置变更。
- **先查现有实现**：新增能力前先搜索现有模块、服务、组件、工具和约定，避免重复造轮子。
- **尊重模块边界**：基础能力、扩展能力、实验代码应分清职责与提交范围。

## 项目结构

```text
Yi.Abp/      # .NET 10 + ABP 后端，SqlSugar ORM
Yi.Vben5/    # Vue 3 + Vben Admin 管理端
docs/        # 设计文档、计划和说明
resource/    # 资源文件
```

## 模块边界

本仓库定位为基础能力底座，包含以下基础模块：

- `rbac`
- `tenant-management`
- `setting-management`
- `file-management`
- `audit-logging`

本仓库不包含与基础能力无关的应用业务模块。

## 常用命令

```bash
dotnet run --project Yi.Abp\src\Yi.Abp.Web\Yi.Abp.Web.csproj
```

```bash
pnpm --dir Yi.Vben5 run dev:antd
```

## 后端开发强制规则

后端任务位于 `Yi.Abp` 时，必须遵循 `Yi.Abp/AGENTS.md`，尤其是以下规则：

- **新增模块必须使用 `module-generator`**：不得手写 5 层 ABP 模块结构。
- **生成 CRUD 必须使用 `crud-generator-plus`**：不得手写完整 CRUD 脚手架替代。
- **实体字段变更必须使用 `field-sync`**：不得只改实体而遗漏 DTO、服务、种子和前端。
- **禁止跨模块引用 Domain 层**：跨模块只能优先依赖 `Application.Contracts` 或 `Domain.Shared`。
- **ORM 固定使用 SqlSugar**：不得引入 Entity Framework 写法。
- **对象映射固定使用 Mapster**：不得引入 AutoMapper。
- **标准模块结构固定为 5 层**：

```text
module/{Module}/
├── Yi.Module.{Module}.Domain.Shared/
├── Yi.Module.{Module}.Domain/
├── Yi.Module.{Module}.Application.Contracts/
├── Yi.Module.{Module}.Application/
└── Yi.Module.{Module}.SqlSugarCore/
```

## 前端开发规则

### Yi.Vben5 管理端

- **先查现有页面模式**：新增页面前先参考同类模块的 API、表格、表单、字典和权限写法。
- **保持类型同步**：后端 DTO、查询条件、枚举变化后，应同步 API 类型、表格列、表单 schema 和搜索项。
- **避免无关依赖变更**：不要随意升级 pnpm 包、删除脚本或调整全局构建配置。
- **接口路径遵循后端动态 API**：不要自行发明路由。

## 新业务模块推荐流程

1. **需求澄清**：明确业务目标、模块边界、实体设计、权限菜单、字典枚举、接口和验收标准。
2. **输出设计文档**：复杂模块先在 `docs/` 下形成设计或实施计划。
3. **生成模块结构**：使用 `module-generator` skill 创建后端模块。
4. **定义领域实体**：在 Domain 层建模聚合根、实体、值对象和领域服务。
5. **生成基础 CRUD**：使用 `crud-generator-plus` skill 生成后端和前端基础代码。
6. **同步字段变化**：后续字段变更使用 `field-sync` skill。
7. **人工审查补强**：补充业务规则、权限动作、操作记录、种子数据和测试验证。

## 跨模块边界原则

- **按职责归属放置能力**：底层技术能力、通用能力和扩展能力应分清边界，不因临时复用随意上移。
- **业务模块只消费契约**：跨模块协作优先依赖 `Application.Contracts` 或 `Domain.Shared`，不要直接引用其他模块实现层。
- **通用能力需确认边界**：只有无业务语义、可被多个模块稳定复用的能力才适合进入 `Framework`。
- **避免无边界公共模块**：不要把领域规则、路由约定或应用逻辑放入 `Common`、`Utils`、`Helpers` 等泛化目录。

## 数据种子规则

- **种子必须可重放**：固定 Guid、固定 Code、幂等插入，不能依赖某个开发者本地已有数据。
- **文件类种子必须包含元数据和实际文件**：只写业务表 `FileId` 不够，还要保证 FileManagement 元数据和 Blob 文件可重建。
- **菜单权限保持一致**：菜单、按钮权限和权限定义应同步，权限码格式保持 `{模块}:{实体}:{操作}`。

## 验证要求

- **后端变更**：至少构建受影响项目；跨模块变更应构建相关模块或解决方案。
- **前端变更**：至少检查类型、路由、页面引用和必要的 type-check/lint。
- **数据种子变更**：检查幂等条件、执行顺序、固定 Guid 和权限码有效性。
- **提交前检查**：使用 `git status --short` 确认没有混入无关文件。

## 禁止事项

- **禁止未确认需求就大范围重构**。
- **禁止手写替代项目强制 skill 的生成流程**。
- **禁止跨模块直接引用其他模块 Domain、Application 或 SqlSugarCore 实现层**。
- **禁止把业务规则塞进 `Framework.Common`、`Utils`、`Helpers` 这类无边界模块**。
- **禁止引入 EF、AutoMapper 或与项目技术栈冲突的库**。
- **禁止修改与任务无关的包版本、锁文件、构建脚本和格式化配置**。
