# API 模式

## 动态 API（Conventional Controllers）

项目使用 ABP **动态 API（Conventional Controllers）**，应用服务自动映射为 REST 接口，无需手动编写 Controller。

## 配置

在 `YiAbpWebModule.cs` 中配置：

```csharp
PreConfigure<AbpAspNetCoreMvcOptions>(options =>
{
    options.ConventionalControllers.Create(
        typeof(YiFramework{Module}ApplicationModule).Assembly,
        options =>
        {
            options.RemoteServiceName = "{module}";
            options.RootPath = "api/{module}";
        });
});
```

## 自动映射规则

- `UserService.GetListAsync()` → `GET /api/user`
- `UserService.CreateAsync()` → `POST /api/user`
- `UserService.UpdateAsync()` → `PUT /api/user/{id}`
- `UserService.DeleteAsync()` → `DELETE /api/user/{id}`

## 相关文档

- [应用服务](/guide/backend/service) - 了解应用服务开发
- [权限与日志](/guide/backend/permission) - 了解权限控制

