# 架构设计

## 项目整体结构

项目分为以下 3 个部分：

```
Yi.Abp/
├── framework/           # 框架部分：基础设施（不需要修改）
├── modules/             # 内置模块部分：Rbac、TenantManagement 等（业务开发区域）
└── src/                 # 组装层：引用模块、启动配置
    ├── Yi.Abp.Web/                    # Web 启动项目
    ├── Yi.Abp.Domain/                 # 领域组装
    ├── Yi.Abp.Domain.Shared/          # 共享组装
    ├── Yi.Abp.Application/            # 应用组装
    ├── Yi.Abp.Application.Contracts/  # 契约组装
    └── Yi.Abp.SqlSugarCore/           # 基础设施组装
```

### 各目录职责

| 目录 | 职责 | 是否需要修改 |
|------|------|-------------|
| `framework/` | 基础设施代码，如 SqlSugar 封装、认证、缓存等 | 一般不需要 |
| `modules/` | 业务模块开发，如 Rbac、TenantManagement | **主要开发区域** |
| `src/` | 组装各模块、启动配置、后台任务 | 需要时修改 |

### src 目录的作用

`src` 目录一般作为**组装层**，它的作用是：

1. **引用模块**：通过 `[DependsOn]` 将各模块组装在一起
2. **启动配置**：配置 JWT、Swagger、Hangfire、跨域等
3. **后台任务**：放置定时任务（Jobs 目录）

```csharp
// YiAbpApplicationModule.cs - 组装各模块
[DependsOn(
    typeof(YiAbpApplicationContractsModule),
    typeof(YiAbpDomainModule),
    typeof(YiFrameworkRbacApplicationModule),      // 引用 Rbac 模块
    typeof(YiFrameworkTenantManagementApplicationModule), // 引用租户模块
    typeof(YiFrameworkCodeGenApplicationModule)    // 引用代码生成模块
)]
public class YiAbpApplicationModule : AbpModule { }
```

### 什么时候在 src 中写代码？

| 场景 | 位置 | 示例 |
|------|------|------|
| 添加后台任务 | `src/Yi.Abp.Web/Jobs/` | `DemoResetJob.cs` |
| 添加全局设置 | `src/Yi.Abp.Domain.Shared/Settings/` | `TestSettingProvider.cs` |
| 修改启动配置 | `src/Yi.Abp.Web/YiAbpWebModule.cs` | JWT、Swagger、跨域配置 |
| 引用新模块 | `src/Yi.Abp.*/YiAbp*Module.cs` | 添加 `[DependsOn]` |

::: warning 注意
真正的业务开发应该在 `modules/` 目录下创建新模块，而不是在 `src` 中直接写业务代码，除非是小项目并且不在意依赖关系
:::

## DDD 分层架构

每个业务模块严格遵循 6 层项目结构：

```
module/{模块名}/
├── Yi.Framework.{Module}.Domain.Shared/          # 共享层：枚举、常量、事件
├── Yi.Framework.{Module}.Domain/                 # 领域层：实体、聚合根、领域服务、仓储接口
├── Yi.Framework.{Module}.Application.Contracts/  # 应用契约层：DTO、服务接口
├── Yi.Framework.{Module}.Application/            # 应用层：服务实现
├── Yi.Framework.{Module}.SqlSugarCore/           # 基础设施层：ORM 配置、仓储实现、数据种子
└── Yi.Framework.{Module}.Web/                    # Web 层（可选）
```

## 各层详解

### Domain.Shared 共享层

最底层，不依赖其他模块，存放：

- 常量定义
- 枚举定义
- 跨模块通用类

::: info 类比
类似三层架构的 Common 层，简单且不包含业务逻辑。
:::

### Domain 领域层

只依赖 `Domain.Shared`，存放：

- 实体和聚合根
- 值对象
- 领域服务（Managers 文件夹）
- 仓储接口
- 领域事件处理器

::: tip 开发模式选择
- **重领域层模式**：大部分业务放在领域层
- **重应用层模式**：大部分业务放在应用层（推荐新手）
:::

### Application.Contracts 应用契约层

对应用层的抽象，结构简单：

- DTO 定义（`Dtos/{Entity}/`）
- 服务接口定义（`IServices/`）

