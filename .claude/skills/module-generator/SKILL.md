---
name: module-generator
description: Generate ABP framework module structure following the project's established patterns. Creates all necessary project files, module classes, directory structures, and updates main module references. Use when user needs to create a new module in the src/WebApi/module directory.
---

# Module Generator

This skill guides the creation of complete ABP framework module structures following the project's established architecture patterns. It generates all necessary project files, module classes, directory structures, and automatically updates main module references.

## Overview

When generating a new module (e.g., "Content", "PersonManagement", "Collection"), you need to:

1. **Convert module name** to appropriate formats (PascalCase for namespaces, kebab-case for directories)
2. **Create 5 project layers**: Domain.Shared, Domain, Application.Contracts, Application, SqlSugarCore
3. **Generate module classes** for each layer
4. **Create directory structures** (Entities, Dtos, IServices, Services, Repositories)
5. **Update main module files** to include the new module
6. **Update solution file** (.slnx) to include all projects

## Workflow

### Step 1: Name Conversion

Convert the module name to appropriate formats:

- **Input**: Can be PascalCase (e.g., "ContentManagement") or kebab-case (e.g., "content-management")
- **PascalCase**: Used for namespaces and class names (e.g., "ContentManagement")
  - If input has no hyphens/underscores and starts with uppercase, use as-is
  - Otherwise, split by `-` or `_`, capitalize first letter of each part, lowercase the rest
- **KebabCase**: Used for directory names (e.g., "content-management")
  - If input has hyphens, use as-is (lowercase)
  - Otherwise, convert PascalCase by inserting `-` before each uppercase letter, then lowercase

**Example:**
- Input: "Content" → PascalCase: "Content", KebabCase: "content"
- Input: "ContentManagement" → PascalCase: "ContentManagement", KebabCase: "content-management"
- Input: "content-management" → PascalCase: "ContentManagement", KebabCase: "content-management"

### Step 2: Create Module Directory Structure

Create the module root directory at: `src/WebApi/module/{kebab-module-name}/`

**Check if module already exists** - if it does, inform the user and stop.

### Step 3: Create Projects

Create 5 projects using `dotnet new classlib`:

1. **Yi.Framework.{PascalModuleName}.Domain.Shared** (TFM: net10.0)
2. **Yi.Framework.{PascalModuleName}.Domain** (TFM: net10.0)
3. **Yi.Framework.{PascalModuleName}.Application.Contracts** (TFM: net10.0)
4. **Yi.Framework.{PascalModuleName}.Application** (TFM: net10.0)
5. **Yi.Framework.{PascalModuleName}.SqlSugarCore** (TFM: net10.0)

For each project:
- Use `dotnet new classlib -n {ProjectName} -f {TFM} -o {ProjectPath} --force`
- Delete the default `Class1.cs` file if it exists

### Step 4: Generate Module Classes and Project Files

#### 4.1 Domain.Shared

**Module Class**: `Yi.Framework.{PascalModuleName}.Domain.Shared/YiFramework{PascalModuleName}DomainSharedModule.cs`

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

**Project File**: `Yi.Framework.{PascalModuleName}.Domain.Shared/Yi.Framework.{PascalModuleName}.Domain.Shared.csproj`

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

#### 4.2 Domain

**Module Class**: `Yi.Framework.{PascalModuleName}.Domain/YiFramework{PascalModuleName}DomainModule.cs`

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

**Project File**: `Yi.Framework.{PascalModuleName}.Domain/Yi.Framework.{PascalModuleName}.Domain.csproj`

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

**Create Directory**: `Yi.Framework.{PascalModuleName}.Domain/Entities/` (empty)

#### 4.3 Application.Contracts

**Module Class**: `Yi.Framework.{PascalModuleName}.Application.Contracts/YiFramework{PascalModuleName}ApplicationContractsModule.cs`

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

**Project File**: `Yi.Framework.{PascalModuleName}.Application.Contracts/Yi.Framework.{PascalModuleName}.Application.Contracts.csproj`

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

**Create Directories**:
- `Yi.Framework.{PascalModuleName}.Application.Contracts/Dtos/` (empty)
- `Yi.Framework.{PascalModuleName}.Application.Contracts/IServices/` (empty)

#### 4.4 Application

**Module Class**: `Yi.Framework.{PascalModuleName}.Application/YiFramework{PascalModuleName}ApplicationModule.cs`

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

