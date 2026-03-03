# 模块生成器

## 功能说明

自动生成完整的 ABP 框架模块结构，遵循项目既定的架构模式。生成所有必要的项目文件、模块类、目录结构，并自动更新主模块引用和动态 API 配置。

## 使用场景

当需要在 `module/` 目录下创建新的业务模块时使用。例如：
- 创建新的业务模块（如 Content、PersonManagement、Collection）
- 需要完整的 5 层项目结构
- 需要自动配置模块依赖和动态 API

## 提示词示例

- `创建一个名为 ContentManagement 的新模块`
- `使用 module-generator 创建 OrderManagement 模块`
- `帮我生成 Video 模块的完整结构`

## 工作流程

### 1. 名称转换

模块名称支持两种格式：
- **PascalCase**：用于命名空间和类名（如 "ContentManagement"）
- **KebabCase**：用于目录名（如 "content-management"）

**示例：**
- 输入 "Content" → PascalCase: "Content", KebabCase: "content"
- 输入 "ContentManagement" → PascalCase: "ContentManagement", KebabCase: "content-management"
- 输入 "content-management" → PascalCase: "ContentManagement", KebabCase: "content-management"

### 2. 创建模块目录结构

在 `module/{kebab-module-name}/` 创建模块根目录。

**重要**：如果模块已存在，会提示用户并停止。

### 3. 创建 5 层项目

使用 `dotnet new classlib` 创建以下 5 个项目：

1. **Yi.Framework.{PascalModuleName}.Domain.Shared** - 共享层（枚举、常量、事件）
2. **Yi.Framework.{PascalModuleName}.Domain** - 领域层（实体、聚合根、领域服务、仓储接口）
3. **Yi.Framework.{PascalModuleName}.Application.Contracts** - 应用契约层（DTO、服务接口）
4. **Yi.Framework.{PascalModuleName}.Application** - 应用层（服务实现）
5. **Yi.Framework.{PascalModuleName}.SqlSugarCore** - 基础设施层（ORM 配置、仓储实现、数据种子）

### 4. 生成模块类和项目文件

为每层生成：
- **模块类**：继承 `AbpModule`，配置 `[DependsOn]` 依赖
- **项目文件**：配置项目引用和 NuGet 包
- **目录结构**：创建必要的空目录（Entities、Dtos、IServices、Services、Repositories）

### 5. 更新主模块文件

自动更新主模块文件以包含新模块：

- **模块类**：在 `YiAbpDomainSharedModule`、`YiAbpDomainModule` 等中添加依赖
- **项目引用**：在对应的 `.csproj` 文件中添加项目引用
- **命名空间**：按字母顺序添加 `using` 语句

### 6. 更新解决方案文件

更新 `Yi.Abp.slnx` 文件：
- 检查或创建模块文件夹
- 按顺序添加 5 个项目到解决方案

### 7. 配置动态 API

在 `YiAbpWebModule.cs` 中添加动态 API 配置：

```csharp
options.ConventionalControllers.Create(typeof(YiFramework{PascalModuleName}ApplicationModule).Assembly,
    options => options.RemoteServiceName = "{kebab-module-name}");
```

## 命名规范

- **模块名称输入**：PascalCase 或 kebab-case
- **PascalCase**：用于命名空间、类名、项目名
- **KebabCase**：用于目录名、解决方案文件夹路径
- **模块类前缀**：`YiFramework`（如 `YiFrameworkContentDomainModule`）
- **命名空间前缀**：`Yi.Framework.`（如 `Yi.Framework.Content.Domain`）

## 文件编码

所有生成的文件使用 **UTF-8 without BOM** 编码。

## 错误处理

- **模块已存在**：创建前检查 `module/{kebab-module-name}/` 是否存在
- **主模块文件未找到**：提示用户但继续执行
- **解决方案文件更新失败**：提示用户手动添加项目
- **动态 API 配置失败**：提示用户手动添加配置

## 示例

### 示例 1：简单模块名

**输入**："Content"
- PascalCase: "Content"
- KebabCase: "content"
- 模块路径：`module/content/`
- 项目：`Yi.Framework.Content.Domain.Shared` 等
- 动态 API RemoteServiceName: "content"

### 示例 2：复合模块名

**输入**："ContentManagement"
- PascalCase: "ContentManagement"
- KebabCase: "content-management"
- 模块路径：`module/content-management/`
- 项目：`Yi.Framework.ContentManagement.Domain.Shared` 等
- 动态 API RemoteServiceName: "content-management"

## 参考实现

参考现有模块：
- `module/setting-management/` - Kebab-case 模块示例
- `module/rbac/` - 复杂模块示例，已配置动态 API

## 后续步骤

生成模块结构后，通常需要：
1. 在 `Domain/Entities/` 中创建实体类
2. 在 `Application.Contracts/Dtos/` 中创建 DTO 类
3. 在 `Application.Contracts/IServices/` 中创建服务接口
4. 在 `Application/Services/` 中创建服务实现
5. 在 `SqlSugarCore/Repositories/` 中创建仓储实现（如需要）

**提示**：使用 [CRUD 生成器](/guide/skills/crud-generator) 可以自动生成业务实体及其相关文件。

## 相关文档

- [CRUD 生成器](/guide/skills/crud-generator) - 了解 CRUD 生成器
- [模块开发](/guide/backend/module) - 了解模块开发规范
