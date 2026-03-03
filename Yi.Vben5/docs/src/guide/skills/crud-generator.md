# CRUD 生成器

## 功能说明

初始化完整的业务模块脚手架，包括后端（C# .NET with ABP framework）和前端（Vue3 + Vben5 + Ant Design Vue）。创建实体类、DTOs、服务接口、服务实现、菜单种子数据、API 文件和视图组件，遵循项目的既定模式。

## ⚠️ 重要提示

**此技能需要全栈实现**

- ❌ **不完整**：仅创建后端文件
- ❌ **不完整**：仅创建前端文件
- ❌ **不完整**：跳过验证步骤
- ✅ **完整**：后端 + 前端 + 验证全部通过

**必须按顺序完成步骤 1-4。每个步骤都有验证检查点。在所有验证步骤通过之前，不要标记任务为完成。**

## 使用场景

当你需要创建新的业务功能或需要全栈实现的 CRUD 模块时使用，例如：
- 创建新实体并包含完整的后端和前端
- 向现有系统添加新的业务模块
- 为部门管理、用户管理、产品管理等业务实体生成脚手架
- 为新模块设置菜单权限和种子数据

**重要**：此技能应在用户明确请求生成或搭建完整模块时使用，不适用于简单的文件创建或小修改。

## 快速检查清单

创建模块时需要：

### 后端（Yi.Abp/module/{module-name}）⚠️ 必需

- [ ] Entity：`Domain/Entities/{EntityName}AggregateRoot.cs`
- [ ] DTOs（5个文件）：`Application.Contracts/Dtos/{EntityName}/`
- [ ] 服务接口：`Application.Contracts/IServices/I{EntityName}Service.cs`
- [ ] 服务实现：`Application/Services/{EntityName}Service.cs`
- [ ] **菜单种子数据**：`module/rbac/.../DataSeeds/MenuDataSeed/{ModuleName}MenuDataSeed.cs` ⚠️ 关键 - 不要跳过
- [ ] 实体种子数据（可选）：`SqlSugarCore/DataSeeds/{EntityName}DataSeed.cs`

### 前端（Yi.Vben5/apps/web-antd/src）⚠️ 必需 - 不要跳过

- [ ] API Model：`api/{module-name}/{entity-name}/model.d.ts` ⚠️ 关键
- [ ] API Functions：`api/{module-name}/{entity-name}/index.ts` ⚠️ 关键
- [ ] View Index：`views/{module-name}/{entity-name}/index.vue` ⚠️ 关键
- [ ] View Data：`views/{module-name}/{entity-name}/data.ts` ⚠️ 关键
- [ ] View Drawer：`views/{module-name}/{entity-name}/{entity-name}-drawer.vue` ⚠️ 关键

### 验证 ⚠️ 必需 - 必须通过

- [ ] 后端：在 Yi.Abp 目录运行 `dotnet build` - 必须成功
- [ ] 后端：修复所有编译错误
- [ ] 前端：运行 TypeScript 类型检查 - 必须成功
- [ ] 前端：修复所有类型错误
- [ ] 前端：运行 lint 检查（推荐）
- [ ] 文档：在 `.docs/` 中记录模块（目录不存在则创建）

## 工作流程

### 步骤 1：理解需求 ⚠️ 必需

询问用户以明确：
- 实体名称和属性
- 模块名称（如果与实体不同）
- 是添加到现有模块菜单还是创建新模块菜单
- 特殊要求（树形结构、关联关系等）

**在需求明确之前，不要进行步骤 2。**

### 步骤 2：创建后端文件 ⚠️ 必需

#### 2.1 实体类

位置：`Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Domain/Entities/{EntityName}AggregateRoot.cs`

要点：
- 继承 `AggregateRoot<Guid>`
- 实现：`ISoftDelete`, `IAuditedObject`, `IOrderNum`, `IState`
- 使用 `[SugarTable]` 和 `[SugarColumn]` 特性

#### 2.2 DTO 类（5个文件）

位置：`Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application.Contracts/Dtos/{EntityName}/`

创建：
1. `{EntityName}GetOutputDto.cs` - 单条实体检索
2. `{EntityName}GetListOutputDto.cs` - 列表查询
3. `{EntityName}GetListInputVo.cs` - 查询参数（继承 `PagedAllResultRequestDto`）
4. `{EntityName}CreateInputVo.cs` - 创建
5. `{EntityName}UpdateInputVo.cs` - 更新

#### 2.3 服务接口

位置：`Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application.Contracts/IServices/I{EntityName}Service.cs`

继承 `IYiCrudAppService<...>` 并包含所有 DTO 类型。

#### 2.4 服务实现

位置：`Yi.Abp/module/{module-name}/Yi.Framework.{ModuleName}.Application/Services/{EntityName}Service.cs`

继承 `YiCrudAppService<...>` 并实现接口。为自定义查询重写 `GetListAsync()`。

#### 2.5 菜单种子数据 ⚠️ 关键 - 不要跳过

**检查点**：在继续之前，验证：
- [ ] 实体文件已创建
- [ ] 所有 5 个 DTO 文件已创建
- [ ] 服务接口已创建
- [ ] 服务实现已创建
- [ ] 菜单种子数据是下一步（必需）

**重要**：菜单种子数据是模块在 UI 中显示和权限工作的必需项。

**决策点**：询问用户是：
- **选项 A**：添加到现有模块菜单（例如，将 AppNav 添加到现有 APP配置 菜单）
- **选项 B**：创建新模块菜单文件

**选项 A - 更新现有菜单文件：**

位置：`Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed/{ExistingModule}MenuDataSeed.cs`

在现有的 `GetSeedData()` 方法中添加新实体的菜单项。

**选项 B - 创建新模块菜单文件：**

