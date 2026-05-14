# Yi.Abp Agents 开发规范

本文档用于约束 AI Agent 在 `Yi.Abp` 后端工程中的协作方式、代码生成流程、DDD 分层规范和质量要求。所有 AI 工具在处理本目录下任务时，应优先遵循本文档。

## 基本原则

- **先理解再修改**：修改代码前先确认模块边界、调用链、实体职责和现有实现方式。
- **优先使用项目 Skill**：涉及模块生成、CRUD 生成、字段同步时，必须使用指定 skill，不允许手写替代。
- **遵循 DDD 分层**：业务规则优先沉淀在领域层，应用层负责用例编排，基础设施层负责技术实现。
- **保持最小变更**：只修改与当前需求直接相关的文件，避免无关格式化、重构和依赖变更。
- **尊重项目约定**：使用 SqlSugar、Mapster、ABP 动态 API、`.slnx` 解决方案和集中版本管理。

## 强制 Skill 使用规则

### 1. 生成模块必须使用 `module-generator`

当任务包含以下任意意图时，必须调用 `module-generator` skill：

- 新建模块
- 创建业务模块
- 生成模块结构
- 添加 ABP Module 五层项目
- 用户提到 `module`、`add module`、`generate module`

生成模块时不得手写创建 5 层项目结构。标准模块结构为：

```text
module/{模块名}/
├── Yi.Module.{Module}.Domain.Shared/
├── Yi.Module.{Module}.Domain/
├── Yi.Module.{Module}.Application.Contracts/
├── Yi.Module.{Module}.Application/
└── Yi.Module.{Module}.SqlSugarCore/
```

生成后必须确认：

- **项目引用**：各层 `.csproj` 依赖关系正确。
- **模块依赖**：各层 ABP Module 使用 `[DependsOn]` 声明依赖。
- **解决方案**：模块项目加入 `Yi.Abp.slnx`。
- **动态 API**：需要暴露应用服务时，在 Web 层注册 Conventional Controllers。

### 2. 开发业务模块基础 CRUD 必须使用 `crud-generator-plus`

当任务包含以下任意意图时，必须调用 `crud-generator-plus` skill：

- 生成 CRUD
- 创建基础业务代码
- 基于实体生成前后端代码
- 新增业务实体的管理页面
- 生成菜单、权限、字典种子数据

不得绕过该 skill 手写完整 CRUD 脚手架。CRUD 生成前应先确认：

- **实体类已定义**：实体位于对应模块 `Domain/Entities`。
- **主键规范**：实体主键统一使用 `Guid`。
- **命名规范**：聚合根使用 `{Name}AggregateRoot`，普通实体使用 `{Name}Entity`。
- **字段语义**：字段类型、是否必填、默认值、枚举和字典需求明确。
- **菜单权限**：资源、动作、菜单路径、按钮权限和操作记录范围明确。

CRUD 生成后必须审查并补充：

- **业务规则**：唯一性校验、状态流转、删除约束等领域规则。
- **权限动作**：优先使用 `PermissionActionEnum`，避免裸字符串。
- **操作记录**：按业务行为添加 `[OperLogEntity]` 和 `[OperLog]`。
- **查询条件**：列表查询使用 SqlSugar `WhereIF` 模式。
- **枚举字典**：对外展示枚举应同步字典种子数据和前端字典常量。

### 3. 实体字段更新必须使用 `field-sync`

当任务包含以下任意意图时，必须调用 `field-sync` skill：

- 给实体新增字段
- 删除实体字段
- 修改实体字段类型
- 修改字段名称
- 同步 DTO、服务、前端表单或表格字段
- 同步字段相关字典种子数据

不得只修改实体类后遗漏 DTO、查询条件、映射、前端页面或字典配置。字段变更必须同步检查：

- **Domain**：实体字段、默认值、索引、导航属性。
- **Application.Contracts**：Create/Update/GetList 输入输出 DTO。
- **Application**：查询条件、排序、业务校验、对象映射。
- **SqlSugarCore**：数据种子、表初始化、字典种子。
- **Vben5 前端**：API 类型、表单 schema、表格列、搜索项、字典渲染。

