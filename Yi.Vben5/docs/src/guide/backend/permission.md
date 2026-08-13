# 权限与操作记录

权限和操作记录由授权抽象层与操作记录模块共同提供。Service 通过 `PermissionResource` 声明资源，通过 `PermissionAction` 声明动作，通过 `OperLogEntity` 和 `OperLog` 声明操作记录元数据。

## 权限码格式

标准格式：

```text
{module}:{entity}:{action}
```

示例：

```text
system:user:query
system:user:add
system:user:edit
system:user:remove
monitor:operlog:query
```

## 平台超管与租户管理员

2.1 起，平台超级管理员角色码统一为 `superadmin`，仅宿主机允许存在。租户侧管理员角色码继续使用 `admin`，不要在租户中创建 `superadmin` 角色。

平台超管账号会返回 `*:*:*` 权限码，可访问全部平台能力。租户管理员只能获得租户套餐分配的菜单与按钮权限，默认不包含租户管理、租户套餐、接口文档等平台治理能力。

当前服务层推荐使用统一动作：

| 动作 | 含义 |
|------|------|
| `query` | 查询、详情、下拉、树等读操作 |
| `add` | 新增、上传、创建等写入操作 |
| `edit` | 编辑、状态变更、同步等更新操作 |
| `remove` | 删除、清空等移除操作 |
| `export` | 导出操作 |
| `import` | 导入操作 |

## 权限解析顺序

接口权限准入顺序：

1. `[AllowAnonymous]` 放行。
2. `[IgnorePermission]` 放行。
3. ABP `[RemoteService(false)]` 禁用的接口放行。
4. 命中 `PermissionOptions.WhitelistActions` 放行。
5. 显式 `[Permission("...")]`，校验指定权限码。
6. 自动推断出权限码时，根据 `AutoCheckResolvedActions` 决定是否校验。
7. 未能推断权限码时，根据 `AllowUnresolvedActions` 决定放行或 403。

## Service 权限配置

### 声明资源

在应用服务类上使用 `[PermissionResource]` 声明模块码和资源码：

```csharp
[PermissionResource("system", "file")]
[OperLogEntity("文件")]
public class FileService : YiCrudAppService<FileAggregateRoot, FileGetListOutputDto, Guid, FileGetListInputVo>
{
}
```

资源声明后，方法动作会组合为：

```text
system:file:{action}
```

例如：

```text
system:file:query
system:file:add
system:file:remove
```

### 声明动作

新增代码推荐使用 `PermissionActionEnum`，避免裸字符串拼写错误：

```csharp
[PermissionAction(PermissionActionEnum.Query)]
public async Task<FileStreamResult> DownloadAsync(Guid id)
{
}
```

常用枚举：

| 枚举 | 输出动作 |
|------|----------|
| `PermissionActionEnum.Query` | `query` |
| `PermissionActionEnum.Add` | `add` |
| `PermissionActionEnum.Edit` | `edit` |
| `PermissionActionEnum.Remove` | `remove` |
| `PermissionActionEnum.Export` | `export` |
| `PermissionActionEnum.Import` | `import` |

字符串构造方式仍可兼容历史代码，但新增功能优先使用枚举。

### CRUD 基类

继承 `YiCrudAppService` 的标准 CRUD 方法会带有统一动作语义。业务服务只需要在类上声明资源和操作记录实体：

```csharp
[PermissionResource("system", "tenantPackage")]
[OperLogEntity("租户套餐")]
public class TenantPackageService : YiCrudAppService<TenantPackageAggregateRoot, TenantPackageGetOutputDto, TenantPackageGetListOutputDto, Guid,
    TenantPackageGetListInputVo, TenantPackageCreateInputVo, TenantPackageUpdateInputVo>
{
}
```

对非标准方法，应显式声明动作：

```csharp
[PermissionAction(PermissionActionEnum.Query)]
public async Task<MenuTreeResultDto> GetMenuTreeAsync(Guid? packageId)
{
}
```

## 自动推断规则

模块名目前基于命名空间推断：

| 命名空间 | 模块码 |
|----------|--------|
| `Yi.Module.Rbac.*` | `system` |
| `Yi.Module.TenantManagement.*` | `system` |
| `Yi.Module.SettingManagement.*` | `system` |
| `Yi.Module.AuditLogging.*` | `monitor` |

实体名来自服务类名，去掉 `Service` 后转小写：