**Project File**: `Yi.Framework.{PascalModuleName}.Application/Yi.Framework.{PascalModuleName}.Application.csproj`

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

**Create Directory**: `Yi.Framework.{PascalModuleName}.Application/Services/` (empty)

#### 4.5 SqlSugarCore

**Module Class**: `Yi.Framework.{PascalModuleName}.SqlSugarCore/YiFramework{PascalModuleName}SqlSugarCoreModule.cs`

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

**Project File**: `Yi.Framework.{PascalModuleName}.SqlSugarCore/Yi.Framework.{PascalModuleName}.SqlSugarCore.csproj`

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

**Create Directory**: `Yi.Framework.{PascalModuleName}.SqlSugarCore/Repositories/` (empty)

### Step 5: Update Main Module Files

Update the main module files in `src/WebApi/src/` to include references to the new module.

#### 5.1 Update Module Classes

For each layer, update the corresponding main module class:

**Domain.Shared**: `src/Yi.Abp.Domain.Shared/YiAbpDomainSharedModule.cs`
- Add `using Yi.Framework.{PascalModuleName}.Domain.Shared;`
- Add `typeof(YiFramework{PascalModuleName}DomainSharedModule)` to `[DependsOn(...)]`

**Domain**: `src/Yi.Abp.Domain/YiAbpDomainModule.cs`
- Add `using Yi.Framework.{PascalModuleName}.Domain;`
- Add `typeof(YiFramework{PascalModuleName}DomainModule)` to `[DependsOn(...)]`

**Application.Contracts**: `src/Yi.Abp.Application.Contracts/YiAbpApplicationContractsModule.cs`
- Add `using Yi.Framework.{PascalModuleName}.Application.Contracts;`
- Add `typeof(YiFramework{PascalModuleName}ApplicationContractsModule)` to `[DependsOn(...)]`

**Application**: `src/Yi.Abp.Application/YiAbpApplicationModule.cs`
- Add `using Yi.Framework.{PascalModuleName}.Application;`
- Add `typeof(YiFramework{PascalModuleName}ApplicationModule)` to `[DependsOn(...)]`

**SqlSugarCore**: `src/Yi.Abp.SqlSugarCore/YiAbpSqlSugarCoreModule.cs`
- Add `using Yi.Framework.{PascalModuleName}.SqlSugarCore;`
- Add `typeof(YiFramework{PascalModuleName}SqlSugarCoreModule)` to `[DependsOn(...)]`

**Important Notes:**
- Add `using` statements in alphabetical order
- Add `typeof(...)` entries in the `[DependsOn(...)]` attribute, maintaining proper formatting
- If it's the last entry, don't add a trailing comma
- If it's not the last entry, add a comma and newline

#### 5.2 Update Project References

For each layer, update the corresponding main project file to add a ProjectReference:

**Domain.Shared**: `src/Yi.Abp.Domain.Shared/Yi.Abp.Domain.Shared.csproj`
- Add: `<ProjectReference Include="..\..\module\{KebabModuleName}\Yi.Framework.{PascalModuleName}.Domain.Shared\Yi.Framework.{PascalModuleName}.Domain.Shared.csproj" />`

**Domain**: `src/Yi.Abp.Domain/Yi.Abp.Domain.csproj`
- Add: `<ProjectReference Include="..\..\module\{KebabModuleName}\Yi.Framework.{PascalModuleName}.Domain\Yi.Framework.{PascalModuleName}.Domain.csproj" />`

**Application.Contracts**: `src/Yi.Abp.Application.Contracts/Yi.Abp.Application.Contracts.csproj`
- Add: `<ProjectReference Include="..\..\module\{KebabModuleName}\Yi.Framework.{PascalModuleName}.Application.Contracts\Yi.Framework.{PascalModuleName}.Application.Contracts.csproj" />`

**Application**: `src/Yi.Abp.Application/Yi.Abp.Application.csproj`
- Add: `<ProjectReference Include="..\..\module\{KebabModuleName}\Yi.Framework.{PascalModuleName}.Application\Yi.Framework.{PascalModuleName}.Application.csproj" />`

**SqlSugarCore**: `src/Yi.Abp.SqlSugarCore/Yi.Abp.SqlSugarCore.csproj`
- Add: `<ProjectReference Include="..\..\module\{KebabModuleName}\Yi.Framework.{PascalModuleName}.SqlSugarCore\Yi.Framework.{PascalModuleName}.SqlSugarCore.csproj" />`