### Application 应用层

存放业务逻辑：

- CRUD 服务实现
- 通用业务服务
- Job 任务调度
- 事件处理
- SignalR Hub

::: info 开发建议
如果是重应用层开发，可以像三层架构一样将业务写入应用层。
:::

### SqlSugarCore 基础设施层

数据访问层，依赖领域层但不依赖应用层：

- ORM 配置
- 仓储实现
- 数据种子

::: warning 注意
框架已封装大部分通用场景，自定义仓储使用机会较少。框架与 SqlSugar 有轻量耦合，可在大部分地方直接使用 SqlSugar 操作。
:::

### Web 层

最简单的一层，仅用于：

- 启动配置
- 暴露 Web API

## 模块内部目录结构

### Domain 层

```
Entities/              # 实体和聚合根
Entities/ValueObjects/ # 值对象
Managers/              # 领域服务
Repositories/          # 仓储接口
EventHandlers/         # 领域事件处理器
Authorization/         # 权限相关（rbac 模块）
```

### Application 层

```
Services/              # 应用服务实现
Services/System/       # 系统级服务
Services/Monitor/      # 监控服务
Adapters/              # 防腐层适配器
SignalRHubs/           # 实时通信 Hub
```

### Application.Contracts 层

```
Dtos/{Entity}/         # 按实体分组的 DTO
IServices/             # 服务接口定义
```

## DDD 限界上下文边界（强制规则）

**禁止跨模块直接引用 Domain 层。** 模块 A 不得引用模块 B 的 `Domain` 层（实体、聚合根、仓储接口、领域服务），这是限界上下文的核心边界。

### 允许的跨模块依赖

| 层 | 是否允许跨模块引用 | 说明 |
|------|------|------|
| `Domain.Shared` | 允许 | 专为跨模块共享设计（枚举、常量、集成事件） |
| `Application.Contracts` | 允许 | 模块的公开 API（服务接口、DTO） |
| `Domain` | **禁止** | 模块内部实现，不得跨模块引用 |
| `Application` | **禁止** | 模块内部实现，不得跨模块引用 |
| `SqlSugarCore` | **禁止** | 模块内部实现，不得跨模块引用 |

### 跨模块交互方式

按优先级排序，优先使用靠前的方式：

#### 方式一：通过 Application.Contracts 服务接口交互（优先）

直接注入目标模块的 `Application.Contracts` 服务接口：

```csharp
public class ModuleAService : ApplicationService
{
    private readonly IModuleBService _moduleBService;  // ModuleB 的契约接口

    public async Task<ResultDto> DoSomethingAsync(Guid id)
    {
        var data = await _moduleBService.GetAsync(id);
    }
}
```

#### 方式二：防腐层适配器模式

当目标模块的 `Application.Contracts` 接口不能满足需求时，使用适配器模式：

1. 在消费方 Domain 层定义适配器接口
2. 在消费方 Domain.Shared 层定义跨模块 DTO
3. 在消费方 Application 层实现适配器

#### 方式三：集成事件（异步解耦）

模块间无需同步响应时，使用分布式事件总线。

## ABP 模块依赖

每个层都是 ABP 模块，通过 `[DependsOn]` 声明依赖关系：

```csharp
[DependsOn(
    typeof(YiFrameworkRbacApplicationContractsModule),
    typeof(YiFrameworkRbacDomainModule),
    typeof(YiFrameworkDddApplicationModule))]
public class YiFrameworkRbacApplicationModule : AbpModule
```

## 相关文档

- [命名规范](/guide/backend/naming) - 了解命名规范
- [实体定义](/guide/backend/entity) - 了解如何定义实体
- [模块开发](/guide/backend/module) - 了解如何开发新模块
- 本节部分内容参考自 [.Net意社区 - Yi框架架构设计](https://ccnetcore.com/article/aaa00329-7f35-d3fe-d258-3a0f8380b742/e0e7e180-f160-fecd-bf03-3a0f8387438c)
- 推荐阅读 [.Net意社区 - Yi框架依赖关系](https://ccnetcore.com/article/aaa00329-7f35-d3fe-d258-3a0f8380b742/27cd461e-2d49-d95b-5e8e-3a1100c6b32d)