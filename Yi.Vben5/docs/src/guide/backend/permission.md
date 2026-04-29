# 权限与操作日志

2.0 后权限和操作日志由 `Yi.Framework.Operation` 提供统一的 Action 元数据解析能力。权限码和操作日志标题可以自动推断，也可以通过特性显式指定。

## 权限码格式

标准格式：

```text
{module}:{entity}:{action}
```

示例：

```text
system:user:list
system:user:add
system:user:edit
system:user:delete
monitor:operlog:list
```

## 权限解析顺序

接口权限准入顺序：

1. `[AllowAnonymous]` 放行。
2. `[IgnorePermission]` 放行。
3. ABP `[RemoteService(false)]` 禁用的接口放行。
4. 命中 `PermissionOptions.WhitelistActions` 放行。
5. 显式 `[Permission("...")]`，校验指定权限码。
6. 自动推断出权限码时，根据 `AutoCheckResolvedActions` 决定是否校验。
7. 未能推断权限码时，根据 `AllowUnresolvedActions` 决定放行或 403。

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
| `GetListAsync` / `GetSelectDataListAsync` | `list` |
| `GetAsync` | `detail` |
| `CreateAsync` / `Post*` | `add` |
| `UpdateAsync` / `Put*` | `edit` |
| `DeleteAsync` / `Delete*` / `Remove*` / `Clear*` | `delete` |
| `ExportAsync` / `GetExportExcelAsync` | `export` |
| `ImportAsync` / `PostImportExcelAsync` | `import` |

::: warning 兼容说明
历史菜单种子中存在 `remove`、`query`、`resetPwd` 等权限码。新功能建议使用标准 action；历史权限码如需保留，应通过显式 `[Permission]` 或配置映射保持兼容。
:::

## 显式权限

当自动推断不满足需求时，在方法上声明：

```csharp
[Permission("system:user:list")]
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

## 操作日志

操作日志同样使用 Action 元数据。

类上可以声明实体显示名：

```csharp
[OperLogEntity("用户")]
public class UserService : ApplicationService
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

如果没有显式 `[OperLog]`，系统会根据 action 推断日志类型和标题。

## 前端按钮权限

前端按钮权限使用后端返回的 `permissionCodes`：

```vue
<a-button v-access:code="['system:user:add']">新增</a-button>
```

登录后 `getUserInfoApi()` 返回用户、角色码和权限码，前端会写入 `accessStore.accessCodes`。

## 相关文档

- [模块开发](/guide/backend/module)
- [菜单管理](/guide/frontend/features/menu)
- [前端权限](/guide/in-depth/access)
