# 启动项目

## 环境要求

后端需要 **.NET 10** 环境启动

## 开发工具

推荐下载以下开发工具：

- **Visual Studio 2026**
- **Rider**

### Visual Studio 启动方式

选中启动项目 `Yi.Abp.Web` 项目

![Visual Studio 启动](/guide/vs-startup.png)

### Rider 启动方式

右上角切换为 `Yi.Abp.Web`，点击运行或调试

![Rider 启动](/guide/rider-startup.png)

## 数据库配置

数据库默认采用 **SQLite**，路径是 `Yi.Abp/src/Yi.Abp.Web/yi-abp-dev.db`

若需要修改数据库类型，请在 `Yi.Abp/src/Yi.Abp.Web/appsettings.json` 中修改

## Swagger

启动后，浏览器将会弹出项目接口地址 Swagger：

![Swagger 界面](/guide/swagger.png)

项目采用模块化方式，已自动分组至右上角，可点击切换分组，框架通过`Yi.Abp\src\Yi.Abp.Web\YiAbpWebModule.cs` PreConfigureServices配置进行分组

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

## 默认账号

默认种子数据超级管理员账号：

- 用户名：`cc`
- 密码：`123456`
