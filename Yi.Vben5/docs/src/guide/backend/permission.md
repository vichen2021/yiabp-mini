# 权限与操作日志

## 权限码格式

权限码格式：`{模块}:{实体}:{操作}`，通过 `[Permission]` 特性控制：

```csharp
[Permission("system:user:list")]
public override async Task<PagedResultDto<UserGetListOutputDto>> GetListAsync(...)

[Permission("system:user:add")]
public async override Task<UserGetOutputDto> CreateAsync(...)
```

## 操作日志

操作日志通过 `[OperLog]` 特性记录：

```csharp
[OperLog("添加用户", OperEnum.Insert)]
public async override Task<UserGetOutputDto> CreateAsync(...)
```

## 批量删除

批量删除为标准模式，单条删除禁用远程访问：

```csharp
[RemoteService(isEnabled: true)]
public virtual async Task DeleteAsync(IEnumerable<TKey> ids) { ... }

[RemoteService(isEnabled: false)]
public override Task DeleteAsync(TKey id) { ... }
```

## 相关文档

- [应用服务](/guide/backend/service) - 了解应用服务开发
- [API 模式](/guide/backend/api) - 了解 API 自动映射

