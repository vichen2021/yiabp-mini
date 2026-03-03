# 模块开发

## 新建模块检查清单

新建业务模块时，确保：

1. 创建完整的 5 层项目结构（Domain.Shared / Domain / Application.Contracts / Application / SqlSugarCore）
2. 每层创建对应的 ABP Module 类并声明 `[DependsOn]`
3. 在 `YiAbpWebModule.cs` 中注册动态 API（`ConventionalControllers.Create`）
4. 在主模块中添加对新模块 Application 层的依赖
5. DTO 遵循 Input 用 `Vo` 后缀、Output 用 `Dto` 后缀的规范

## 使用模块生成器

推荐使用 Claude Skills 中的 **模块生成器** 自动生成模块结构：

查看 [模块生成器文档](/guide/skills/module-generator) 了解如何使用。

## 使用Yi.Abp.Tool

[意社区](https://ccnetcore.com/article/aaa00329-7f35-d3fe-d258-3a0f8380b742/4264aef4-979f-f533-dc79-3a100334d6a8)提供Tool工具自动生成模块结构，但是仍然需要逐个为新模块添加项目引用

## 模块依赖关系

每个层都是 ABP 模块，通过 `[DependsOn]` 声明依赖关系：

```csharp
[DependsOn(
    typeof(YiFrameworkRbacApplicationContractsModule),
    typeof(YiFrameworkRbacDomainModule),
    typeof(YiFrameworkDddApplicationModule))]
public class YiFrameworkRbacApplicationModule : AbpModule
```

## 相关文档

- [架构设计](/guide/backend/architecture) - 了解架构设计
- [命名规范](/guide/backend/naming) - 了解命名规范
- [模块生成器](/guide/skills/module-generator) - 使用 AI 工具生成模块
- [意社区](https://ccnetcore.com/) - .net 交流社区，包含Yi框架开发文档和模块生成工具