```text
UserService -> user
OperationLogService -> operationlog
```

标准方法动作映射：

| 方法 | action |
|------|--------|
| `GetListAsync` / `GetSelectDataListAsync` / `GetAsync` | `query` |
| `CreateAsync` / `Post*` | `add` |
| `UpdateAsync` / `Put*` | `edit` |
| `DeleteAsync` / `Delete*` / `Remove*` / `Clear*` | `remove` |
| `ExportAsync` / `PostExportAsync` | `export` |
| `ImportAsync` / `PostImportExcelAsync` | `import` |

::: warning 兼容说明
如果历史菜单种子中存在自定义权限码，例如 `resetPwd`，应通过显式 `[Permission]` 或配置映射保持兼容。新增功能建议使用统一动作。
:::

## 显式权限

当自动推断不满足需求时，在方法上声明：

```csharp
[Permission("system:user:query")]
public override async Task<PagedResultDto<UserGetListOutputDto>> GetListAsync(UserGetListInputVo input)
{
}
```

显式权限优先级最高。

## 忽略权限

类或方法上使用 `[IgnorePermission]` 可跳过权限检查：

```csharp
[IgnorePermission]
public async Task<LoginOutputDto> LoginAsync(LoginInputVo input)
{
}
```

## 配置映射

`appsettings.json` 中可以配置权限映射：

```json
{
  "Operation": {
    "Permission": {
      "AutoCheckResolvedActions": true,
      "AllowUnresolvedActions": true,
      "WhitelistActions": [],
      "Mappings": {
        "Yi.Module.Rbac.Application.Services.System.UserService.ResetPasswordAsync": "system:user:resetPwd"
      }
    }
  }
}
```

映射优先级低于 `[Permission]`，高于自动推断。

## 操作记录

操作记录同样使用 Service 的 Action 元数据。

类上可以声明实体显示名：

```csharp
[OperLogEntity("文件")]
public class FileService : ApplicationService
{
}
```

方法上可以显式声明日志标题和操作类型：

```csharp
[OperLog("添加用户", OperEnum.Insert)]
public override async Task<UserGetOutputDto> CreateAsync(UserCreateInputVo input)
{
}
```

如果没有显式 `[OperLog]`，系统会根据 action 和 `[OperLogEntity]` 推断日志类型和标题。

对于特殊业务动作，建议显式声明：

```csharp
[PermissionAction(PermissionActionEnum.Edit)]
[OperLog("同步租户套餐", OperEnum.Update)]
public async Task SyncPackageAsync(Guid tenantId, Guid packageId)
{
}
```

## 前端按钮权限

前端按钮权限使用后端返回的 `permissionCodes`：

```vue
<Button v-access:code="['system:user:add']">新增</Button>
```

登录后 `getUserInfoApi()` 返回用户、角色码和权限码，前端会写入 `accessStore.accessCodes`。

按钮权限必须与后端接口权限保持一致。例如通知公告“推送”接口复用 `system:notice:edit`，前端按钮也应使用 `system:notice:edit`，不要额外发明 `system:notice:send`。

## 路由菜单父级补全

普通角色授权时可能只勾选子菜单或按钮。后端 `/account/router` 在构建路由前会递归补齐缺失的父级菜单，避免子菜单因为缺少目录节点而无法进入最终路由树。

父级补全只用于路由结构完整性，不会额外授予按钮权限。按钮权限仍以角色实际关联菜单的 `PermissionCode` 为准。

## 数据权限

接口权限决定用户能否调用某个接口，数据权限决定接口返回哪些业务数据。两者必须分别设计，拥有 `system:post:query` 只能说明用户可以查询岗位，不能因此返回全部部门的岗位数据。

### 数据范围

角色支持以下数据范围：

| 范围 | 含义 |
|------|------|
| `ALL` | 全部数据 |
| `CUSTOM` | 角色关联的自定义部门数据 |
| `DEPT` | 当前用户所属部门数据 |
| `DEPT_FOLLOW` | 当前用户所属部门及全部下级部门数据 |
| `USER` | 当前用户本人创建的数据 |

用户拥有多个角色时按并集计算。任一角色为 `ALL` 时直接放开全部数据；其他范围合并部门标识与本人创建条件。角色数据范围会写入登录令牌，修改角色后必须退出并重新登录。

### 核心契约

`IDataPermission` 与 `IHasDataPermission` 职责不同：

