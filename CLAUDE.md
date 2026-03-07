# YiABP - 项目指南

## Monorepo 架构

项目采用 **Monorepo（单体仓库）** 架构，后端与前端代码统一管理在同一仓库中，便于版本同步、依赖共享和跨项目重构。

## 技术栈

### 后端

| 项目 | 技术 |
|------|------|
| 框架 | .NET 10 + ABP Framework 10.0.2 |
| ORM | SqlSugar 5.1.4.211（非 Entity Framework） |
| 数据库 | 兼容SQLite / MySQL / PostgreSQL等 |
| 认证 | JWT Bearer |
| 后台任务 | Hangfire（Redis / Memory） |
| 日志 | Serilog |
| DI 容器 | Autofac |
| 对象映射 | Mapster（非 AutoMapper） |
| 缓存 | FreeRedis / Memory Cache |
| 实时通信 | SignalR |
| 分布式锁 | Medallion（Redis） |

### 前端

| 项目 | 技术 |
|------|------|
| 框架 | Vue 3.5 + TypeScript 5.8 |
| 管理模板 | Vben Admin v5.5.7 |
| UI 库 | Ant Design Vue 4.2 |
| 构建工具 | Vite 6.3 |
| Monorepo | Turborepo + pnpm 10.10 |
| 状态管理 | Pinia 3.0 |
| 表格 | VXE Table 4.13 |
| 图表 | ECharts 5.6 |
| 测试 | Vitest（单元）、Playwright（E2E） |

## 后端架构

### DDD 分层架构

每个业务模块严格遵循 5 层项目结构：

```
module/{模块名}/
├── Yi.Framework.{Module}.Domain.Shared/          # 共享层：枚举、常量、事件
├── Yi.Framework.{Module}.Domain/                 # 领域层：实体、聚合根、领域服务、仓储接口
├── Yi.Framework.{Module}.Application.Contracts/  # 应用契约层：DTO、服务接口
├── Yi.Framework.{Module}.Application/            # 应用层：服务实现
└── Yi.Framework.{Module}.SqlSugarCore/           # 基础设施层：ORM 配置、仓储实现、数据种子
```

### 模块内部目录结构

**Domain 层：**
```
Entities/              # 实体和聚合根
Entities/ValueObjects/ # 值对象
Managers/              # 领域服务
Repositories/          # 仓储接口
EventHandlers/         # 领域事件处理器
Authorization/         # 权限相关（rbac 模块）
```

**Application 层：**
```
Services/              # 应用服务实现
Services/System/       # 系统级服务
Services/Monitor/      # 监控服务
Adapters/              # 防腐层适配器
SignalRHubs/           # 实时通信 Hub
```

**Application.Contracts 层：**
```
Dtos/{Entity}/         # 按实体分组的 DTO
IServices/             # 服务接口定义
```

## 命名规范

### 项目命名

项目遵循层级命名：`Yi.Framework.{模块名}.{层名}`

```
Yi.Framework.Rbac.Domain
Yi.Framework.Rbac.Application
Yi.Framework.Rbac.Application.Contracts
Yi.Framework.Rbac.SqlSugarCore
```

### 类命名

| 类型 | 命名规则 | 示例 |
|------|----------|------|
| 聚合根 | `{Name}AggregateRoot` | `UserAggregateRoot` |
| 普通实体 | `{Name}Entity` | `UserRoleEntity`, `DictionaryEntity` |
| 值对象 | `{Name}ValueObject` | `EncryPasswordValueObject` |
| 应用服务 | `{Name}Service` | `UserService` |
| 领域服务 | `{Name}Manager` | `UserManager`, `AccountManager` |
| 服务接口 | `I{Name}Service` | `IUserService`, `IRoleService` |
| ABP 模块 | `YiFramework{Module}{Layer}Module` | `YiFrameworkRbacApplicationModule` |

### DTO 命名

**注意：输入用 `Vo` 后缀，输出用 `Dto` 后缀。**

