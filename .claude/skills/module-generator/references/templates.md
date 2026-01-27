# Module Generation Templates

This document contains all code templates used for generating ABP framework modules.

## Module Name Conversion

### PascalCase Conversion
- Input: "Content" → Output: "Content"
- Input: "ContentManagement" → Output: "ContentManagement"
- Input: "content-management" → Output: "ContentManagement"
- Input: "content_management" → Output: "ContentManagement"

### KebabCase Conversion
- Input: "Content" → Output: "content"
- Input: "ContentManagement" → Output: "content-management"
- Input: "content-management" → Output: "content-management"

## Project Templates

### Domain.Shared Module Class

**File**: `Yi.Framework.{PascalModuleName}.Domain.Shared/YiFramework{PascalModuleName}DomainSharedModule.cs`

```csharp
using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Yi.Framework.{PascalModuleName}.Domain.Shared
{
    [DependsOn(typeof(AbpDddDomainSharedModule))]
    public class YiFramework{PascalModuleName}DomainSharedModule : AbpModule
    {

    }
}
```

### Domain.Shared Project File

**File**: `Yi.Framework.{PascalModuleName}.Domain.Shared/Yi.Framework.{PascalModuleName}.Domain.Shared.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<Import Project="..\..\..\common.props" />
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

	<ItemGroup>
		<PackageReference Include="Volo.Abp.Ddd.Domain.Shared" Version="$(AbpVersion)" />
	</ItemGroup>

</Project>
```

### Domain Module Class

**File**: `Yi.Framework.{PascalModuleName}.Domain/YiFramework{PascalModuleName}DomainModule.cs`

```csharp
using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Yi.Framework.{PascalModuleName}.Domain.Shared;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.{PascalModuleName}.Domain
{
    [DependsOn(typeof(YiFramework{PascalModuleName}DomainSharedModule),
        typeof(AbpDddDomainModule),
        typeof(YiFrameworkSqlSugarCoreAbstractionsModule))]
    public class YiFramework{PascalModuleName}DomainModule : AbpModule
    {

    }
}
```

### Domain Project File

**File**: `Yi.Framework.{PascalModuleName}.Domain/Yi.Framework.{PascalModuleName}.Domain.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<Import Project="..\..\..\common.props" />
	<PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

	<ItemGroup>
		<PackageReference Include="Volo.Abp.Ddd.Domain" Version="$(AbpVersion)" />
	</ItemGroup>

	<ItemGroup>
	  <ProjectReference Include="..\..\..\framework\Yi.Framework.SqlSugarCore.Abstractions\Yi.Framework.SqlSugarCore.Abstractions.csproj" />
	  <ProjectReference Include="..\Yi.Framework.{PascalModuleName}.Domain.Shared\Yi.Framework.{PascalModuleName}.Domain.Shared.csproj" />
	</ItemGroup>
</Project>
```

### Application.Contracts Module Class

**File**: `Yi.Framework.{PascalModuleName}.Application.Contracts/YiFramework{PascalModuleName}ApplicationContractsModule.cs`

```csharp
using Yi.Framework.{PascalModuleName}.Domain.Shared;
using Yi.Framework.Ddd.Application.Contracts;

namespace Yi.Framework.{PascalModuleName}.Application.Contracts
{
    [DependsOn(typeof(YiFramework{PascalModuleName}DomainSharedModule),
        typeof(YiFrameworkDddApplicationContractsModule))]
    public class YiFramework{PascalModuleName}ApplicationContractsModule : AbpModule
    {

    }
}
```

### Application.Contracts Project File

**File**: `Yi.Framework.{PascalModuleName}.Application.Contracts/Yi.Framework.{PascalModuleName}.Application.Contracts.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<Import Project="..\..\..\common.props" />
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\framework\Yi.Framework.Ddd.Application.Contracts\Yi.Framework.Ddd.Application.Contracts.csproj" />
    <ProjectReference Include="..\Yi.Framework.{PascalModuleName}.Domain.Shared\Yi.Framework.{PascalModuleName}.Domain.Shared.csproj" />
  </ItemGroup>

</Project>
```

### Application Module Class

**File**: `Yi.Framework.{PascalModuleName}.Application/YiFramework{PascalModuleName}ApplicationModule.cs`

```csharp
using Yi.Framework.{PascalModuleName}.Application.Contracts;
using Yi.Framework.{PascalModuleName}.Domain;
using Yi.Framework.Ddd.Application;

namespace Yi.Framework.{PascalModuleName}.Application
{
    [DependsOn(typeof(YiFramework{PascalModuleName}ApplicationContractsModule),
        typeof(YiFramework{PascalModuleName}DomainModule),
        typeof(YiFrameworkDddApplicationModule))]
    public class YiFramework{PascalModuleName}ApplicationModule : AbpModule
    {

    }
}
```

### Application Project File

**File**: `Yi.Framework.{PascalModuleName}.Application/Yi.Framework.{PascalModuleName}.Application.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<Import Project="..\..\..\common.props" />
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include="..\..\..\framework\Yi.Framework.Ddd.Application\Yi.Framework.Ddd.Application.csproj" />
    <ProjectReference Include="..\Yi.Framework.{PascalModuleName}.Application.Contracts\Yi.Framework.{PascalModuleName}.Application.Contracts.csproj" />
    <ProjectReference Include="..\Yi.Framework.{PascalModuleName}.Domain\Yi.Framework.{PascalModuleName}.Domain.csproj" />
  </ItemGroup>

</Project>
```

