# 命名规范

## 命名空间分层

2.0 后命名空间按职责区分：

| 类型 | 命名空间前缀 | 示例 |
|------|--------------|------|
| 框架基础设施 | `Yi.Framework.*` | `Yi.Framework.SqlSugarCore` |
| 功能/业务模块 | `Yi.Module.*` | `Yi.Module.Rbac.Application` |
| 应用组装层 | `Yi.Abp.*` | `Yi.Abp.Web` |

不要把业务模块命名为 `Yi.Framework.{Module}`。`framework` 只用于通用基础能力，例如 SqlSugar、缓存、统一返回、操作横切等。

## 项目命名

业务模块项目使用：

```text
Yi.Module.{Module}.Domain.Shared
Yi.Module.{Module}.Domain
Yi.Module.{Module}.Application.Contracts
Yi.Module.{Module}.Application
Yi.Module.{Module}.SqlSugarCore
```

示例：

```text
Yi.Module.Rbac.Domain
Yi.Module.Rbac.Application
Yi.Module.Rbac.Application.Contracts
Yi.Module.Rbac.SqlSugarCore
```

## ABP 模块类命名

| 项目 | 模块类 |
|------|--------|
| `Yi.Module.Rbac.Domain` | `YiModuleRbacDomainModule` |
| `Yi.Module.Rbac.Application` | `YiModuleRbacApplicationModule` |
| `Yi.Framework.SqlSugarCore` | `YiFrameworkSqlSugarCoreModule` |
| `Yi.Abp.Web` | `YiAbpWebModule` |

规则是：命名空间前缀决定类名前缀，`Yi.Module.*` 使用 `YiModule`，`Yi.Framework.*` 使用 `YiFramework`。

## 类命名

| 类型 | 命名规则 | 示例 |
|------|----------|------|
| 聚合根 | `{Name}AggregateRoot` | `UserAggregateRoot` |
| 普通实体 | `{Name}Entity` | `UserRoleEntity`, `DictionaryEntity` |
| 值对象 | `{Name}ValueObject` | `EncryptPasswordValueObject` |
| 应用服务 | `{Name}Service` | `UserService` |
| 领域服务 | `{Name}Manager` | `UserManager`, `AccountManager` |
| 服务接口 | `I{Name}Service` | `IUserService`, `IRoleService` |

## DTO 命名

输入用 `Vo` 后缀，输出用 `Dto` 后缀：

| 用途 | 命名规则 | 示例 |
|------|----------|------|
| 创建输入 | `{Entity}CreateInputVo` | `UserCreateInputVo` |
| 更新输入 | `{Entity}UpdateInputVo` | `UserUpdateInputVo` |
| 列表查询输入 | `{Entity}GetListInputVo` | `UserGetListInputVo` |
| 单条输出 | `{Entity}GetOutputDto` | `UserGetOutputDto` |
| 列表输出 | `{Entity}GetListOutputDto` | `UserGetListOutputDto` |

## 表名规范

数据库表名使用大驼峰，不加项目前缀：

```csharp
[SugarTable("User")]
public class UserAggregateRoot : AggregateRoot<Guid>
{
}
```

推荐：

```text
User
Role
Tenant
AuditLog
EntityChange
Setting
```

不推荐：

```text
YiUser
YiTenant
yi_user
rbac_user
```

只有在确实存在跨模块同名冲突时，才考虑加领域语义前缀，例如 `SystemConfig`。不要加 `Yi` 这种项目前缀。

## 变量命名

- 私有字段使用下划线前缀：`_repository`, `_userManager`, `_currentUser`
- 命名空间与文件夹结构一致：`Yi.Module.Rbac.Domain.Entities`
- 实体主键类型统一使用 `Guid`
- 前端 API 和视图目录使用 kebab-case 或既有业务目录风格，优先保持当前模块一致

## 相关文档

- [实体定义](/guide/backend/entity)
- [模块开发](/guide/backend/module)