| 用途 | 命名规则 | 示例 |
|------|----------|------|
| 创建输入 | `{Entity}CreateInputVo` | `UserCreateInputVo` |
| 更新输入 | `{Entity}UpdateInputVo` | `UserUpdateInputVo` |
| 列表查询输入 | `{Entity}GetListInputVo` | `UserGetListInputVo` |
| 单条输出 | `{Entity}GetOutputDto` | `UserGetOutputDto` |
| 列表输出 | `{Entity}GetListOutputDto` | `UserGetListOutputDto` |

### 变量命名

- 私有字段使用下划线前缀：`_repository`, `_userManager`, `_currentUser`
- 命名空间与文件夹结构一致：`Yi.Framework.Rbac.Domain.Entities`
- 所有实体主键类型统一使用 `Guid`

## 编码模式

### 实体定义

实体使用 SqlSugar 特性进行 ORM 配置，常见接口组合为 `ISoftDelete`, `IAuditedObject`, `IOrderNum`, `IState`：

```csharp
[SugarTable("User")]
[SugarIndex($"index_{nameof(UserName)}", nameof(UserName), OrderByType.Asc)]
public class UserAggregateRoot : AggregateRoot<Guid>, ISoftDelete, IAuditedObject, IOrderNum, IState
{
    [SugarColumn(IsPrimaryKey = true)]
    public override Guid Id { get; protected set; }

    [SugarColumn(IsOwnsOne = true)]
    public EncryPasswordValueObject EncryPassword { get; set; } = new();
}
```

关联关系使用 `[Navigate]` 特性：

```csharp
// 多对多（通过中间表）
[Navigate(typeof(UserRoleEntity), nameof(UserRoleEntity.UserId), nameof(UserRoleEntity.RoleId))]
public List<RoleAggregateRoot> Roles { get; set; }

// 一对一
[Navigate(NavigateType.OneToOne, nameof(DeptId))]
public DeptAggregateRoot? Dept { get; set; }
```

### 枚举使用规范

**强制规则：具有 2 个及以上预定义值的字段必须使用枚举，禁止使用魔法字符串或魔法数字。**

#### 后端枚举定义

枚举定义在 `Domain.Shared/Enums/` 目录下，使用 `[Description]` 特性标注中文说明，值从 0 开始递增：

```csharp
public enum StateEnum
{
    [Description("禁用")] Disable = 0,
    [Description("启用")] Enable = 1,
}
```

实体和 DTO 中直接使用枚举类型（SqlSugar 自动映射为 int 存储）：

```csharp
// 实体
public StateEnum State { get; set; } = StateEnum.Enable;

// 查询输入（可空，用于条件过滤）
public StateEnum? State { get; set; }
```

#### 字典种子数据同步

每个枚举必须在 RBAC 模块的字典种子数据中注册对应的字典类型和字典项，供前端管理后台下拉框使用：

- `DictionaryTypeDataSeed.cs` — 注册字典类型
- `DictionaryDataSeed.cs` — 注册字典项，`DictValue` 为枚举 int 值的字符串形式（如 `"0"`, `"1"`）

#### 前端管理后台（Vben5）

在 `dict-enum.ts` 中注册字典常量，表单和表格中通过字典渲染：

```typescript
// dict-enum.ts
export const DictEnum = {
  STATE: 'state',
  // ...
} as const;

// data.ts — 下拉选项（字典值转为 number）
const stateOptions = () =>
  getDictOptions(DictEnum.STATE).map((o) => ({
    ...o,
    value: Number(o.value),
  }));

// data.ts — 表格列渲染
{ field: 'state', title: '状态', slots: {
    default: ({ row }) => renderDict(row.state, DictEnum.STATE),
  },
},
```

### 依赖注入

构造函数注入使用**元组解构**赋值模式：

```csharp
public UserService(ISqlSugarRepository<UserAggregateRoot, Guid> repository,
    UserManager userManager, ICurrentUser currentUser) : base(repository) =>
    (_userManager, _currentUser, _repository) =
    (userManager, currentUser, repository);
```

服务注册遵循 ABP 约定自动注册，显式注册使用 `ITransientDependency`：

```csharp
public class SomeAdapter : ISomeAdapter, ITransientDependency
```

### 应用服务基类

框架提供泛型 CRUD 基类 `YiCrudAppService`，内置 Excel 导入导出、批量删除等功能：