### SqlSugarCore Module Class

**File**: `Yi.Framework.{PascalModuleName}.SqlSugarCore/YiFramework{PascalModuleName}SqlSugarCoreModule.cs`

```csharp
using Yi.Framework.SqlSugarCore;

namespace Yi.Framework.{PascalModuleName}.SqlSugarCore
{
    [DependsOn(typeof(YiFrameworkSqlSugarCoreModule))]
    public class YiFramework{PascalModuleName}SqlSugarCoreModule:AbpModule
    {

    }
}
```

### SqlSugarCore Project File

**File**: `Yi.Framework.{PascalModuleName}.SqlSugarCore/Yi.Framework.{PascalModuleName}.SqlSugarCore.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
	<Import Project="..\..\..\common.props" />
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\framework\Yi.Framework.SqlSugarCore\Yi.Framework.SqlSugarCore.csproj" />
    <ProjectReference Include="..\Yi.Framework.{PascalModuleName}.Domain\Yi.Framework.{PascalModuleName}.Domain.csproj" />
  </ItemGroup>

</Project>
```

## Directory Structure

After generation, the module should have this structure:

```
module/{kebab-module-name}/
├── Yi.Framework.{PascalModuleName}.Domain.Shared/
│   ├── YiFramework{PascalModuleName}DomainSharedModule.cs
│   └── Yi.Framework.{PascalModuleName}.Domain.Shared.csproj
├── Yi.Framework.{PascalModuleName}.Domain/
│   ├── YiFramework{PascalModuleName}DomainModule.cs
│   ├── Yi.Framework.{PascalModuleName}.Domain.csproj
│   └── Entities/ (empty)
├── Yi.Framework.{PascalModuleName}.Application.Contracts/
│   ├── YiFramework{PascalModuleName}ApplicationContractsModule.cs
│   ├── Yi.Framework.{PascalModuleName}.Application.Contracts.csproj
│   ├── Dtos/ (empty)
│   └── IServices/ (empty)
├── Yi.Framework.{PascalModuleName}.Application/
│   ├── YiFramework{PascalModuleName}ApplicationModule.cs
│   ├── Yi.Framework.{PascalModuleName}.Application.csproj
│   └── Services/ (empty)
└── Yi.Framework.{PascalModuleName}.SqlSugarCore/
    ├── YiFramework{PascalModuleName}SqlSugarCoreModule.cs
    ├── Yi.Framework.{PascalModuleName}.SqlSugarCore.csproj
    └── Repositories/ (empty)
```

## Main Module Update Patterns

### Using Statement Pattern
Add using statements in alphabetical order:
```csharp
using Yi.Framework.{PascalModuleName}.Domain.Shared;
```

### DependsOn Pattern
Add to the `[DependsOn(...)]` attribute:
```csharp
[DependsOn(
    typeof(ExistingModule),
    typeof(YiFramework{PascalModuleName}DomainSharedModule)  // Add this
)]
```

### ProjectReference Pattern
Add to the `.csproj` file within an `<ItemGroup>`:
```xml
<ItemGroup>
  <ProjectReference Include="..\..\module\{KebabModuleName}\Yi.Framework.{PascalModuleName}.Domain.Shared\Yi.Framework.{PascalModuleName}.Domain.Shared.csproj" />
</ItemGroup>
```

## Solution File Update Pattern

Add a new folder element to the solution XML:

```xml
<Folder Name="/module/{KebabModuleName}/">
  <Project Path="module/{KebabModuleName}/Yi.Framework.{PascalModuleName}.Application.Contracts/Yi.Framework.{PascalModuleName}.Application.Contracts.csproj" />
  <Project Path="module/{KebabModuleName}/Yi.Framework.{PascalModuleName}.Application/Yi.Framework.{PascalModuleName}.Application.csproj" />
  <Project Path="module/{KebabModuleName}/Yi.Framework.{PascalModuleName}.Domain.Shared/Yi.Framework.{PascalModuleName}.Domain.Shared.csproj" />
  <Project Path="module/{KebabModuleName}/Yi.Framework.{PascalModuleName}.Domain/Yi.Framework.{PascalModuleName}.Domain.csproj" />
  <Project Path="module/{KebabModuleName}/Yi.Framework.{PascalModuleName}.SqlSugarCore/Yi.Framework.{PascalModuleName}.SqlSugarCore.csproj" />
</Folder>
```

## Replacement Rules

When generating files, replace:
- `{PascalModuleName}` with the PascalCase module name (e.g., "Content", "ContentManagement")
- `{KebabModuleName}` with the kebab-case module name (e.g., "content", "content-management")
- `{PascalModuleName}` in class names (e.g., `YiFramework{PascalModuleName}DomainModule`)

## Example: Content Module

If module name is "Content":
- PascalCase: "Content"
- KebabCase: "content"
- Module class: `YiFrameworkContentDomainModule`
- Namespace: `Yi.Framework.Content.Domain`
- Project: `Yi.Framework.Content.Domain.Shared`
- Directory: `module/content/`
