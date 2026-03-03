# 应用服务

## 应用服务基类

框架提供泛型 CRUD 基类 `YiCrudAppService`，内置 Excel 导入导出、批量删除等功能：

```csharp
public class UserService : YiCrudAppService<
    UserAggregateRoot,        // 实体
    UserGetOutputDto,         // 单条输出 DTO
    UserGetListOutputDto,     // 列表输出 DTO
    Guid,                     // 主键类型
    UserGetListInputVo,       // 列表查询输入
    UserCreateInputVo,        // 创建输入
    UserUpdateInputVo>        // 更新输入
```

## 列表查询 DTO

所有列表查询 DTO 继承 `PagedAllResultRequestDto`，自带时间范围过滤和动态排序。

## 依赖注入

构造函数注入使用**元组解构**赋值模式：

```csharp
public UserService(ISqlSugarRepository<UserAggregateRoot, Guid> repository,
    UserManager userManager, ICurrentUser currentUser) : base(repository) =>
    (_userManager, _currentUser, _repository) =
    (userManager, currentUser, repository);
```

## 服务注册

服务注册遵循 ABP 约定自动注册，显式注册使用 `ITransientDependency`：

```csharp
public class SomeAdapter : ISomeAdapter, ITransientDependency
```

## 对象映射

使用 Mapster（非 AutoMapper）：

```csharp
userRoleMenu.User = user.Adapt<UserDto>();
role.Adapt<RoleDto>();
```

## 相关文档

- [API 模式](/guide/backend/api) - 了解 API 自动映射
- [权限与日志](/guide/backend/permission) - 了解权限控制