```csharp
public class UserService : YiCrudAppService<
    UserAggregateRoot,        // 实体
    UserGetOutputDto,         // 单条输出 DTO
    UserGetListOutputDto,     // 列表输出 DTO
    Guid,                     // 主键类型
    UserGetListInputVo,       // 列表查询输入
    UserCreateInputVo,        // 创建输入
    UserUpdateInputVo>        // 更新输入
```

所有列表查询 DTO 继承 `PagedAllResultRequestDto`，自带时间范围过滤和动态排序。

### API 模式

项目使用 ABP **动态 API（Conventional Controllers）**，应用服务自动映射为 REST 接口，无需手动编写 Controller：

```csharp
// 在 YiAbpWebModule 中配置
PreConfigure<AbpAspNetCoreMvcOptions>(options =>
{
    options.ConventionalControllers.Create(
        typeof(YiFramework{Module}ApplicationModule).Assembly,
        options =>
        {
            options.RemoteServiceName = "{module}";
            options.RootPath = "api/{module}";
        });
});
```

自动映射规则：`UserService.GetListAsync()` → `GET /api/user`，`CreateAsync()` → `POST /api/user`。

### 权限与操作日志

权限码格式：`{模块}:{实体}:{操作}`，通过 `[Permission]` 特性控制：

```csharp
[Permission("system:user:list")]
public override async Task<PagedResultDto<UserGetListOutputDto>> GetListAsync(...)

[Permission("system:user:add")]
public async override Task<UserGetOutputDto> CreateAsync(...)
```

操作日志通过 `[OperLog]` 特性记录：

```csharp
[OperLog("添加用户", OperEnum.Insert)]
public async override Task<UserGetOutputDto> CreateAsync(...)
```

批量删除为标准模式，单条删除禁用远程访问：

```csharp
[RemoteService(isEnabled: true)]
public virtual async Task DeleteAsync(IEnumerable<TKey> ids) { ... }

[RemoteService(isEnabled: false)]
public override Task DeleteAsync(TKey id) { ... }
```

### 查询模式

使用 SqlSugar 的 `WhereIF` 进行条件过滤，这是项目中最常见的查询模式：

```csharp
var outPut = await _repository._DbQueryable
    .WhereIF(!string.IsNullOrEmpty(input.UserName), x => x.UserName.Contains(input.UserName!))
    .WhereIF(input.State is not null, x => x.State == input.State)
    .WhereIF(input.StartTime is not null && input.EndTime is not null,
        x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)
    .LeftJoin<DeptAggregateRoot>((user, dept) => user.DeptId == dept.Id)
    .OrderByDescending(user => user.CreationTime)
    .Select((user, dept) => new UserGetListOutputDto(), true)
    .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
```

### ABP 模块依赖

每个层都是 ABP 模块，通过 `[DependsOn]` 声明依赖关系：

```csharp
[DependsOn(
    typeof(YiFrameworkRbacApplicationContractsModule),
    typeof(YiFrameworkRbacDomainModule),
    typeof(YiFrameworkDddApplicationModule))]
public class YiFrameworkRbacApplicationModule : AbpModule
```

### 事件总线

支持本地事件和分布式集成事件：

```csharp
// 本地事件
await _localEventBus.PublishAsync(new UserCreateEventArgs(entity.Id));

// 集成事件（跨模块）
[EventName("ModuleA.SomethingHappened")]
public class SomethingHappenedIntegrationEvent { ... }
```

### 对象映射

使用 Mapster（非 AutoMapper）：

```csharp
userRoleMenu.User = user.Adapt<UserDto>();
role.Adapt<RoleDto>();
```

## 测试规范

### 框架与工具

- 测试框架：xUnit
- 断言库：Shouldly
- 测试数据库：SQLite（每次运行生成唯一数据库文件）

### 测试命名

格式：`{Action}_{Entity}_Test`

```csharp
[Fact]
public async Task Get_User_Test() { ... }

[Fact]
public async Task Create_User_Test() { ... }

[Fact]
public async Task Update_User_Test() { ... }

[Fact]
public async Task Delete_User_Test() { ... }
```

测试通过应用服务层进行（集成测试风格），使用 `ServiceProvider.GetRequiredService<T>()` 解析服务。

## Git 提交规范

### 格式

遵循 **Conventional Commits** 规范，描述使用中文：

