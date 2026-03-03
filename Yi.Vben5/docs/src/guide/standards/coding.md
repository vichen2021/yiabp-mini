# 编码规范

## 后端编码规范

### 依赖注入

构造函数注入使用**元组解构**赋值模式：

```csharp
public UserService(ISqlSugarRepository<UserAggregateRoot, Guid> repository,
    UserManager userManager, ICurrentUser currentUser) : base(repository) =>
    (_userManager, _currentUser, _repository) =
    (userManager, currentUser, repository);
```

### 对象映射

使用 Mapster（非 AutoMapper）：

```csharp
userRoleMenu.User = user.Adapt<UserDto>();
role.Adapt<RoleDto>();
```

### 查询模式

使用 SqlSugar 的 `WhereIF` 进行条件过滤：

```csharp
var outPut = await _repository._DbQueryable
    .WhereIF(!string.IsNullOrEmpty(input.UserName), x => x.UserName.Contains(input.UserName!))
    .WhereIF(input.State is not null, x => x.State == input.State)
    .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
```

## 前端编码规范

### TypeScript

- 使用 TypeScript 5.8
- 严格类型检查
- 避免使用 `any`

### 代码风格

- 遵循 ESLint 规则
- 使用 Prettier 格式化
- 统一的命名规范

## 相关文档

- [命名规范](/guide/backend/naming) - 了解命名规范
- [Git 提交规范](/guide/standards/git) - 了解 Git 规范

