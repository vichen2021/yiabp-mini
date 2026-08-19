# 后端依赖升级设计

## 目标

将 .NET 10 后端所有直接依赖尽量升级到 NuGet 最新稳定版，并通过 ABP 10.6.0 官方依赖链兼容 Swashbuckle 10 与 Microsoft.OpenApi 2.x。

## 方案

- 全局 ABP 使用 `10.6.0`，由 `Volo.Abp.Swashbuckle 10.6.0` 官方引入 `Swashbuckle.AspNetCore 10.2.3`。
- 全局 SqlSugar 使用 `5.1.4.217`，其余直接依赖按 NuGet 过期包扫描结果升级到最新稳定版。
- Web 项目使用 `Microsoft.AspNetCore.OpenApi 10.0.11`，与 `net10.0` 对齐。
- Web 项目不显式覆盖 Swashbuckle 版本，避免再次发生 ABP Filter 接口的运行时 ABI 不兼容。
- `SkiaSharp.NativeAssets.Linux.NoDependencies` 保持 `2.88.9`，与 `Lazy.Captcha.Core 2.2.4` 解析的托管 `SkiaSharp 2.88.9` 对齐，不使用 ABI 不兼容的 4.151.1。
- 在 SqlSugar 抽象项目和独立 MCP 项目引用 `SQLitePCLRaw.bundle_e_sqlite3 2.1.13`，覆盖 SqlSugar 传递引入的存在 High 级漏洞的 2.1.11，同时保持 2.x ABI。
- OpenAPI 类型统一使用 `Microsoft.OpenApi` 根命名空间。
- 枚举 Schema Filter 接收 `IOpenApiSchema`，只在具体实现为 `OpenApiSchema` 时修改，并用 `JsonNode` 表达枚举值。
- 安全需求使用 Swashbuckle 10 的 document factory 和 `OpenApiSecuritySchemeReference`。

## 验证

1. 使用 `dotnet restore --force --no-cache` 重新生成依赖资产，避免旧缓存掩盖依赖问题。
2. 使用 `dotnet list Yi.Abp.slnx package --outdated` 确认除已记录的 SkiaSharp 兼容例外外没有过期直接依赖。
3. 构建完整解决方案并实际启动 `Yi.Abp.Web.csproj`，确认越过 ABP 模块初始化。
4. 请求 Swagger JSON，验证 ABP Filter 和自定义 Filter 的运行时执行。
5. 运行带后端构建的代码规范检查；既有、无关的静态规范问题只记录不修复。
6. 扫描完整解决方案的直接和传递依赖漏洞，要求所有项目无已知漏洞。

## 边界

- 不迁移到 `AddOpenApi`/`MapOpenApi`，继续使用现有 ABP/Swashbuckle 文档管线。
- 不修改业务 API、Swagger 路由或文档分组行为。
- 不直接引用或覆盖 Swashbuckle 版本。
- 不为了追求单个包的最高版本而混用不匹配的托管与原生 ABI。