```
<type>(<scope>): <中文描述>
```

### 类型（type）

| 类型 | 含义 | 示例 |
|------|------|------|
| `feat` | 新功能 | `feat(rbac): 新增部门树形查询接口` |
| `fix` | 修复缺陷 | `fix(rbac): 修复角色删除时未清理关联数据` |
| `refactor` | 重构 | `refactor(domain): 重构领域服务以统一事件发布` |
| `chore` | 杂项维护 | `chore(deps): 升级 ABP 至 10.0.2` |
| `docs` | 文档 | `docs(project): 更新项目规则文档` |

### 作用域（scope）

作用域对应模块目录名：`rbac`, `domain`, `codegen`, `audit`, `setting`, `tenant`, `project` 等。

## 重要注意事项

### 架构决策

- ORM 使用 **SqlSugar**（非 Entity Framework），仓储接口为 `ISqlSugarRepository<TEntity, TKey>`
- 对象映射使用 **Mapster**（非 AutoMapper），调用方式为 `entity.Adapt<TDto>()`
- 解决方案文件使用 `.slnx` 格式（XML 格式，非传统 `.sln`）
- 版本集中管理在 `version.props`，构建属性集中在 `common.props`

### DDD 限界上下文边界（强制规则）

**禁止跨模块直接引用 Domain 层。** 模块 A 不得引用模块 B 的 `Domain` 层（实体、聚合根、仓储接口、领域服务），这是限界上下文的核心边界。

#### 允许的跨模块依赖

| 层 | 是否允许跨模块引用 | 说明 |
|------|------|------|
| `Domain.Shared` | 允许 | 专为跨模块共享设计（枚举、常量、集成事件） |
| `Application.Contracts` | 允许 | 模块的公开 API（服务接口、DTO） |
| `Domain` | **禁止** | 模块内部实现，不得跨模块引用 |
| `Application` | **禁止** | 模块内部实现，不得跨模块引用 |
| `SqlSugarCore` | **禁止** | 模块内部实现，不得跨模块引用 |

#### 跨模块交互方式

按优先级排序，优先使用靠前的方式：

**方式一：通过 Application.Contracts 服务接口交互（优先）**

直接注入目标模块的 `Application.Contracts` 服务接口，适用于目标模块已有合适接口的场景：

```csharp
public class ModuleAService : ApplicationService
{
    private readonly IModuleBService _moduleBService;  // ModuleB 的契约接口

    public async Task<ResultDto> DoSomethingAsync(Guid id)
    {
        // 通过接口调用，不直接操作 ModuleB 的实体和仓储
        var data = await _moduleBService.GetAsync(id);
    }
}
```

项目引用和模块依赖同步更新：

```xml
<!-- ModuleA.Application.csproj -->
<ProjectReference Include="..\..\module-b\Yi.Framework.ModuleB.Application.Contracts\..." />
```
```csharp
// YiFrameworkModuleAApplicationModule.cs
[DependsOn(typeof(YiFrameworkModuleBApplicationContractsModule))]
```

**方式二：防腐层适配器模式（Application.Contracts 无法满足时）**

当目标模块的 `Application.Contracts` 接口不能满足需求时（例如领域服务需要跨模块数据），使用适配器模式：

1. **在消费方 Domain 层定义适配器接口**（声明"我需要什么"）：

```
module/{消费方模块}/
└── Yi.Framework.{Module}.Domain/
    └── Adapters/
        └── I{Target}Adapter.cs      ← 接口定义
```

2. **在消费方 Domain.Shared 层定义跨模块 DTO**（避免依赖目标模块实体）：

```
module/{消费方模块}/
└── Yi.Framework.{Module}.Domain.Shared/
    └── Integration/
        └── {Target}Dto.cs            ← 防腐层 DTO
```

3. **在消费方 Application 层实现适配器**（桥接到目标模块的 Application.Contracts）：

```
module/{消费方模块}/
└── Yi.Framework.{Module}.Application/
    └── Adapters/
        └── {Target}Adapter.cs        ← 实现，注入目标模块的 IService
```

示例结构：

