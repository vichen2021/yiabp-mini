# Backend Dependency Upgrade Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Upgrade every direct backend dependency to the latest compatible stable release and prove the application still builds and starts correctly.

**Architecture:** Upgrade the centrally managed ABP and SqlSugar versions together with all project-level direct packages. Let ABP select the Web project's Swashbuckle version, adapt the custom code to Microsoft.OpenApi 2.x, and validate the complete solution plus runtime Swagger generation. Keep the Linux SkiaSharp native asset aligned with the managed SkiaSharp version supplied by Lazy.Captcha rather than forcing an incompatible major version.

**Tech Stack:** .NET 10, ABP 10.6.0, Microsoft.AspNetCore.OpenApi 10.0.11, Swashbuckle.AspNetCore 10.2.3, Microsoft.OpenApi 2.7.5, SqlSugar 5.1.4.217.

---

### Task 1: Align package versions

**Files:**
- Modify: `Yi.Abp/version.props`
- Modify: `Yi.Abp/src/Yi.Abp.Web/Yi.Abp.Web.csproj`

- [x] **Step 1: Upgrade the ASP.NET Core integration package**

Set the central versions and Web OpenAPI reference to:

```xml
<AbpVersion>10.6.0</AbpVersion>
<SqlSugarVersion>5.1.4.217</SqlSugarVersion>
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.11" />
```

- [x] **Step 2: Use the ABP-provided Swagger version**

Do not add a direct Web `Swashbuckle.AspNetCore` override; verify that ABP 10.6.0 supplies version 10.2.3.

- [x] **Step 3: Upgrade all remaining direct packages**

Upgrade Hangfire, NewtonsoftJson, container tooling, MiniExcel, Aliyun SDK, Mapster, MySqlBackup, AutoMapper, Lazy.Captcha, TencentCloudSDK, Microsoft.NET.Test.Sdk, and NSubstitute to the latest stable versions reported by NuGet.

- [x] **Step 4: Preserve the SkiaSharp native compatibility boundary**

Use `SkiaSharp.NativeAssets.Linux.NoDependencies 2.88.9` because `Lazy.Captcha.Core 2.2.4` resolves managed `SkiaSharp 2.88.9`; do not mix it with the incompatible 4.151.1 Linux native ABI.

- [x] **Step 5: Override the vulnerable SQLite native bundle**

Reference `SQLitePCLRaw.bundle_e_sqlite3 2.1.13` from the shared SqlSugar abstraction and the standalone MCP project so all consumers avoid vulnerable transitive version 2.1.11 while remaining on the compatible 2.x ABI.

### Task 2: Adapt custom Swagger code

**Files:**
- Modify: `Yi.Abp/framework/aspnetcore/Yi.Framework.AspNetCore/Microsoft/Extensions/DependencyInjection/SwaggerAddExtensions.cs`
- Modify: `Yi.Abp/framework/aspnetcore/Yi.Framework.AspNetCore/YiFrameworkAspNetCoreModule.cs`
- Modify: `Yi.Abp/src/Yi.Abp.Web/YiAbpWebModule.cs`

- [x] **Step 1: Move OpenAPI types to the root namespace**

Replace `Microsoft.OpenApi.Any` and `Microsoft.OpenApi.Models` imports with `Microsoft.OpenApi`; add `System.Text.Json.Nodes` for enum values.

- [x] **Step 2: Update security requirements**

Construct the requirement through the Swashbuckle 10 document callback:

```csharp
options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
{
    [new OpenApiSecuritySchemeReference("JwtBearer", document)] = []
});
```

- [x] **Step 3: Update enum schema filtering**

Accept `IOpenApiSchema`, guard for `OpenApiSchema`, set `JsonSchemaType.String`, and populate `Enum` with `JsonValue.Create(enumName)`.

### Task 3: Verify dependency resolution and build

**Files:**
- Verify: `Yi.Abp/src/Yi.Abp.Web/obj/project.assets.json`

- [x] **Step 1: Force regeneration of restore assets**

Run `dotnet restore --force --no-cache` for the Web project so the complete transitive graph and `project.assets.json` are regenerated.

- [x] **Step 2: Restore and inspect packages**

Run:

```powershell
dotnet list Yi.Abp/src/Yi.Abp.Web/Yi.Abp.Web.csproj package --include-transitive
```

Expected: ABP 10.6.0, ASP.NET Core OpenAPI 10.0.11, Swashbuckle 10.2.3, and Microsoft.OpenApi 2.7.5 resolve without downgrade warnings.

- [x] **Step 3: Build the Web project**

Run:

```powershell
dotnet build Yi.Abp/src/Yi.Abp.Web/Yi.Abp.Web.csproj
```

Expected: exit code 0 with no OpenAPI compile errors.

- [x] **Step 4: Start the Web host and request Swagger documents**

Start the built executable from `Yi.Abp/src/Yi.Abp.Web`, then request every configured Swagger group on an isolated port.

Expected: the host passes ABP module initialization; all Swagger JSON endpoints return HTTP 200; JWT, enum descriptions, and `__tenant` parameters are present.

### Task 4: Run repository checks

**Files:**
- Verify: complete master worktree

- [x] **Step 1: Run the code standard review with build**

Run the repository `code-standard-review.ps1` with `-RunBuild`.

- [x] **Step 2: Review the final diff**

Confirm only dependency version files, OpenAPI compatibility code, and planning documents changed.

- [x] **Step 3: Scan transitive dependency vulnerabilities**

Run `dotnet list Yi.Abp.slnx package --vulnerable --include-transitive --no-restore` and require no vulnerable packages in all projects.
