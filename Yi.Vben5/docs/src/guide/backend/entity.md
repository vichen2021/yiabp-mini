# 实体定义

## 实体接口组合

实体使用 SqlSugar 特性进行 ORM 配置，常见接口组合为 `ISoftDelete`, `IAuditedObject`, `IOrderNum`, `IState`：

```csharp
[SugarTable("User")]
[SugarIndex($"index_{nameof(UserName)}", nameof(UserName), OrderByType.Asc)]
public class UserAggregateRoot : AggregateRoot<Guid>, ISoftDelete, IAuditedObject, IOrderNum, IState
{
    [SugarColumn(IsPrimaryKey = true)]
    public override Guid Id { get; protected set; }

    [SugarColumn(IsOwnsOne = true)]
    public EncryPasswordValueObject EncryPassword { get; set; } = new();
}
```

## 关联关系

关联关系使用 `[Navigate]` 特性：

### 多对多（通过中间表）

```csharp
[Navigate(typeof(UserRoleEntity), nameof(UserRoleEntity.UserId), nameof(UserRoleEntity.RoleId))]
public List<RoleAggregateRoot> Roles { get; set; }
```

### 一对一

```csharp
[Navigate(NavigateType.OneToOne, nameof(DeptId))]
public DeptAggregateRoot? Dept { get; set; }
```

## 值对象

值对象使用 `[SugarColumn(IsOwnsOne = true)]` 特性：

```csharp
[SugarColumn(IsOwnsOne = true)]
public EncryPasswordValueObject EncryPassword { get; set; } = new();
```

## 索引

使用 `[SugarIndex]` 特性定义索引：

```csharp
[SugarIndex($"index_{nameof(UserName)}", nameof(UserName), OrderByType.Asc)]
```

## 相关文档

- [枚举使用](/guide/backend/enum) - 了解枚举定义和使用
- [模块开发](/guide/backend/module) - 了解查询模式