| 契约 | 职责 |
|------|------|
| `IDataPermission` | ABP `IDataFilter` 的开关标记，用于临时启停数据权限过滤 |
| `IHasDataPermission` | 业务实体主动接入数据权限的能力接口 |
| `IDataPermissionScopeProvider` | 从当前用户及令牌声明读取尚未展开数据库关系的数据范围 |
| `DataPermissionScopeResult` | 统一表达全部、本人、当前部门、下级部门和自定义角色范围 |

业务实体只有实现 `IHasDataPermission` 才会进入通用 SqlSugar 过滤。该机制采用 opt-in 设计，避免新增实体在未确认部门字段和创建者语义时被意外过滤。

```csharp
public class PostAggregateRoot : AggregateRoot<Guid>, IHasDataPermission
{
    public Guid DeptId { get; set; }

    Guid? IHasDataPermission.DataPermissionDeptId => DeptId;

    public Guid? CreatorId { get; set; }

    Guid? IHasDataPermission.DataPermissionCreatorId => CreatorId;
}
```

接入实体时必须确认：

- 部门字段表达数据归属部门，而不是审批部门、展示部门等其他业务概念
- 创建者字段可用于 `USER` 范围，且历史数据不会大量为空
- 列表、详情、导出和下拉查询都经过同一仓储与全局过滤链路
- 后台任务、数据迁移等确需读取全部数据的场景，应在最小作用域内显式关闭过滤

### 特殊实体

`UserAggregateRoot` 保留用户列表专用规则：本人范围按用户 `Id` 过滤，部门范围按用户 `DeptId` 过滤。

`RoleAggregateRoot` 不参与数据权限过滤。角色是授权配置数据，如果按业务数据范围裁剪，会导致角色配置不完整。

`DeptAggregateRoot` 不实现 `IHasDataPermission`。部门是范围配置和组织树来源，角色自定义部门配置必须能够读取完整组织数据；需要在业务页面展示部门树时，由具体查询接口按当前范围显式裁剪。

### 公共读取依赖

岗位、用户等页面通常依赖部门选择数据和状态字典。如果要求调用方同时拥有 `system:dept:query` 或 `system:dict:query`，只授权岗位管理的角色会收到 403。

以下接口属于已登录用户的公共读取依赖：

| 接口 | 资源权限 | 数据边界 |
|------|----------|----------|
| `GET /api/dept/select-data-list` | 使用 `[IgnorePermission]` 跳过部门管理权限 | 仍按当前数据范围裁剪，并补齐树结构所需上级部门节点 |
| `GET /api/dictionary/dict-type/{dictType}` | 使用 `[IgnorePermission]` 跳过字典管理权限 | 只返回指定类型下启用的字典项 |

`[IgnorePermission]` 只跳过资源权限校验，不等于 `[AllowAnonymous]`，ABP 默认认证仍然生效。公共读取接口不得为了消除 403 直接改为匿名访问。

部门树中补齐的上级部门节点只用于维持层级结构，不代表用户获得这些上级部门的业务数据权限。各范围的部门树结果如下：

| 范围 | 部门选择树 |
|------|------------|
| `ALL` | 完整部门树 |
| `CUSTOM` | 从顶级部门到自定义部门的完整路径 |
| `DEPT` | 从顶级部门到当前部门的完整路径 |
| `DEPT_FOLLOW` | 从顶级部门到当前部门的完整路径，以及当前部门的全部子部门 |
| `USER` | 空部门树 |

### 临时关闭过滤

只有授权配置、系统维护或明确的跨范围后台任务可以临时关闭数据权限过滤，并且必须限制作用域：

```csharp
using (DataFilter.Disable<IDataPermission>())
{
    // 仅在该作用域内读取未裁剪数据
}
```

不要在普通列表、详情、导出或下拉接口中关闭过滤。调用框架过滤 API 时，需要在代码注释中说明绕过原因、权限边界以及框架升级后的复核要求。

### 开发验证

接入数据权限后，应分别覆盖五种数据范围和多角色并集场景，并验证分页总数、查询条件、详情、导出和下拉接口无法绕过数据范围。部门树应符合前述上级部门节点补齐规则，公共读取依赖不得因缺少对应管理菜单权限而返回 403。

## 相关文档

- [模块开发](/guide/backend/module)
- [基础能力配置](/guide/backend/infrastructure)
- [菜单管理](/guide/frontend/features/menu)
- [前端权限](/guide/in-depth/access)
