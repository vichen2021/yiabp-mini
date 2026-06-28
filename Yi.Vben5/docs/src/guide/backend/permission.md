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

## 相关文档

- [模块开发](/guide/backend/module)
- [基础能力配置](/guide/backend/infrastructure)
- [菜单管理](/guide/frontend/features/menu)
- [前端权限](/guide/in-depth/access)
