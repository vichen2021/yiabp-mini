# 架构设计

## DDD 分层架构

每个业务模块严格遵循 5 层项目结构：

```
module/{模块名}/
├── Yi.Framework.{Module}.Domain.Shared/          # 共享层：枚举、常量、事件
├── Yi.Framework.{Module}.Domain/                 # 领域层：实体、聚合根、领域服务、仓储接口
├── Yi.Framework.{Module}.Application.Contracts/  # 应用契约层：DTO、服务接口
├── Yi.Framework.{Module}.Application/            # 应用层：服务实现
└── Yi.Framework.{Module}.SqlSugarCore/           # 基础设施层：ORM 配置、仓储实现、数据种子
```

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

1. **在消费方 Domain 层定义适配器接口**
2. **在消费方 Domain.Shared 层定义跨模块 DTO**
3. **在消费方 Application 层实现适配器**

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