**Important Notes:**
- Add ProjectReference within an existing `<ItemGroup>` that contains other ProjectReference elements
- If no such ItemGroup exists, create a new one
- Maintain proper indentation (4 spaces)

### Step 6: Update Solution File

Update the solution file: `src/WebApi/Yi.Abp.slnx`

The solution file is an XML file. You need to:

1. **Check if module folder exists**: Look for a `<Folder Name="/module/{KebabModuleName}/">` element
2. **If it doesn't exist**, create a new Folder element with the name `/module/{KebabModuleName}/`
3. **Add all 5 projects** to this folder in the following order:
   - Application.Contracts
   - Application
   - Domain.Shared
   - Domain
   - SqlSugarCore
4. **Insert the folder** after the last existing module folder (one with name like `/module/*/`)
5. **If no module folders exist**, insert after the `/module/` parent folder (if it exists)

**Project XML structure:**
```xml
<Folder Name="/module/{KebabModuleName}/">
  <Project Path="module/{KebabModuleName}/Yi.Framework.{PascalModuleName}.Application.Contracts/Yi.Framework.{PascalModuleName}.Application.Contracts.csproj" />
  <Project Path="module/{KebabModuleName}/Yi.Framework.{PascalModuleName}.Application/Yi.Framework.{PascalModuleName}.Application.csproj" />
  <Project Path="module/{KebabModuleName}/Yi.Framework.{PascalModuleName}.Domain.Shared/Yi.Framework.{PascalModuleName}.Domain.Shared.csproj" />
  <Project Path="module/{KebabModuleName}/Yi.Framework.{PascalModuleName}.Domain/Yi.Framework.{PascalModuleName}.Domain.csproj" />
  <Project Path="module/{KebabModuleName}/Yi.Framework.{PascalModuleName}.SqlSugarCore/Yi.Framework.{PascalModuleName}.SqlSugarCore.csproj" />
</Folder>
```

## Naming Conventions

- **Module name input**: PascalCase or kebab-case
- **PascalCase**: Used for namespaces, class names, project names
- **KebabCase**: Used for directory names, solution folder paths
- **Module class prefix**: `YiFramework` (e.g., `YiFrameworkContentDomainModule`)
- **Namespace prefix**: `Yi.Framework.` (e.g., `Yi.Framework.Content.Domain`)

## File Encoding

All generated files should use **UTF-8 without BOM** encoding.

## Error Handling

- **Module already exists**: Check if `src/WebApi/module/{kebab-module-name}/` exists before creating
- **Main module files not found**: If main module files don't exist, inform the user but continue
- **Solution file update fails**: Inform the user to manually add projects to the solution

## Examples

### Example 1: Simple Module Name
**Input**: "Content"
- PascalCase: "Content"
- KebabCase: "content"
- Module path: `src/WebApi/module/content/`
- Projects: `Yi.Framework.Content.Domain.Shared`, `Yi.Framework.Content.Domain`, etc.

### Example 2: Compound Module Name
**Input**: "ContentManagement"
- PascalCase: "ContentManagement"
- KebabCase: "content-management"
- Module path: `src/WebApi/module/content-management/`
- Projects: `Yi.Framework.ContentManagement.Domain.Shared`, etc.

### Example 3: Kebab-Case Input
**Input**: "content-management"
- PascalCase: "ContentManagement"
- KebabCase: "content-management"
- Module path: `src/WebApi/module/content-management/`
- Projects: `Yi.Framework.ContentManagement.Domain.Shared`, etc.

## Reference Implementation

See existing modules for reference:
- `src/WebApi/module/content/` - Simple module example
- `src/WebApi/module/setting-management/` - Kebab-case module example
- `src/WebApi/module/rbac/` - Complex module example

## Next Steps After Module Generation

After generating the module structure, the user typically needs to:
1. Create entity classes in `Domain/Entities/`
2. Create DTO classes in `Application.Contracts/Dtos/`
3. Create service interfaces in `Application.Contracts/IServices/`
4. Create service implementations in `Application/Services/`
5. Create repository implementations in `SqlSugarCore/Repositories/` (if needed)

Use the `business-module-initializer` skill for generating business entities and their related files.
