---
name: module-generator
description: 生成完整的 ABP 模块结构，以最小 token 开销创建 5 层项目、更新主模块依赖、解决方案文件和动态 API 配置。当用户想要创建新模块，提到"新建模块"、"创建模块"、"module"、"add module"、"generate module"时使用。支持简单名称（Payment）和复合名称（content-management、ContentManagement）。
---

# 模块生成器

通过委托 C# 脚本执行来生成完整的 ABP 业务模块结构。业务模块命名空间使用 `Yi.Module.{Module}`，框架基础设施引用保持 `Yi.Framework.*`。

## 工作流程

### 步骤 1：验证输入

检查模块名称格式：
- 支持 PascalCase（如 `Payment`、`ContentManagement`）
- 支持 kebab-case（如 `payment`、`content-management`）
- 支持 snake_case（自动转换为 PascalCase）

### 步骤 2：执行生成脚本

直接运行 C# 文件：

```bash
dotnet run --file .claude/skills/module-generator/scripts/generate_module.cs -- <模块名称>
```

脚本处理以下事项：
1. 名称转换（PascalCase ↔ kebab-case）
2. 创建 `module/{kebab-name}/Yi.Module.{PascalName}.*` 5 层项目结构
3. 生成模块类和 csproj 文件
4. 创建子目录（Entities、Dtos、IServices、Services、Repositories）
5. 更新主模块 DependsOn 声明
6. 向主 csproj 文件添加 ProjectReference，并引用 `framework/{group}/Yi.Framework.*` 基础设施项目
7. 更新解决方案文件（.slnx）
8. 在 YiAbpWebModule.cs 中配置动态 API

如果模块已存在，脚本将报告错误并停止。

### 步骤 3：报告结果

向用户展示：
- 生成的模块路径
- 创建的文件列表（摘要，非完整路径）
- 实体/DTO/服务创建的后续步骤

## 输出格式

成功生成后：

```
模块 '{PascalName}' 创建成功。

位置：module/{kebab-name}/Yi.Module.{PascalName}.*

已生成：
- 5 个项目层（Domain.Shared、Domain、Application.Contracts、Application、SqlSugarCore）
- 带有正确 DependsOn 的模块类
- 带有正确引用的项目文件
- 目录结构（Entities、Dtos、IServices、Services、Repositories）

已更新：
- 主模块类（5 个文件）
- 主项目文件（5 个文件）
- 解决方案文件（Yi.Abp.slnx）
- 动态 API 配置（YiAbpWebModule.cs）

后续建议步骤：
1. 使用 /crud-generator-plus 生成完整 CRUD 功能

是否需要执行 dotnet build 构建检查（一般情况下不需要）？
```

## 错误处理

| 场景 | 操作 |
|------|------|
| 模块已存在 | 脚本报告错误，告知用户删除或使用不同名称 |
| C# 脚本执行错误 | 显示错误消息，建议检查 .NET 安装 |

## 预运行模式

用于验证而不做实际更改：

```bash
dotnet run --file .claude/skills/module-generator/scripts/generate_module.cs -- <名称> --dry-run
```

适用于用户询问"会创建什么"或想要预览更改的情况。

## 示例

| 输入 | PascalCase | kebab-case | 模块路径 |
|------|------------|------------|----------|
| `Payment` | Payment | payment | `module/payment/Yi.Module.Payment.*` |
| `content-management` | ContentManagement | content-management | `module/content-management/Yi.Module.ContentManagement.*` |
| `ContentManagement` | ContentManagement | content-management | `module/content-management/Yi.Module.ContentManagement.*` |
