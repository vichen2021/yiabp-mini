# 架构设计

## 项目整体结构

后端位于 `Yi.Abp/`，2.0 后明确区分 `framework`、`module` 和 `src`：

```text
Yi.Abp/
├── framework/           # 框架基础设施：SqlSugar、缓存、认证、操作横切等
├── module/              # 功能模块：rbac、tenant-management、audit-logging 等
└── src/                 # 应用组装层：启动项目、模块引用、统一配置
    ├── Yi.Abp.Web/
    ├── Yi.Abp.Domain/
    ├── Yi.Abp.Domain.Shared/
    ├── Yi.Abp.Application/
    ├── Yi.Abp.Application.Contracts/
    └── Yi.Abp.SqlSugarCore/
```

| 目录 | 职责 | 是否建议写业务 |
|------|------|----------------|
| `framework/` | 通用基础设施、横切能力、技术适配 | 否，除非在扩展框架能力 |
| `module/` | 独立功能模块和业务模块 | 是，主要开发区域 |
| `src/` | 应用入口和模块组装 | 只放启动配置、组合逻辑和少量全局能力 |

## src 组装层

`src` 不是业务模块目录，它负责把各个模块组合成最终应用：

1. 通过 `[DependsOn]` 引用模块。
2. 配置 JWT、Swagger、Hangfire、跨域、统一返回等启动能力。
3. 放置少量应用级后台任务或全局设置。

```csharp
[DependsOn(
    typeof(YiAbpApplicationContractsModule),
    typeof(YiAbpDomainModule),
    typeof(YiModuleRbacApplicationModule),
    typeof(YiModuleTenantManagementApplicationModule),
    typeof(YiModuleAuditLoggingApplicationModule)
    // ~~typeof(YiFrameworkCodeGenApplicationModule)~~
    // 2.0 起 code-gen 模块已移除，代码生成改为使用 Skills。
)]
public class YiAbpApplicationModule : AbpModule
{
}
```

| 场景 | 推荐位置 |
|------|----------|
| 添加后台任务 | `src/Yi.Abp.Web/Jobs/` |
| 添加全局设置定义 | `src/Yi.Abp.Domain.Shared/Settings/` |
| 修改启动配置 | `src/Yi.Abp.Web/YiAbpWebModule.cs` |
| 引用新模块 | `src/Yi.Abp.*/YiAbp*Module.cs` |

::: warning 注意
业务功能优先放到 `module/` 下的新模块，不要直接堆在 `src/`。`src` 的职责是组装，不是承载业务边界。
:::

## 模块分层

标准业务模块通常包含 5 层项目：

```text
module/{kebab-module-name}/
├── Yi.Module.{Module}.Domain.Shared/
├── Yi.Module.{Module}.Domain/
├── Yi.Module.{Module}.Application.Contracts/
├── Yi.Module.{Module}.Application/
└── Yi.Module.{Module}.SqlSugarCore/
```

可选情况下可以增加 Web 层，但当前项目主要通过应用服务和 ABP Conventional Controllers 暴露接口。

### Domain.Shared

最底层共享契约，适合放：

- 枚举
- 常量
- 跨模块共享的轻量类型
- 集成事件契约

### Domain

领域层只依赖 `Domain.Shared`，适合放：

- 聚合根和实体
- 值对象
- 领域服务，也就是 `Managers/`
- 仓储接口
- 领域事件处理器

### Application.Contracts

应用契约层是模块对外公开的应用 API：

- DTO：`Dtos/{Entity}/`
- 服务接口：`IServices/`

跨模块同步调用优先依赖这一层，而不是引用对方的 `Domain`。

### Application

应用层实现用例和接口编排：

- CRUD 服务实现
- 通用业务服务
- SignalR Hub
- 应用事件处理
- 防腐层适配器实现

### SqlSugarCore

基础设施层负责数据访问：

- 仓储实现
- CodeFirst 表映射补充
- 数据种子

框架已经封装了通用仓储，普通 CRUD 不需要额外创建仓储实现。

## 模块内部目录

Domain 层常见目录：

```text
Entities/              # 聚合根和实体
Entities/ValueObjects/ # 值对象
Managers/              # 领域服务
Repositories/          # 仓储接口
EventHandlers/         # 领域事件处理器
Authorization/         # 权限相关，rbac 模块使用
```

Application 层常见目录：

```text
Services/              # 应用服务
Services/System/       # 系统管理服务
Services/Monitor/      # 监控服务
Adapters/              # 防腐层适配器
SignalRHubs/           # 实时通信 Hub
```

Application.Contracts 层常见目录：

```text
Dtos/{Entity}/
IServices/
```

## 模块边界

**禁止跨模块直接引用 Domain 层。** 模块 A 不应依赖模块 B 的实体、聚合根、领域服务和仓储接口。

| 层 | 是否允许跨模块引用 | 说明 |
|----|--------------------|------|
| `Domain.Shared` | 允许 | 枚举、常量、集成事件等共享契约 |
| `Application.Contracts` | 允许 | 服务接口和 DTO |
| `Domain` | 禁止 | 模块内部领域模型 |
| `Application` | 禁止 | 模块内部用例实现 |
| `SqlSugarCore` | 禁止 | 模块内部数据访问实现 |

跨模块交互优先级：

1. 注入目标模块的 `Application.Contracts` 服务接口。
2. 在消费方定义防腐层适配器。
3. 使用集成事件做异步解耦。

## 相关文档

- [命名规范](/guide/backend/naming)
- [实体定义](/guide/backend/entity)
- [模块开发](/guide/backend/module)