```csharp
// 1. ModuleA.Domain/Adapters/IModuleBAdapter.cs — 接口定义
public interface IModuleBAdapter
{
    Task<List<SomeDto>> GetListAsync();
    Task<SomeDto?> GetByIdAsync(Guid id);
}

// 2. ModuleA.Domain.Shared/Integration/SomeDto.cs — 防腐层 DTO
public record SomeDto(Guid Id, string Name);

// 3. ModuleA.Application/Adapters/ModuleBAdapter.cs — 适配器实现
public class ModuleBAdapter : IModuleBAdapter, ITransientDependency
{
    private readonly IModuleBService _moduleBService;  // ModuleB.Application.Contracts

    public async Task<List<SomeDto>> GetListAsync()
    {
        var result = await _moduleBService.GetListAsync(new());
        return result.Items.Select(x => new SomeDto(x.Id, x.Name)).ToList();
    }
}
```

**方式三：集成事件（异步解耦）**

模块间无需同步响应时，使用分布式事件总线：

```csharp
// 发布方
await _distributedEventBus.PublishAsync(new SomethingHappenedIntegrationEvent { ... });

// 订阅方
public class SomethingHappenedHandler : IDistributedEventHandler<SomethingHappenedIntegrationEvent> { ... }
```

#### 违规示例（禁止）

```csharp
// 错误：ModuleA 直接引用 ModuleB.Domain 的实体和仓储
using Yi.Framework.ModuleB.Domain.Entities;
private readonly ISqlSugarRepository<ModuleBEntity, Guid> _moduleBRepository;
```

### 新建模块检查清单

新建业务模块时，确保：

1. 创建完整的 5 层项目结构（Domain.Shared / Domain / Application.Contracts / Application / SqlSugarCore）
2. 每层创建对应的 ABP Module 类并声明 `[DependsOn]`
3. 在 `YiAbpWebModule.cs` 中注册动态 API（`ConventionalControllers.Create`）
4. 在主模块中添加对新模块 Application 层的依赖
5. DTO 遵循 Input 用 `Vo` 后缀、Output 用 `Dto` 后缀的规范

### Claude Skills

项目集成了以下 Claude Skills（位于 `.claude/skills/`）：

- **module-generator** — 自动生成 ABP 模块的完整 5 层项目结构
- **field-sync** — 实体字段变更后同步 DTO、服务、前端及字典种子数据
- **simplify** — 审查变更代码，发现并修复质量、复用性问题
- **skill-creator** — 创建新 Skill 的指南工具

### 环境要求

| 组件 | 要求 |
|------|------|
| 后端 | .NET 10 SDK |
| 前端 | Node.js >= 20.15.0, pnpm |
| 数据库 | SQLite / MySQL / PostgreSQL |
| 缓存（可选） | Redis |

## AI 协作规范

### ⚠️ 已记录的严重错误（警示）

以下错误曾在实际协作中发生，**绝对禁止重犯**：

#### 错误一：使用错误的 Skill（已有明确指令时）

**罪证**：用户明确写道「你可以查看 `/crud-module-generator`」，我却调用了 `Skill(module-generator)`。

**规则**：用户通过 `/skill-name` 语法指定 skill 时，必须调用**完全匹配名称**的 skill，不得调用其他 skill。调用前仔细核对 skill 名称，一字不差。

#### 错误二：对简单文件搜索任务偷懒调用子 Agent

**罪证**：仅需查找 1-2 个文件，完全可以直接用 Glob/Grep/Read 完成，却启动了 Task(Explore) 子 Agent。

**规则**：以下情况**禁止**使用子 Agent：
- 单个文件读取（用 Read）
- 简单文件路径搜索（用 Glob）
- 关键词搜索（用 Grep）
- 不超过 3 次工具调用能完成的任务

子 Agent 仅用于：明确可并行的 3 个以上独立子任务，或需要深度探索整个代码库的复杂研究任务。

---

### 子 Agent 使用规范

- **拆分原则**：除非当前任务可以被清晰拆分为 **3 个及以上、互相独立且可并行执行的子任务**，否则不得创建或调度子 Agent。

### 文件查找与确认

**当用户指定参考文件时：**

1. 使用工具查找文件
2. 文件不存在时，明确告知用户并等待指示
3. 禁止在未找到文件的情况下擅自生成内容

**原则：** 透明告知、用户主导、避免假设
