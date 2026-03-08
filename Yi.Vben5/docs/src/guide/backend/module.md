# 模块开发

## 新建模块检查清单

新建业务模块时，确保：

1. 创建完整的 5 层项目结构（Domain.Shared / Domain / Application.Contracts / Application / SqlSugarCore / Web(可选)）
2. 每层创建对应的 ABP Module 类并声明 `[DependsOn]`
3. 在 `YiAbpWebModule.cs` 中注册动态 API（`ConventionalControllers.Create`）
4. 在主模块中添加对新模块 Application 层的依赖
5. DTO 遵循 Input 用 `Vo` 后缀、Output 用 `Dto` 后缀的规范

## 使用模块生成器

推荐使用 Claude Skills 中的 **模块生成器** 自动生成模块结构：

查看 [模块生成器文档](/guide/skills/module-generator) 了解如何使用。

## 使用 Yi.Abp.Tool

[Yi.Abp.Tool](https://ccnetcore.com/article/aaa00329-7f35-d3fe-d258-3a0f8380b742/4264aef4-979f-f533-dc79-3a100334d6a8) 提供 Tool 工具自动生成模块结构，但是仍然需要逐个为新模块添加项目引用。

## 模块依赖关系

每个层都是 ABP 模块，通过 `[DependsOn]` 声明依赖关系：

```csharp
[DependsOn(
    typeof(YiFrameworkRbacApplicationContractsModule),
    typeof(YiFrameworkRbacDomainModule),
    typeof(YiFrameworkDddApplicationModule))]
public class YiFrameworkRbacApplicationModule : AbpModule
```

---

## 应用服务开发

::: tip 推荐
开发完整的 CRUD 功能时，推荐使用 [CRUD 生成器](/guide/skills/crud-generator) 自动生成后端和前端代码，包含实体、DTO、服务、菜单种子数据、API 和视图组件。
:::

### 应用服务基类

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

### 列表查询 DTO

所有列表查询 DTO 继承 `PagedAllResultRequestDto`，自带时间范围过滤和动态排序。

### 依赖注入

构造函数注入使用**元组解构**赋值模式：

```csharp
public UserService(ISqlSugarRepository<UserAggregateRoot, Guid> repository,
    UserManager userManager, ICurrentUser currentUser) : base(repository) =>
    (_userManager, _currentUser, _repository) =
    (userManager, currentUser, repository);
```

### 服务注册

服务注册遵循 ABP 约定自动注册，显式注册使用 `ITransientDependency`：

```csharp
public class SomeAdapter : ISomeAdapter, ITransientDependency
```

### 对象映射

使用 Mapster（非 AutoMapper）：

```csharp
userRoleMenu.User = user.Adapt<UserDto>();
role.Adapt<RoleDto>();
```

---

## 动态 API

### Conventional Controllers

项目使用 ABP **动态 API（Conventional Controllers）**，应用服务自动映射为 REST 接口，无需手动编写 Controller。

### 自动映射规则

| 服务方法 | HTTP 接口 |
|---------|----------|
| `UserService.GetListAsync()` | `GET /api/user` |
| `UserService.CreateAsync()` | `POST /api/user` |
| `UserService.UpdateAsync()` | `PUT /api/user/{id}` |
| `UserService.DeleteAsync()` | `DELETE /api/user/{id}` |

---

## 查询模式

### WhereIF 条件过滤

使用 SqlSugar 的 `WhereIF` 进行条件过滤，这是项目中最常见的查询模式：

```csharp
var outPut = await _repository._DbQueryable
    .WhereIF(!string.IsNullOrEmpty(input.UserName), x => x.UserName.Contains(input.UserName!))
    .WhereIF(input.State is not null, x => x.State == input.State)
    .WhereIF(input.StartTime is not null && input.EndTime is not null,
        x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)
    .LeftJoin<DeptAggregateRoot>((user, dept) => user.DeptId == dept.Id)
    .OrderByDescending(user => user.CreationTime)
    .Select((user, dept) => new UserGetListOutputDto(), true)
    .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
```

### 关联查询

使用 `LeftJoin` 进行关联查询：

```csharp
.LeftJoin<DeptAggregateRoot>((user, dept) => user.DeptId == dept.Id)
```

### 排序

使用 `OrderByDescending` 或 `OrderBy` 进行排序：

```csharp
.OrderByDescending(user => user.CreationTime)
```

### 分页

使用 `ToPageListAsync` 进行分页查询：

```csharp
.ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
```

---

## 相关文档

- [架构设计](/guide/backend/architecture) - 了解架构设计
- [命名规范](/guide/backend/naming) - 了解命名规范
- [实体定义](/guide/backend/entity) - 了解实体定义
- [模块生成器](/guide/skills/module-generator) - 使用 AI 工具生成模块
- [CRUD 生成器](/guide/skills/crud-generator) - 自动生成完整 CRUD 功能
- [意社区](https://ccnetcore.com/) - .net 交流社区，包含 Yi 框架开发文档和模块生成工具
