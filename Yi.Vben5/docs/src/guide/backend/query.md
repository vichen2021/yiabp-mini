# 查询模式

## WhereIF 条件过滤

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

## 关联查询

使用 `LeftJoin` 进行关联查询：

```csharp
.LeftJoin<DeptAggregateRoot>((user, dept) => user.DeptId == dept.Id)
```

## 排序

使用 `OrderByDescending` 或 `OrderBy` 进行排序：

```csharp
.OrderByDescending(user => user.CreationTime)
```

## 分页

使用 `ToPageListAsync` 进行分页查询：

```csharp
.ToPageListAsync(input.SkipCount, input.MaxResultCount, total);
```

## 相关文档

- [应用服务](/guide/backend/service) - 了解应用服务开发
- [实体定义](/guide/backend/entity) - 了解实体定义