## DDD 开发规范

### 分层职责

| 层 | 职责 | 可包含内容 |
|---|---|---|
| `Domain.Shared` | 跨层、跨模块共享契约 | 枚举、常量、错误码、集成事件、共享 DTO |
| `Domain` | 核心业务模型和业务规则 | 聚合根、实体、值对象、领域服务、仓储接口、领域事件 |
| `Application.Contracts` | 应用层公开契约 | 应用服务接口、输入 Vo、输出 Dto |
| `Application` | 用例编排和事务边界 | 应用服务、权限、操作记录、查询编排、防腐层实现 |
| `SqlSugarCore` | 持久化和基础设施 | ORM 配置、仓储实现、数据种子、字典种子 |

### 领域模型规则

- **聚合优先**：复杂业务对象应建模为聚合根，由聚合根维护内部一致性。
- **实体有行为**：实体不应只是贫血数据容器，关键业务规则应通过方法表达。
- **值对象表达概念**：无独立生命周期、依赖属性组合表达含义的概念应优先建模为值对象。
- **领域服务处理跨实体规则**：不自然归属于单个实体的领域规则放入 `Managers`。
- **应用服务不承载核心规则**：应用服务负责权限、事务、DTO 转换和调用领域对象，不应堆积核心业务判断。

### 聚合与事务边界

- **一个事务优先修改一个聚合**：避免在单个用例中直接修改多个聚合导致边界混乱。
- **跨聚合协作使用领域服务**：需要协调多个聚合时，由领域服务表达业务含义。
- **跨模块协作使用契约或事件**：不得为了方便直接引用其他模块 Domain 层。
- **最终一致性优先事件化**：跨模块、异步、可延迟处理的场景优先使用集成事件。

### 限界上下文边界

**禁止跨模块直接引用 Domain 层。** 模块 A 不得直接引用模块 B 的实体、聚合根、仓储接口或领域服务。

允许的跨模块依赖：

| 目标层 | 是否允许 | 说明 |
|---|---|---|
| `Domain.Shared` | 允许 | 用于共享枚举、常量和集成事件 |
| `Application.Contracts` | 允许 | 用于公开服务接口和 DTO |
| `Domain` | 禁止 | 属于模块内部领域实现 |
| `Application` | 禁止 | 属于模块内部用例实现 |
| `SqlSugarCore` | 禁止 | 属于模块内部基础设施实现 |

跨模块交互优先级：

1. **Application.Contracts 服务接口**：同步查询或命令调用的首选方式。
2. **防腐层适配器**：消费方 Domain 定义接口，Application 层实现适配目标模块契约。
3. **集成事件**：用于异步解耦、最终一致性、跨模块状态同步。

## 编码规范

### 命名规范

| 类型 | 命名规则 | 示例 |
|---|---|---|
| 聚合根 | `{Name}AggregateRoot` | `UserAggregateRoot` |
| 普通实体 | `{Name}Entity` | `UserRoleEntity` |
| 值对象 | `{Name}ValueObject` | `EncryPasswordValueObject` |
| 领域服务 | `{Name}Manager` | `UserManager` |
| 应用服务 | `{Name}Service` | `UserService` |
| 服务接口 | `I{Name}Service` | `IUserService` |
| 创建输入 | `{Entity}CreateInputVo` | `UserCreateInputVo` |
| 更新输入 | `{Entity}UpdateInputVo` | `UserUpdateInputVo` |
| 查询输入 | `{Entity}GetListInputVo` | `UserGetListInputVo` |
| 单条输出 | `{Entity}GetOutputDto` | `UserGetOutputDto` |
| 列表输出 | `{Entity}GetListOutputDto` | `UserGetListOutputDto` |

### 实体规范