位置：`Yi.Abp/module/rbac/Yi.Framework.Rbac.SqlSugarCore/DataSeeds/MenuDataSeed/{ModuleName}MenuDataSeed.cs`

创建包含以下内容的完整菜单结构：
- 顶级目录菜单
- 带列表权限的实体菜单
- CRUD 权限菜单（查询、添加、编辑、删除）

**权限码格式**：`{module-name}:{entity-name}:{action}`
- 示例：`app:app-nav:list`, `app:app-nav:add`, `app:app-nav:edit`, `app:app-nav:remove`

**步骤 2 检查点 - 在继续之前验证：**
- [ ] 实体文件存在且正确
- [ ] 所有 5 个 DTO 文件存在且正确
- [ ] 服务接口存在且正确
- [ ] 服务实现存在且正确
- [ ] 菜单种子数据已创建/更新（关键）

**⚠️ 在步骤 2 的所有项目都检查之前，不要进行步骤 3。**

### 步骤 3：创建前端文件 ⚠️ 必需 - 不要跳过

**重要**：前端文件是模块功能所必需的。仅创建后端文件会导致模块不完整。

#### 3.1 API 文件 ⚠️ 关键

位置：`Yi.Vben5/apps/web-antd/src/api/{module-name}/{entity-name}/`

创建：
- `model.d.ts` - TypeScript 接口（必需）
- `index.ts` - API 函数（list、info、add、update、remove）（必需）

#### 3.2 视图文件 ⚠️ 关键

位置：`Yi.Vben5/apps/web-antd/src/views/{module-name}/{entity-name}/`

创建：
- `index.vue` - 主列表视图，包含表格和 CRUD 操作（必需）
- `data.ts` - 表单模式（querySchema、columns、drawerSchema）（必需）
- `{entity-name}-drawer.vue` - 创建/编辑抽屉组件（必需）

**步骤 3 检查点 - 在继续之前验证：**
- [ ] API model.d.ts 文件存在且正确
- [ ] API index.ts 文件存在且正确
- [ ] View index.vue 文件存在且正确
- [ ] View data.ts 文件存在且正确
- [ ] View drawer 组件存在且正确

**⚠️ 在步骤 3 的所有项目都检查之前，不要进行步骤 4。**

### 步骤 4：构建验证 ⚠️ 必需 - 必须通过

**关键**：此步骤必须通过，任务才被视为完成。不要跳过验证。

#### 4.1 后端验证（必需）

1. 在 `Yi.Abp` 目录运行 `dotnet build`
2. 如果构建失败：
   - 仔细阅读错误消息
   - 修复编译错误（常见：错误的基类、缺少 using 语句）
   - 重新运行构建直到成功
3. **在后端构建成功之前，不要继续**

#### 4.2 前端验证（必需）

1. 导航到 `Yi.Vben5` 目录
2. 运行 TypeScript 类型检查：`pnpm run typecheck`（或等效命令）
3. 如果存在类型错误：
   - 修复 API 模型和视图文件中的 TypeScript 错误
   - 确保所有导入都正确
   - 重新运行类型检查直到成功
4. 运行 lint 检查：`pnpm run lint`（可选但推荐）
5. **在前端验证通过之前，不要继续**

#### 4.3 文档（必需）

在 `.docs/{ModuleName}模块开发文档.md` 中记录模块，包含：
- 模块概述
- 实体结构
- API 端点
- 前端路由和权限

**注意**：如果 `.docs/` 目录不存在，请先创建它。

**最终检查点 - 任务完成验证：**
- [ ] 后端构建：`dotnet build` 无错误通过
- [ ] 前端类型检查：TypeScript 编译无错误通过
- [ ] 前端 lint：无关键 lint 错误（警告可接受）
- [ ] 文档：模块已在 `.docs/` 中记录
- [ ] 快速检查清单：上面清单中的所有项目都已检查

**⚠️ 只有在所有上述项目都已检查并验证后，任务才算完成。**

## 常见模式

### 树形结构实体

- 添加 `ParentId` 属性（Guid，默认 `Guid.Empty`）
- 添加 `Children` 属性用于树构建
- 在后端使用 `TreeHelper.SetTree()`
- 在前端网格中使用 `treeConfig`

### 自定义查询

- 为连接或复杂过滤重写 `GetListAsync()`
- 使用 SqlSugar 的 `LeftJoin`、`WhereIF`、`Select`

### 验证

- 重写 `CheckCreateInputDtoAsync()` 和 `CheckUpdateInputDtoAsync()`

## 命名规范

- **实体**：PascalCase（如 `AppNav`、`Dept`）
- **文件**：匹配实体名称（如 `AppNavAggregateRoot.cs`）
- **API 路径**：kebab-case（如 `/app-nav/list`）
- **前端目录**：kebab-case（如 `api/app/app-nav/`）
- **函数**：camelCase（如 `appNavList`、`appNavInfo`）

## 参考文件

- `references/backend-patterns.md` - 实体、DTO 和服务模板
- `references/menu-seed-patterns.md` - 菜单种子数据示例
- `references/frontend-patterns.md` - 前端 API 和视图模板
- `references/troubleshooting.md` - 常见问题和解决方案

## 示例

参考现有模块：
- **Config**：`Yi.Abp/module/rbac/` 和 `Yi.Vben5/apps/web-antd/src/views/system/config/` 和 `Yi.Vben5\apps\web-antd\src\api\system\config`
- **User**：`Yi.Abp/module/rbac/` 和 `Yi.Vben5/apps/web-antd/src/views/system/user/` 和 `Yi.Vben5\apps\web-antd\src\api\system\user`

## 相关文档

- [模块生成器](/guide/skills/module-generator) - 了解模块生成器
- [字段同步器](/guide/skills/field-sync) - 了解字段同步器
