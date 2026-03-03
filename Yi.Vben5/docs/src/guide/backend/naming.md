# 命名规范

## 项目命名

项目遵循层级命名：`Yi.Framework.{模块名}.{层名}`

```
Yi.Framework.Rbac.Domain
Yi.Framework.Rbac.Application
Yi.Framework.Rbac.Application.Contracts
Yi.Framework.Rbac.SqlSugarCore
```

## 类命名

| 类型 | 命名规则 | 示例 |
|------|----------|------|
| 聚合根 | `{Name}AggregateRoot` | `UserAggregateRoot` |
| 普通实体 | `{Name}Entity` | `UserRoleEntity`, `DictionaryEntity` |
| 值对象 | `{Name}ValueObject` | `EncryPasswordValueObject` |
| 应用服务 | `{Name}Service` | `UserService` |
| 领域服务 | `{Name}Manager` | `UserManager`, `AccountManager` |
| 服务接口 | `I{Name}Service` | `IUserService`, `IRoleService` |
| ABP 模块 | `YiFramework{Module}{Layer}Module` | `YiFrameworkRbacApplicationModule` |

## DTO 命名

**注意：输入用 `Vo` 后缀，输出用 `Dto` 后缀。**

| 用途 | 命名规则 | 示例 |
|------|----------|------|
| 创建输入 | `{Entity}CreateInputVo` | `UserCreateInputVo` |
| 更新输入 | `{Entity}UpdateInputVo` | `UserUpdateInputVo` |
| 列表查询输入 | `{Entity}GetListInputVo` | `UserGetListInputVo` |
| 单条输出 | `{Entity}GetOutputDto` | `UserGetOutputDto` |
| 列表输出 | `{Entity}GetListOutputDto` | `UserGetListOutputDto` |

## 变量命名

- 私有字段使用下划线前缀：`_repository`, `_userManager`, `_currentUser`
- 命名空间与文件夹结构一致：`Yi.Framework.Rbac.Domain.Entities`
- 所有实体主键类型统一使用 `Guid`

## 相关文档

- [实体定义](/guide/backend/entity) - 了解如何定义实体
- [应用服务](/guide/backend/service) - 了解应用服务开发