- **ORM**：使用 SqlSugar 特性配置实体，禁止切换到 Entity Framework 写法。
- **主键**：所有实体主键类型统一使用 `Guid`。
- **审计**：按需要实现 `IAuditedObject`、`ISoftDelete` 等接口。
- **排序和状态**：通用排序使用 `IOrderNum`，启停状态使用统一枚举。
- **导航属性**：关联关系使用 SqlSugar `[Navigate]` 特性。
- **枚举字段**：两个及以上固定值必须定义枚举，禁止魔法字符串和魔法数字。

### 应用服务规范

- **CRUD 基类**：标准 CRUD 优先继承 `YiCrudAppService`。
- **动态 API**：使用 ABP Conventional Controllers，默认不手写 Controller。
- **查询模式**：条件过滤优先使用 SqlSugar `WhereIF`。
- **对象映射**：使用 Mapster，禁止引入 AutoMapper。
- **批量删除**：优先暴露批量删除接口，单条删除按项目模式禁用远程访问。
- **依赖注入**：遵循项目现有构造函数注入风格。

### 权限与操作记录

- **权限资源**：服务类使用 `[PermissionResource]` 声明模块和实体资源。
- **权限动作**：方法使用 `[PermissionAction]` 或 `[Permission]` 声明权限。
- **动作枚举**：标准动作优先使用 `PermissionActionEnum`。
- **操作记录**：重要增删改和业务动作应添加操作日志。
- **权限码格式**：统一使用 `{模块}:{实体}:{操作}`。

## 工作流程建议

### 新业务模块开发流程

1. **需求澄清**：明确业务目标、模块边界、实体设计、权限菜单、接口和验收标准。
2. **生成模块**：必须使用 `module-generator` 创建 5 层模块结构。
3. **定义实体**：在 Domain 层建模聚合根、实体、值对象和领域服务。
4. **生成 CRUD**：必须使用 `crud-generator-plus` 基于实体生成基础代码。
5. **补充规则**：将唯一性、状态流转、删除约束等业务规则补充到领域层或应用层合适位置。
6. **同步检查**：涉及实体字段变更时，必须使用 `field-sync` 完成全链路同步。
7. **验证构建**：运行后端构建、必要测试和前端页面检查。

### 修改现有功能流程

1. **定位权威逻辑**：先找实体、领域服务、应用服务和前端调用点。
2. **判断变更类型**：模块生成、CRUD 生成、字段同步分别触发对应 skill。
3. **最小范围修改**：避免无关重构、格式化和依赖升级。
4. **补齐影响面**：同步 DTO、权限、操作记录、查询、种子数据和前端。
5. **验证结果**：根据变更范围运行构建、测试或页面验证。

## 禁止事项

- **禁止手写替代 `module-generator`**：新模块结构必须由 skill 生成。
- **禁止手写替代 `crud-generator-plus`**：基础 CRUD 脚手架必须由 skill 生成。
- **禁止跳过 `field-sync`**：实体字段变化必须全链路同步。
- **禁止跨模块引用 Domain**：不得直接使用其他模块实体、仓储或领域服务。
- **禁止引入 EF/AutoMapper**：本项目使用 SqlSugar 和 Mapster。
- **禁止无依据改动公共框架**：框架层改动必须确认复用价值和影响范围。
- **禁止遗漏权限和操作记录**：管理后台业务接口必须考虑权限动作和操作日志。
- **禁止只改后端不查前端**：涉及接口或字段变化时必须检查 Vben5 调用方。

## 交付检查清单

- **Skill 使用**：是否按任务类型调用了 `module-generator`、`crud-generator-plus` 或 `field-sync`。
- **DDD 边界**：是否保持模块内聚，未跨模块引用 Domain。
- **实体建模**：是否正确使用聚合根、实体、值对象和领域服务。
- **DTO 命名**：输入是否使用 `Vo`，输出是否使用 `Dto`。
- **权限动作**：是否声明资源、动作和必要的操作记录。
- **字典枚举**：枚举是否同步字典种子和前端字典常量。
- **动态 API**：应用服务是否正确注册并遵循项目路由约定。
- **构建验证**：是否完成必要的后端构建、测试或前端验证。
