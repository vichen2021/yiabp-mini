

# Setting-Management.Domain 文件全解（按职责分层）

文件多是因为这是 ABP **设置管理框架的本地化复刻** —— ABP 官方的 setting 模块基于 EF Core，团队照抄一份改成 SqlSugar 就形成了这个目录。

---

## 第一层：实体 + 持久化（2 个）

| 文件 | 作用 |
|---|---|
| [SettingAggregateRoot.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/SettingAggregateRoot.cs:0:0-0:0) | 数据库实体，对应 `Setting` 表（Name / Value / ProviderName / ProviderKey） |
| [AbpSettingManagementDbProperties.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/AbpSettingManagementDbProperties.cs:0:0-0:0) | 表名/Schema 常量 |

---

## 第二层：仓储 + Store（4 个）

`Repository → ManagementStore → Store` 三层套娃，是 ABP 原版照搬：

| 文件 | 作用 |
|---|---|
| [ISettingRepository.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/ISettingRepository.cs:0:0-0:0) | 仓储接口（具体实现在 `SqlSugarCore` 项目） |
| [ISettingManagementStore.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/ISettingManagementStore.cs:0:0-0:0) | "管理用"存储接口（CRUD 全套） |
| [SettingManagementStore.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/SettingManagementStore.cs:0:0-0:0) | 调 Repository 实现读写 + 缓存失效事件 |
| [SettingStore.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/SettingStore.cs:0:0-0:0) | 适配 ABP `ISettingStore`（**只读**接口，给 ABP `ISettingProvider` 用） |

> **为什么有两个 Store？** ABP 把"读"和"管理"拆开：业务读用 `ISettingStore`（轻量），管理改用 `ISettingManagementStore`（全套 CRUD）。

---

## 第三层：缓存（2 个）

| 文件 | 作用 |
|---|---|
| [SettingCacheItem.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/SettingCacheItem.cs:0:0-0:0) | 分布式缓存项，缓存 key 计算规则 |
| [SettingCacheItemInvalidator.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/SettingCacheItemInvalidator.cs:0:0-0:0) | 监听 `Setting` 实体变更事件，清缓存 |

---

## 第四层：5 级 Provider（5 个 + 1 个抽象基类 = 6 个）

这是 Setting 表「真正区别于 Config 表」的核心 —— **多级隔离 + fallback 链**：

| 文件 | 作用 | 优先级 |
|---|---|---|
| [UserSettingManagementProvider.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/UserSettingManagementProvider.cs:0:0-0:0) | 当前用户级（`providerKey = userId`） | 最高 |
| [TenantSettingManagementProvider.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/TenantSettingManagementProvider.cs:0:0-0:0) | 当前租户级（`providerKey = tenantId`） | 次高 |
| [GlobalSettingManagementProvider.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/GlobalSettingManagementProvider.cs:0:0-0:0) | 全局级（`providerKey = null`） | 中 |
| [ConfigurationSettingManagementProvider.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/ConfigurationSettingManagementProvider.cs:0:0-0:0) | 从 [appsettings.json](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/src/Yi.Abp.Web/appsettings.json:0:0-0:0) 读 | 低 |
| [DefaultValueSettingManagementProvider.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/DefaultValueSettingManagementProvider.cs:0:0-0:0) | `SettingDefinition` 里写的默认值 | 最低 |
| [SettingManagementProvider.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/SettingManagementProvider.cs:0:0-0:0) | 上面前 3 个的抽象基类（共享 Store CRUD 逻辑） | — |

> 5 个 Provider 的差异**仅在 [NormalizeProviderKey](cci:1://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/SettingManagementProvider.cs:32:4-35:5)**：User 用 userId、Tenant 用 tenantId、Global 强制 null、Configuration 走 IConfiguration、DefaultValue 走 SettingDefinition。

---

## 第五层：Provider 接口 + 注册（2 个）

| 文件 | 作用 |
|---|---|
| [ISettingManagementProvider.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/ISettingManagementProvider.cs:0:0-0:0) | 5 个 Provider 共同实现的接口 |
| [SettingManagementOptions.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/SettingManagementOptions.cs:0:0-0:0) | 注册 Provider 列表的选项类（控制查询顺序） |

---

## 第六层：核心调度器（2 个）

| 文件 | 作用 |
|---|---|
| [ISettingManager.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/ISettingManager.cs:0:0-0:0) / [SettingManager.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/SettingManager.cs:0:0-0:0) | **入口**。按 `SettingManagementOptions` 列出的 Provider 顺序，依次查询直到命中（即 fallback 链的真正实现） |

---

## 第七层：开发者快捷扩展方法（5 个）

让业务代码不用每次都填 `providerName` / `providerKey`，直接 `SetForCurrentTenantAsync(name, value)`：

| 文件 | 作用 |
|---|---|
| [UserSettingManagerExtensions.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/UserSettingManagerExtensions.cs:0:0-0:0) | `GetForCurrentUserAsync` / `SetForCurrentUserAsync` |
| [TenantSettingManagerExtensions.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/TenantSettingManagerExtensions.cs:0:0-0:0) | `GetOrNullForCurrentTenantAsync` / `SetForTenantAsync` |
| [GlobalSettingManagerExtensions.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/GlobalSettingManagerExtensions.cs:0:0-0:0) | `SetGlobalAsync` |
| [ConfigurationValueSettingManagerExtensions.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/ConfigurationValueSettingManagerExtensions.cs:0:0-0:0) | 读 appsettings 的便捷方法 |
| [DefaultValueSettingManagerExtensions.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/DefaultValueSettingManagerExtensions.cs:0:0-0:0) | 读 SettingDefinition 默认值 |

---

## 第八层：模块入口（1 个）

| 文件 | 作用 |
|---|---|
| [YiModuleSettingManagementDomainModule.cs](cci:7://file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/setting-management/Yi.Module.SettingManagement.Domain/YiModuleSettingManagementDomainModule.cs:0:0-0:0) | 注册 5 个 Provider 到 `SettingManagementOptions.Providers`（决定 fallback 链顺序）|

---

## 总结：文件多但**结构清晰，零冗余**

```
入口层    ISettingManager / SettingManager
          ↓
Provider 链   User → Tenant → Global → Configuration → Default
          ↓
存储层    ISettingManagementStore → SettingStore → Repository
          ↓
实体      SettingAggregateRoot
缓存      SettingCacheItem + Invalidator
扩展      *Extensions（语法糖）
```

**对比 Config 表**：Config 只需要一个 [ConfigService](file:///e:/WCG/Private/WJ-CMS-Agent-Work/WorkTrees/master-branch/Yi.Abp/module/rbac/Yi.Module.Rbac.Application/Services/System/ConfigService.cs:19:4-82:5) + 一个实体就跑起来了，因为它没有 fallback、没有多租户隔离、没有加密、没有 SettingDefinition 契约 —— **这 27 个文件解决的所有事 Config 一个都不解决**。

这也正是为什么 OSS 租户隔离方案必须落到 Setting 上 —— Config 表如果要做同样的事，等于把这 27 个文件再写一遍。