# 模块开发

## 新建模块检查清单

新建业务模块时，至少确认：

1. 在 `Yi.Abp/module/{kebab-module-name}/` 创建 5 层项目结构。
2. 项目命名使用 `Yi.Module.{Module}.*`。
3. 每层创建对应 ABP Module 类并声明 `[DependsOn]`。
4. 在 `src` 组装层添加项目引用和模块依赖。
5. 在 `YiAbpWebModule.cs` 中注册动态 API。
6. DTO 遵循输入 `Vo`、输出 `Dto` 的命名规范。

## 推荐生成方式

2.0 起推荐使用 Skills 生成模块和 CRUD：

- [模块生成器](/guide/skills/module-generator)
- [CRUD 生成器](/guide/skills/crud-generator)

## 历史工具

~~`Yi.Abp.Tool` 曾用于生成模块结构。~~

2.0 起该工具已移除，模块生成改为使用 Skill 实现。历史说明保留用于理解迁移背景，不建议继续使用旧 Tool 流程。

## 模块依赖关系

每层都是 ABP 模块，通过 `[DependsOn]` 声明依赖：

```csharp
[DependsOn(
    typeof(YiModuleRbacApplicationContractsModule),
    typeof(YiModuleRbacDomainModule),
    typeof(YiFrameworkDddApplicationModule))]
public class YiModuleRbacApplicationModule : AbpModule
{
}
```

依赖方向应保持单向：

```text
Application
  -> Application.Contracts
  -> Domain
  -> Domain.Shared

SqlSugarCore
  -> Domain
```

## 应用服务开发

完整 CRUD 推荐继承 `YiCrudAppService`：

```csharp
public class UserService : YiCrudAppService<
    UserAggregateRoot,
    UserGetOutputDto,
    UserGetListOutputDto,
    Guid,
    UserGetListInputVo,
    UserCreateInputVo,
    UserUpdateInputVo>,
    IUserService
{
}
```

### 列表查询 DTO

列表查询 DTO 通常继承 `PagedAllResultRequestDto`，用于分页、时间范围和动态排序。

::: warning 分页字段语义
`PagedAllResultRequestDto` 继承 ABP 分页字段名 `SkipCount/MaxResultCount`，但当前项目 CRUD 查询使用 SqlSugar `ToPageListAsync(pageIndex, pageSize, total)`。

因此在本项目列表接口中：

- `SkipCount` 表示从 1 开始的当前页码，不是 offset。
- `MaxResultCount` 表示每页大小。
- 前端应传 `SkipCount = currentPage`，不要传 `(currentPage - 1) * pageSize`。

当前不新增 `PageIndex/PageSize` 兼容 DTO，避免同一 API 同时暴露两套分页字段名。
:::

### 依赖注入

构造函数可以使用元组解构赋值：

```csharp
public UserService(
    ISqlSugarRepository<UserAggregateRoot, Guid> repository,
    UserManager userManager,
    ICurrentUser currentUser) : base(repository) =>
    (_repository, _userManager, _currentUser) =
    (repository, userManager, currentUser);
```

### 对象映射

项目使用 Mapster：

```csharp
var dto = entity.Adapt<UserGetOutputDto>();
```

## 动态 API

项目使用 ABP Conventional Controllers。应用服务会自动映射为 REST 接口，不需要手写 Controller。

当前 Web 启动模块中统一配置：

```csharp
options.ConventionalControllers.Create(
    typeof(YiModuleRbacApplicationModule).Assembly,
    options => options.RemoteServiceName = "rbac");

options.ConventionalControllers.ConventionalControllerSettings
    .ForEach(x => x.RootPath = "api");
```

因此接口根路径统一是 `/api`，不是 `/api/{module}`。

示例：

| 服务方法 | HTTP 接口 |
|----------|-----------|
| `UserService.GetListAsync()` | `GET /api/user` |
| `UserService.CreateAsync()` | `POST /api/user` |
| `UserService.UpdateAsync()` | `PUT /api/user/{id}` |
| `UserService.DeleteAsync()` | `DELETE /api/user/{id}` |

## 查询模式

常见查询使用 SqlSugar 的 `WhereIF`：

```csharp
var total = 0;
var output = await _repository._DbQueryable
    .WhereIF(!string.IsNullOrEmpty(input.UserName), x => x.UserName.Contains(input.UserName!))
    .WhereIF(input.State is not null, x => x.State == input.State)
    .WhereIF(input.StartTime is not null && input.EndTime is not null,
        x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)
    .LeftJoin<DeptAggregateRoot>((user, dept) => user.DeptId == dept.Id)
    .OrderByDescending(user => user.CreationTime)
    .Select((user, dept) => new UserGetListOutputDto(), true)
    .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
```

这里的 `input.SkipCount` 按 SqlSugar 页码传入，期望值为 `1, 2, 3...`，不是跳过记录数。

## 相关文档

- [架构设计](/guide/backend/architecture)
- [命名规范](/guide/backend/naming)
- [实体定义](/guide/backend/entity)
- [模块生成器](/guide/skills/module-generator)
- [CRUD 生成器](/guide/skills/crud-generator)
