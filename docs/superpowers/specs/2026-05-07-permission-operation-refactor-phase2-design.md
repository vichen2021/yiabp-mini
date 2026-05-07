# 权限与操作日志过滤机制优化 - Phase 2 设计文档

**状态**: 已审核通过

**Goal:** 拆分权限和日志决策，创建独立的 Resolver，两者共享 IActionIdentityResolver。

**Architecture:**
- 新增 `PermissionRequirement` 模型（不含日志字段）
- 新增 `OperationLogRequirement` 模型（不含权限字段）
- 新增 `IPermissionRequirementResolver` + `DefaultPermissionRequirementResolver`
- 新增 `IOperationLogRequirementResolver` + `DefaultOperationLogRequirementResolver`
- `PermissionAuthorizationFilter` 改用 `IPermissionRequirementResolver`
- `DefaultOperationLogMapper` 改用 `IOperationLogRequirementResolver`
- 保留旧 `IActionMetadataResolver`（Phase 5 清理）

**Tech Stack:** .NET 10, ABP Framework 10.0.2

---

## 文件结构

| 文件 | 责任 | 层 |
|------|------|------|
| `PermissionRequirement.cs` | 权限要求模型 | Abstractions |
| `IPermissionRequirementResolver.cs` | 权限解析接口 | Abstractions |
| `OperationLogRequirement.cs` | 日志要求模型 | Abstractions |
| `IOperationLogRequirementResolver.cs` | 日志解析接口 | Abstractions |
| `DefaultPermissionRequirementResolver.cs` | 权限解析实现 | Core |
| `DefaultOperationLogRequirementResolver.cs` | 日志解析实现 | Core |
| `PermissionAuthorizationFilter.cs` | 改用新 Resolver | Core（修改） |
| `DefaultOperationLogMapper.cs` | 改用新 Resolver | AuditLogging（修改） |
| `YiFrameworkOperationCoreModule.cs` | 注册新 Resolver | Core（修改） |

---

## Section 1: PermissionRequirement 模型

```csharp
public sealed class PermissionRequirement
{
    /// <summary>是否忽略权限检查 ([IgnorePermission])</summary>
    public bool Ignore { get; set; }

    /// <summary>是否成功解析权限码</summary>
    public bool IsResolved { get; set; }

    /// <summary>权限码 (system:user:add)</summary>
    public string? Code { get; set; }

    /// <summary>权限码来源 (诊断: ExplicitPermission, PermissionAction+Resource)</summary>
    public string? Source { get; set; }

    /// <summary>未解析原因 (诊断)</summary>
    public string? UnresolvedReason { get; set; }
}
```

**解耦要点:** 不含 OperType、Title、IsWriteOperation 等日志字段。

---

## Section 2: OperationLogRequirement 模型

```csharp
public sealed class OperationLogRequirement
{
    /// <summary>是否忽略日志记录 ([IgnoreOperLog])</summary>
    public bool Ignore { get; set; }

    /// <summary>是否应该记录日志</summary>
    public bool ShouldLog { get; set; }

    /// <summary>操作类型</summary>
    public OperEnum? OperType { get; set; }

    /// <summary>日志标题</summary>
    public string? Title { get; set; }

    /// <summary>是否保存请求数据</summary>
    public bool SaveRequestData { get; set; }

    /// <summary>是否保存响应数据</summary>
    public bool SaveResponseData { get; set; }

    /// <summary>是否写操作</summary>
    public bool IsWriteOperation { get; set; }
}
```

**解耦要点:** 不含 Code、IsResolved、Source 等权限字段。

---

## Section 3: Resolver 接口

**IPermissionRequirementResolver:**

```csharp
public interface IPermissionRequirementResolver
{
    PermissionRequirement Resolve(ControllerActionDescriptor descriptor);
    PermissionRequirement Resolve(Type serviceType, MethodInfo methodInfo);
}
```

**IOperationLogRequirementResolver:**

```csharp
public interface IOperationLogRequirementResolver
{
    OperationLogRequirement Resolve(ControllerActionDescriptor descriptor);
    OperationLogRequirement Resolve(Type serviceType, MethodInfo methodInfo);
}
```

---

## Section 4: Resolver 实现逻辑

### DefaultPermissionRequirementResolver

**解析流程:**

```text
1. [IgnorePermission] → Ignore=true, 返回
2. [Permission("完整码")] → Code=完整码, Source="ExplicitPermission", 返回
3. 无 PermissionResource + 无 Module/Resource → IsResolved=false
4. 无 CrudAction → IsResolved=false
5. 组合权限码 → Code="{Module}:{Resource}:{CrudAction}"
```

**依赖:** IActionIdentityResolver (Phase 1)

### DefaultOperationLogRequirementResolver

**解析流程:**

```text
1. [IgnoreOperLog] → Ignore=true, 返回
2. [OperLog("标题", OperType)] → 使用显式值, ShouldLog=true, 返回
3. CrudAction 推断 OperType (add→Insert, edit→Update, remove→Delete)
4. [OperLogEntity("实体名")] → Title="添加实体名"
5. IsWriteOperation = CrudAction in [add, edit, remove, import, export]
6. ShouldLog = IsWriteOperation
```

**依赖:** IActionIdentityResolver (Phase 1)

---

## Section 5: Filter 和 Mapper 改造

### PermissionAuthorizationFilter

**变更:**

```csharp
// 构造函数
- IActionMetadataResolver _metadataResolver
+ IPermissionRequirementResolver _permissionResolver

// 权限校验
if (requirement.Ignore) return;
if (requirement.IsResolved && requirement.Code != null)
    await _permissionHandler.IsGrantedAsync(requirement.Code);
if (!requirement.IsResolved && !_options.AllowUnresolvedActions)
    context.Result = new ForbidResult();
```

### DefaultOperationLogMapper

**变更:**

```csharp
// 构造函数
- IActionMetadataResolver _metadataResolver
+ IOperationLogRequirementResolver _logResolver

// 日志记录
if (requirement.Ignore) return null;
if (!requirement.ShouldLog && !hasExplicitLog) return null;
// 使用 requirement.OperType, requirement.Title
```

---

## Section 6: 服务注册

```csharp
// YiFrameworkOperationCoreModule.cs
context.Services.AddTransient<IPermissionRequirementResolver, DefaultPermissionRequirementResolver>();
context.Services.AddTransient<IOperationLogRequirementResolver, DefaultOperationLogRequirementResolver>();

// 旧 Resolver 保留（Phase 5 清理）
context.Services.AddTransient<IActionMetadataResolver, DefaultActionMetadataResolver>();
```

---

## 验收标准

| 验收点 | 验证 |
|--------|------|
| 权限修改不影响日志 | PermissionRequirement 不含 OperType/Title |
| 日志修改不影响权限 | OperationLogRequirement 不含 Code |
| Filter 使用新 Resolver | 依赖注入 IPermissionRequirementResolver |
| Mapper 使用新 Resolver | 依赖注入 IOperationLogRequirementResolver |
| 构建 | dotnet build 0 errors |

---

## 迁移策略

采用渐进式替换（方案 A）：
- 新 Resolver 与旧 ActionMetadataResolver 并行运行
- Filter 和 Mapper 立即改用新 Resolver
- 旧 Resolver 暂时保留，供其他地方使用
- Phase 5 时删除旧 ActionMetadataResolver