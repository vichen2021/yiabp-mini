#!/usr/bin/env dotnet
#:property PublishAot=false
#:property PackAsTool=false
/*
 * CRUD 代码生成器 - 解析实体类 .cs 文件并生成前后端代码。
 *
 * 用法:
 *     dotnet run --file generate_crud.cs -- --entity <路径> --module <模块名> [--enum <枚举路径>]
 *
 * 示例:
 *     dotnet run --file generate_crud.cs -- --entity "Yi.Abp/module/order/Yi.Module.Order.Domain/Entities/OrderInfoAggregateRoot.cs" --module order --enum "Yi.Abp/module/order/Yi.Module.Order.Domain.Shared/Enums/OrderTypeEnum.cs"
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

var parsedArgs = ParseArgs(Environment.GetCommandLineArgs().Skip(1).ToList());

if (string.IsNullOrWhiteSpace(parsedArgs.EntityPath) || string.IsNullOrWhiteSpace(parsedArgs.Module))
{
    PrintUsage();
    Environment.Exit(1);
}

var basePath = ResolveBasePath(parsedArgs.BasePath);
if (string.IsNullOrWhiteSpace(basePath))
{
    Console.WriteLine("错误: 无法找到项目根目录。");
    Environment.Exit(1);
}

Console.WriteLine("项目根目录: " + basePath);
Console.WriteLine("实体路径: " + parsedArgs.EntityPath);
Console.WriteLine("模块: " + parsedArgs.Module);
Console.WriteLine();

// 解析实体文件
var entityInfo = ParseEntityFile(parsedArgs.EntityPath, parsedArgs.Module);
Console.WriteLine("实体: " + entityInfo.EntityName);
Console.WriteLine("是否树形: " + entityInfo.IsTree);
Console.WriteLine("字段数: " + entityInfo.Fields.Count);
Console.WriteLine();

// 解析枚举文件
var enumInfos = new List<EnumInfo>();
if (!string.IsNullOrWhiteSpace(parsedArgs.EnumPath))
{
    var enumPaths = parsedArgs.EnumPath.Split(';');
    foreach (var enumPath in enumPaths)
    {
        if (File.Exists(enumPath))
        {
            enumInfos.Add(ParseEnumFile(enumPath));
        }
    }
}
Console.WriteLine("枚举数: " + enumInfos.Count);
Console.WriteLine();

// 生成后端文件
Console.WriteLine("=== 正在生成后端文件 ===");
GenerateBackendFiles(basePath, entityInfo, enumInfos);
Console.WriteLine();

// 生成前端文件
Console.WriteLine("=== 正在生成前端文件 ===");
GenerateFrontendFiles(basePath, entityInfo, enumInfos);
Console.WriteLine();

Console.WriteLine("=== 生成完成 ===");
Console.WriteLine("后续步骤:");
Console.WriteLine("  1. 运行并行 Agent 生成菜单/字典种子数据");
Console.WriteLine("  2. 运行增量构建验证");

// ==================================== 解析函数 ====================================

EntityInfo ParseEntityFile(string entityPath, string module)
{
    var content = File.ReadAllText(entityPath);
    var info = new EntityInfo();

    info.Module = module;
    info.ModuleNamespace = ToPascal(module);

    // 从类声明中提取实体名称
    var classMatch = Regex.Match(content, @"class\s+(\w+)AggregateRoot");
    if (classMatch.Success)
    {
        info.EntityName = classMatch.Groups[1].Value;
        info.EntityNameLower = ToLower(info.EntityName);
        info.EntityNameKebab = ToKebabCase(info.EntityName);
    }

    // 从 summary 注释中提取实体注释（支持多行格式）
    var commentMatch = Regex.Match(content, @"///\s*<summary>\s*\n\s*///\s*(.+?)\s*\n\s*///\s*</summary>", RegexOptions.Multiline);
    if (commentMatch.Success)
    {
        info.EntityComment = commentMatch.Groups[1].Value.Trim();
    }

    // 检查是否为树形实体（有 ParentId 字段）
    info.IsTree = content.Contains("ParentId") && content.Contains("Guid");

    // 提取字段列表（精确匹配属性注释块，支持 SugarColumn 特性和初始化表达式）
    // 使用更宽松的正则，允许多个空行和复杂特性参数
    var fieldMatches = Regex.Matches(content, @"///\s*<summary>\s*\n\s*///\s*(.+?)\s*\n\s*///\s*</summary>\s*\n\s*(?:\[SugarColumn\([^)]*\)\]\s*)?\s*public\s+(\w+(?:<[^>]+>)?)\s*(\?)?\s+(\w+)\s*{\s*get;\s*set;\s*}\s*(?:=\s*[^;]+;)?", RegexOptions.Multiline);

    foreach (Match match in fieldMatches)
    {
        var comment = match.Groups[1].Value.Trim();
        var type = match.Groups[2].Value;
        var nullable = match.Groups[3].Value == "?";
        var name = match.Groups[4].Value;

        // 跳过标准字段
        if (IsStandardField(name)) continue;

        var field = new FieldInfo
        {
            Name = name,
            Type = CleanType(type),
            Comment = comment,
            Nullable = nullable,
            IsTreeField = name == "ParentId",
            IsIndex = Regex.IsMatch(content, $"index_{name}") || name.EndsWith("Name"),
            IsSearchField = IsSearchableField(name, type),
            IsEnum = type.EndsWith("Enum"),
            EnumType = type.EndsWith("Enum") ? type : null
        };

        info.Fields.Add(field);

        if (field.IsEnum && !string.IsNullOrWhiteSpace(field.EnumType))
        {
            info.EnumTypes.Add(field.EnumType);
        }
    }

    return info;
}

EnumInfo ParseEnumFile(string enumPath)
{
    var content = File.ReadAllText(enumPath);
    var info = new EnumInfo();

    // 提取枚举名称
    var enumMatch = Regex.Match(content, @"enum\s+(\w+)");
    if (enumMatch.Success)
    {
        info.EnumName = enumMatch.Groups[1].Value;
        info.EnumNameLower = info.EnumName.Replace("Enum", "").ToLowerInvariant();
    }

    // 提取枚举值列表
    var valueMatches = Regex.Matches(content, @"\[Description\(""([^""]+)""\)\]\s+(\w+)\s*=\s+(\d+)");

    foreach (Match match in valueMatches)
    {
        info.Values.Add(new EnumValueInfo
        {
            Label = match.Groups[1].Value,
            Name = match.Groups[2].Value,
            Value = int.Parse(match.Groups[3].Value)
        });
    }

    return info;
}

bool IsStandardField(string name)
{
    var standardFields = new[] { "Id", "State", "OrderNum", "Remark", "IsDeleted", "CreationTime",
        "CreatorId", "LastModifierId", "LastModificationTime", "ConcurrencyStamp", "ExtraProperties", "Children" };
    return standardFields.Contains(name);
}

bool IsSearchableField(string name, string type)
{
    // 仅字符串类型可进行 Contains 搜索
    if (type != "string") return false;

    // 搜索字段关键字（按优先级排序）
    var searchKeywords = new[] { "Name", "Title", "Code", "Key", "No", "Number", "Phone", "Email", "Address", "Description" };
    return searchKeywords.Any(k => name.EndsWith(k) || name.Contains(k));
}

List<FieldInfo> SelectListColumns(EntityInfo entity)
{
    const int maxBusinessColumns = 6;
    var fields = entity.Fields
        .Where(f => !f.IsTreeField && ShouldConsiderListColumn(f))
        .Select((field, index) => new
        {
            Field = field,
            Index = index,
            Score = GetListColumnScore(entity, field)
        })
        .Where(x => x.Score > 0)
        .OrderByDescending(x => x.Score)
        .ThenBy(x => x.Index)
        .Take(maxBusinessColumns)
        .OrderBy(x => x.Index)
        .Select(x => x.Field)
        .ToList();

    if (fields.Count == 0)
    {
        var fallback = entity.Fields.FirstOrDefault(f => !f.IsTreeField);
        if (fallback != null)
        {
            fields.Add(fallback);
        }
    }

    return fields;
}

bool ShouldConsiderListColumn(FieldInfo field)
{
    var name = field.Name;
    if (IsStandardField(name)) return false;
    if (name.EndsWith("Id", StringComparison.Ordinal) && field.Type == "Guid") return false;
    if (IsLongTextField(name)) return false;
    return true;
}

int GetListColumnScore(EntityInfo entity, FieldInfo field)
{
    var name = field.Name;
    var entityName = entity.EntityName;

    if (name == "Name" || name == $"{entityName}Name" || name.EndsWith("Name")) return 100;
    if (name == "Title" || name.EndsWith("Title")) return 95;
    if (name.EndsWith("Code") || name.EndsWith("No") || name.EndsWith("Number")) return 90;
    if (field.IsEnum) return 80;
    if (IsImportantBooleanField(name, field.Type)) return 70;
    if (IsNumericField(field.Type) && IsBusinessNumericField(name)) return 65;
    if (name.EndsWith("Phone") || name.EndsWith("Mobile") || name.EndsWith("Email")) return 60;
    if (field.Type == "string") return 50;
    if (field.Type == "DateTime" && IsBusinessDateField(name)) return 45;
    if (IsNumericField(field.Type)) return 40;
    if (field.Type == "bool") return 30;
    return 0;
}

bool IsLongTextField(string name)
{
    var keywords = new[] { "Content", "Description", "Detail", "Json", "Html", "Text", "Body" };
    return keywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
}

bool IsImportantBooleanField(string name, string type)
{
    if (type != "bool") return false;
    var keywords = new[] { "IsDefault", "IsRecommend", "IsRecommended", "IsTop", "IsHot", "IsPublic" };
    return keywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
}

bool IsNumericField(string type)
{
    return type is "int" or "long" or "decimal" or "double" or "float";
}

bool IsBusinessNumericField(string name)
{
    var keywords = new[] { "Price", "Amount", "Count", "Stock", "Total", "Score", "Point", "Weight", "Rate", "Level" };
    return keywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
}

bool IsBusinessDateField(string name)
{
    if (IsStandardField(name)) return false;
    var keywords = new[] { "Start", "End", "Begin", "Expire", "Publish", "Release", "Open", "Close" };
    return keywords.Any(k => name.Contains(k, StringComparison.OrdinalIgnoreCase));
}

string GetColumnWidth(FieldInfo field)
{
    if (field.IsEnum) return "120";
    if (field.Type == "bool") return "100";
    if (field.Type == "DateTime") return "160";
    if (IsNumericField(field.Type)) return "100";
    if (field.Name.EndsWith("Code") || field.Name.EndsWith("No") || field.Name.EndsWith("Number")) return "140";
    if (field.Name.EndsWith("Name") || field.Name.EndsWith("Title")) return "160";
    return "120";
}

string CleanType(string type)
{
    // 移除泛型参数，简化类型映射
    return type.Replace("<Guid>", "").Replace("<string>", "");
}

// ==================================== 生成函数 ====================================

void GenerateBackendFiles(string basePath, EntityInfo entity, List<EnumInfo> enums)
{
    var modulePath = Path.Combine(basePath, "Yi.Abp", "module", entity.Module);
    var contractsPath = Path.Combine(modulePath, $"Yi.Module.{entity.ModuleNamespace}.Application.Contracts");
    var applicationPath = Path.Combine(modulePath, $"Yi.Module.{entity.ModuleNamespace}.Application");

    // 创建 DTO 目录
    var dtosPath = Path.Combine(contractsPath, "Dtos", entity.EntityName);
    Directory.CreateDirectory(dtosPath);

    // 生成 DTOs
    WriteFile(Path.Combine(dtosPath, $"{entity.EntityName}GetOutputDto.cs"), GenerateGetOutputDto(entity, enums));
    WriteFile(Path.Combine(dtosPath, $"{entity.EntityName}GetListOutputDto.cs"), GenerateGetListOutputDto(entity, enums));
    WriteFile(Path.Combine(dtosPath, $"{entity.EntityName}GetListInputVo.cs"), GenerateGetListInputVo(entity));
    WriteFile(Path.Combine(dtosPath, $"{entity.EntityName}CreateInputVo.cs"), GenerateCreateInputVo(entity, enums));
    WriteFile(Path.Combine(dtosPath, $"{entity.EntityName}UpdateInputVo.cs"), GenerateUpdateInputVo(entity, enums));

    // 创建 IServices 目录
    var iservicesPath = Path.Combine(contractsPath, "IServices");
    Directory.CreateDirectory(iservicesPath);
    WriteFile(Path.Combine(iservicesPath, $"I{entity.EntityName}Service.cs"), GenerateIService(entity));

    // 创建 Services 目录
    var servicesPath = Path.Combine(applicationPath, "Services");
    Directory.CreateDirectory(servicesPath);
    WriteFile(Path.Combine(servicesPath, $"{entity.EntityName}Service.cs"), GenerateService(entity));

    Console.WriteLine($"  已生成 {6 + (entity.IsTree ? 3 : 0)} 个后端文件");
}

void GenerateFrontendFiles(string basePath, EntityInfo entity, List<EnumInfo> enums)
{
    var frontendPath = Path.Combine(basePath, "Yi.Vben5", "apps", "web-antd", "src");

    // 创建 API 目录（短横线命名，不包含模块名）
    var apiPath = Path.Combine(frontendPath, "api", entity.EntityNameKebab);
    Directory.CreateDirectory(apiPath);

    WriteFile(Path.Combine(apiPath, "model.d.ts"), GenerateModelTs(entity, enums));
    WriteFile(Path.Combine(apiPath, "index.ts"), GenerateIndexTs(entity));

    // 创建 Views 目录（模块名/实体名短横线）
    var viewsPath = Path.Combine(frontendPath, "views", entity.Module, entity.EntityNameKebab);
    Directory.CreateDirectory(viewsPath);

    WriteFile(Path.Combine(viewsPath, "data.ts"), GenerateDataTs(entity, enums));
    WriteFile(Path.Combine(viewsPath, "index.vue"), GenerateIndexVue(entity));
    WriteFile(Path.Combine(viewsPath, $"{entity.EntityNameKebab}-drawer.vue"), GenerateDrawerVue(entity));

    Console.WriteLine($"  已生成 5 个前端文件");
}

// ==================================== 模板生成器 ====================================

string GenerateGetOutputDto(EntityInfo entity, List<EnumInfo> enums)
{
    var hasEnums = entity.EnumTypes.Count > 0;
    var sb = new StringBuilder();
    sb.AppendLine("using Volo.Abp.Application.Dtos;");
    if (hasEnums) sb.AppendLine($"using Yi.Module.{entity.ModuleNamespace}.Domain.Shared.Enums;");
    sb.AppendLine();
    sb.AppendLine($"namespace Yi.Module.{entity.ModuleNamespace}.Application.Contracts.Dtos.{entity.EntityName}");
    sb.AppendLine("{");
    sb.AppendLine($"    /// <summary>");
    sb.AppendLine($"    /// {entity.EntityComment}单个输出DTO");
    sb.AppendLine($"    /// </summary>");
    sb.AppendLine($"    public class {entity.EntityName}GetOutputDto : EntityDto<Guid>");
    sb.AppendLine("    {");

    foreach (var field in entity.Fields.Where(f => !f.IsTreeField))
    {
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// {field.Comment}");
        sb.AppendLine($"        /// </summary>");
        // 枚举字段使用 int 类型
        var fieldType = field.IsEnum ? "int" : (field.Nullable ? field.Type + "?" : field.Type);
        sb.AppendLine($"        public {fieldType} {field.Name} {{ get; set; }}");
    }

    if (entity.IsTree)
    {
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// 父级id");
        sb.AppendLine($"        /// </summary>");
        sb.AppendLine($"        public Guid? ParentId {{ get; set; }}");
    }

    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 排序");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public int OrderNum {{ get; set; }}");
    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 状态");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public bool State {{ get; set; }}");
    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 创建时间");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public DateTime CreationTime {{ get; set; }}");
    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 创建者");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public Guid? CreatorId {{ get; set; }}");
    sb.AppendLine("    }");
    sb.AppendLine("}");

    return sb.ToString();
}

string GenerateGetListOutputDto(EntityInfo entity, List<EnumInfo> enums)
{
    var hasEnums = entity.EnumTypes.Count > 0;
    var sb = new StringBuilder();
    sb.AppendLine("using Volo.Abp.Application.Dtos;");
    if (hasEnums) sb.AppendLine($"using Yi.Module.{entity.ModuleNamespace}.Domain.Shared.Enums;");
    sb.AppendLine();
    sb.AppendLine($"namespace Yi.Module.{entity.ModuleNamespace}.Application.Contracts.Dtos.{entity.EntityName}");
    sb.AppendLine("{");
    sb.AppendLine($"    /// <summary>");
    sb.AppendLine($"    /// {entity.EntityComment}列表输出DTO");
    sb.AppendLine($"    /// </summary>");
    sb.AppendLine($"    public class {entity.EntityName}GetListOutputDto : EntityDto<Guid>");
    sb.AppendLine("    {");

    foreach (var field in entity.Fields.Where(f => !f.IsTreeField))
    {
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// {field.Comment}");
        sb.AppendLine($"        /// </summary>");
        // 枚举字段使用 int 类型
        var fieldType = field.IsEnum ? "int" : (field.Nullable ? field.Type + "?" : field.Type);
        sb.AppendLine($"        public {fieldType} {field.Name} {{ get; set; }}");
    }

    if (entity.IsTree)
    {
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// 父级id");
        sb.AppendLine($"        /// </summary>");
        sb.AppendLine($"        public Guid? ParentId {{ get; set; }}");
    }

    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 排序");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public int OrderNum {{ get; set; }}");
    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 状态");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public bool State {{ get; set; }}");
    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 创建时间");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public DateTime CreationTime {{ get; set; }}");
    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 创建者");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public Guid? CreatorId {{ get; set; }}");
    sb.AppendLine("    }");
    sb.AppendLine("}");

    return sb.ToString();
}

string GenerateGetListInputVo(EntityInfo entity)
{
    var hasEnums = entity.EnumTypes.Count > 0;
    var searchFields = entity.Fields.Where(f => f.IsSearchField).ToList();
    var enumFields = entity.Fields.Where(f => f.IsEnum && !f.IsTreeField).ToList();

    var sb = new StringBuilder();
    sb.AppendLine("using Yi.Framework.Ddd.Application.Contracts;");
    if (hasEnums) sb.AppendLine($"using Yi.Module.{entity.ModuleNamespace}.Domain.Shared.Enums;");
    sb.AppendLine();
    sb.AppendLine($"namespace Yi.Module.{entity.ModuleNamespace}.Application.Contracts.Dtos.{entity.EntityName}");
    sb.AppendLine("{");
    sb.AppendLine($"    /// <summary>");
    sb.AppendLine($"    /// {entity.EntityComment}列表查询输入");
    sb.AppendLine($"    /// </summary>");
    sb.AppendLine($"    public class {entity.EntityName}GetListInputVo : PagedAllResultRequestDto");
    sb.AppendLine("    {");

    // 搜索字段（用于 Contains 查询）
    foreach (var field in searchFields)
    {
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// {field.Comment}");
        sb.AppendLine($"        /// </summary>");
        sb.AppendLine($"        public string? {field.Name} {{ get; set; }}");
    }

    // 枚举字段（用于精确匹配）
    foreach (var field in enumFields)
    {
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// {field.Comment}");
        sb.AppendLine($"        /// </summary>");
        sb.AppendLine($"        public {field.Type}? {field.Name} {{ get; set; }}");
    }

    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 状态");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public bool? State {{ get; set; }}");
    sb.AppendLine("    }");
    sb.AppendLine("}");

    return sb.ToString();
}

string GenerateCreateInputVo(EntityInfo entity, List<EnumInfo> enums)
{
    var hasEnums = entity.EnumTypes.Count > 0;
    var sb = new StringBuilder();
    sb.AppendLine("using System.ComponentModel.DataAnnotations;");
    if (hasEnums) sb.AppendLine($"using Yi.Module.{entity.ModuleNamespace}.Domain.Shared.Enums;");
    sb.AppendLine();
    sb.AppendLine($"namespace Yi.Module.{entity.ModuleNamespace}.Application.Contracts.Dtos.{entity.EntityName}");
    sb.AppendLine("{");
    sb.AppendLine($"    /// <summary>");
    sb.AppendLine($"    /// {entity.EntityComment}创建输入");
    sb.AppendLine($"    /// </summary>");
    sb.AppendLine($"    public class {entity.EntityName}CreateInputVo");
    sb.AppendLine("    {");

    // 树形实体需要 ParentId
    if (entity.IsTree)
    {
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// 父级id");
        sb.AppendLine($"        /// </summary>");
        sb.AppendLine($"        public Guid? ParentId {{ get; set; }}");
    }

    foreach (var field in entity.Fields.Where(f => !f.IsTreeField))
    {
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// {field.Comment}");
        sb.AppendLine($"        /// </summary>");
        if (!field.Nullable) sb.AppendLine("        [Required]");
        sb.AppendLine($"        public {(field.Nullable ? field.Type + "?" : field.Type)} {field.Name} {{ get; set; }}");
    }

    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 排序");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public int OrderNum {{ get; set; }}");
    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 状态");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public bool State {{ get; set; }} = true;");
    sb.AppendLine("    }");
    sb.AppendLine("}");

    return sb.ToString();
}

string GenerateUpdateInputVo(EntityInfo entity, List<EnumInfo> enums)
{
    var hasEnums = entity.EnumTypes.Count > 0;
    var sb = new StringBuilder();
    sb.AppendLine("using System.ComponentModel.DataAnnotations;");
    if (hasEnums) sb.AppendLine($"using Yi.Module.{entity.ModuleNamespace}.Domain.Shared.Enums;");
    sb.AppendLine();
    sb.AppendLine($"namespace Yi.Module.{entity.ModuleNamespace}.Application.Contracts.Dtos.{entity.EntityName}");
    sb.AppendLine("{");
    sb.AppendLine($"    /// <summary>");
    sb.AppendLine($"    /// {entity.EntityComment}更新输入");
    sb.AppendLine($"    /// </summary>");
    sb.AppendLine($"    public class {entity.EntityName}UpdateInputVo");
    sb.AppendLine("    {");

    // 树形实体需要 ParentId
    if (entity.IsTree)
    {
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// 父级id");
        sb.AppendLine($"        /// </summary>");
        sb.AppendLine($"        public Guid? ParentId {{ get; set; }}");
    }

    foreach (var field in entity.Fields.Where(f => !f.IsTreeField))
    {
        sb.AppendLine($"        /// <summary>");
        sb.AppendLine($"        /// {field.Comment}");
        sb.AppendLine($"        /// </summary>");
        if (!field.Nullable) sb.AppendLine("        [Required]");
        sb.AppendLine($"        public {(field.Nullable ? field.Type + "?" : field.Type)} {field.Name} {{ get; set; }}");
    }

    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 排序");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public int OrderNum {{ get; set; }}");
    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// 状态");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public bool State {{ get; set; }}");
    sb.AppendLine("    }");
    sb.AppendLine("}");

    return sb.ToString();
}

string GenerateIService(EntityInfo entity)
{
    var sb = new StringBuilder();
    sb.AppendLine("using Yi.Framework.Ddd.Application.Contracts;");
    sb.AppendLine($"using Yi.Module.{entity.ModuleNamespace}.Application.Contracts.Dtos.{entity.EntityName};");
    sb.AppendLine();
    sb.AppendLine($"namespace Yi.Module.{entity.ModuleNamespace}.Application.Contracts.IServices");
    sb.AppendLine("{");
    sb.AppendLine($"    /// <summary>");
    sb.AppendLine($"    /// {entity.EntityComment}服务接口");
    sb.AppendLine($"    /// </summary>");
    sb.AppendLine($"    public interface I{entity.EntityName}Service : IYiCrudAppService<{entity.EntityName}GetOutputDto, {entity.EntityName}GetListOutputDto, Guid,");
    sb.AppendLine($"        {entity.EntityName}GetListInputVo, {entity.EntityName}CreateInputVo, {entity.EntityName}UpdateInputVo>");
    sb.AppendLine("    {");
    // 树形实体不定义 GetListAsync，由 Service 通过 Route 特性创建独立路由
    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// {entity.EntityComment}下拉列表");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        Task<List<{entity.EntityName}GetOutputDto>> SelectListAsync(string? keywords = null);");
    sb.AppendLine("    }");
    sb.AppendLine("}");

    return sb.ToString();
}

string GenerateService(EntityInfo entity)
{
    var indexField = entity.Fields.FirstOrDefault(f => f.IsIndex);
    var sb = new StringBuilder();
    sb.AppendLine("using SqlSugar;");
    sb.AppendLine("using Volo.Abp;");
    sb.AppendLine("using Volo.Abp.Application.Dtos;");
    if (entity.IsTree)
        sb.AppendLine("using Microsoft.AspNetCore.Mvc;");
    sb.AppendLine("using Yi.Framework.Ddd.Application;");
    sb.AppendLine($"using Yi.Module.{entity.ModuleNamespace}.Application.Contracts.Dtos.{entity.EntityName};");
    sb.AppendLine($"using Yi.Module.{entity.ModuleNamespace}.Application.Contracts.IServices;");
    sb.AppendLine($"using Yi.Module.{entity.ModuleNamespace}.Domain.Entities;");
    if (entity.EnumTypes.Count > 0)
        sb.AppendLine($"using Yi.Module.{entity.ModuleNamespace}.Domain.Shared.Enums;");
    sb.AppendLine("using Yi.Framework.SqlSugarCore.Abstractions;");
    sb.AppendLine();
    sb.AppendLine($"namespace Yi.Module.{entity.ModuleNamespace}.Application.Services");
    sb.AppendLine("{");
    sb.AppendLine($"    /// <summary>");
    sb.AppendLine($"    /// {entity.EntityComment}服务实现");
    sb.AppendLine($"    /// </summary>");
    sb.AppendLine($"    [OperLogEntity(\"{entity.EntityComment}\")]");
    sb.AppendLine($"    [PermissionResource(\"{entity.Module}\", \"{entity.EntityNameLower}\")]");
    sb.AppendLine($"    public class {entity.EntityName}Service : YiCrudAppService<{entity.EntityName}AggregateRoot, {entity.EntityName}GetOutputDto, {entity.EntityName}GetListOutputDto, Guid,");
    sb.AppendLine($"        {entity.EntityName}GetListInputVo, {entity.EntityName}CreateInputVo, {entity.EntityName}UpdateInputVo>, I{entity.EntityName}Service");
    sb.AppendLine("    {");
    sb.AppendLine($"        private readonly ISqlSugarRepository<{entity.EntityName}AggregateRoot, Guid> _repository;");
    sb.AppendLine();
    sb.AppendLine($"        public {entity.EntityName}Service(ISqlSugarRepository<{entity.EntityName}AggregateRoot, Guid> repository) : base(repository) =>");
    sb.AppendLine("            _repository = repository;");
    sb.AppendLine();

    // GetListAsync 方法重写 - 树形实体返回 List，普通实体返回 PagedResultDto
    if (entity.IsTree)
    {
        sb.AppendLine($"        [Route(\"{entity.EntityNameKebab}/list\")]");
        sb.AppendLine($"        public new async Task<List<{entity.EntityName}GetListOutputDto>> GetListAsync({entity.EntityName}GetListInputVo input)");
        sb.AppendLine("        {");
        sb.AppendLine("            var output = await _repository._DbQueryable");
    }
    else
    {
        sb.AppendLine($"        public override async Task<PagedResultDto<{entity.EntityName}GetListOutputDto>> GetListAsync({entity.EntityName}GetListInputVo input)");
        sb.AppendLine("        {");
        sb.AppendLine("            RefAsync<int> total = 0;");
        sb.AppendLine("            var output = await _repository._DbQueryable");
    }

    // 搜索字段（Contains 查询）
    foreach (var field in entity.Fields.Where(f => f.IsSearchField && !f.IsTreeField))
    {
        sb.AppendLine($"                .WhereIF(!string.IsNullOrEmpty(input.{field.Name}), x => x.{field.Name}.Contains(input.{field.Name}!))");
    }

    // 枚举字段（精确匹配）
    foreach (var field in entity.Fields.Where(f => f.IsEnum && !f.IsTreeField))
    {
        sb.AppendLine($"                .WhereIF(input.{field.Name} is not null, x => x.{field.Name} == ({field.EnumType})input.{field.Name}!)");
    }

    sb.AppendLine("                .WhereIF(input.State is not null, x => x.State == input.State)");
    if (!entity.IsTree)
    {
        sb.AppendLine("                .WhereIF(input.StartTime is not null && input.EndTime is not null,");
        sb.AppendLine("                    x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)");
        sb.AppendLine("                .OrderByDescending(x => x.CreationTime)");
    }
    else
    {
        sb.AppendLine("                .OrderBy(x => x.OrderNum, OrderByType.Asc)");
    }
    sb.AppendLine($"                .Select(x => new {entity.EntityName}GetListOutputDto");
    sb.AppendLine("                {");
    sb.AppendLine("                    Id = x.Id,");
    foreach (var field in entity.Fields.Where(f => !f.IsTreeField))
    {
        // 枚举字段需要转换为 int
        if (field.IsEnum)
            sb.AppendLine($"                    {field.Name} = (int)x.{field.Name},");
        else
            sb.AppendLine($"                    {field.Name} = x.{field.Name},");
    }
    if (entity.IsTree)
    {
        sb.AppendLine("                    ParentId = x.ParentId,");
    }
    sb.AppendLine("                    State = x.State,");
    sb.AppendLine("                    OrderNum = x.OrderNum,");
    sb.AppendLine("                    CreationTime = x.CreationTime,");
    sb.AppendLine("                    CreatorId = x.CreatorId");
    sb.AppendLine("                })");
    if (entity.IsTree)
    {
        sb.AppendLine("                .ToListAsync();");
        sb.AppendLine();
        sb.AppendLine("            return output;");
    }
    else
    {
        sb.AppendLine("                .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);");
        sb.AppendLine();
        sb.AppendLine($"            return new PagedResultDto<{entity.EntityName}GetListOutputDto>(total, output);");
    }
    sb.AppendLine("        }");
    sb.AppendLine();

    // 验证逻辑
    if (indexField != null)
    {
        sb.AppendLine($"        protected override async Task CheckCreateInputDtoAsync({entity.EntityName}CreateInputVo input)");
        sb.AppendLine("        {");
        sb.AppendLine($"            if (!string.IsNullOrEmpty(input.{indexField.Name}))");
        sb.AppendLine("            {");
        sb.AppendLine($"                var isExist = await _repository.IsAnyAsync(x => x.{indexField.Name} == input.{indexField.Name});");
        sb.AppendLine("                if (isExist)");
        sb.AppendLine("                {");
        sb.AppendLine($"                    throw new UserFriendlyException(\"{indexField.Comment}已存在\");");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
        sb.AppendLine();
        sb.AppendLine($"        protected override async Task CheckUpdateInputDtoAsync({entity.EntityName}AggregateRoot entity, {entity.EntityName}UpdateInputVo input)");
        sb.AppendLine("        {");
        sb.AppendLine($"            if (!string.IsNullOrEmpty(input.{indexField.Name}))");
        sb.AppendLine("            {");
        sb.AppendLine("                var isExist = await _repository._DbQueryable.Where(x => x.Id != entity.Id)");
        sb.AppendLine($"                    .AnyAsync(x => x.{indexField.Name} == input.{indexField.Name});");
        sb.AppendLine("                if (isExist)");
        sb.AppendLine("                {");
        sb.AppendLine($"                    throw new UserFriendlyException(\"{indexField.Comment}已存在\");");
        sb.AppendLine("                }");
        sb.AppendLine("            }");
        sb.AppendLine("        }");
    }

    // SelectListAsync 下拉列表方法
    var nameField = entity.Fields.FirstOrDefault(f => f.Name == "Name") ?? indexField;
    sb.AppendLine();
    sb.AppendLine($"        /// <summary>");
    sb.AppendLine($"        /// {entity.EntityComment}下拉列表");
    sb.AppendLine($"        /// </summary>");
    sb.AppendLine($"        public async Task<List<{entity.EntityName}GetOutputDto>> SelectListAsync(string? keywords = null)");
    sb.AppendLine("        {");
    sb.AppendLine($"            var query = _repository._DbQueryable.Where(x => x.State == true)");
    if (nameField != null)
    {
        sb.AppendLine($"                .WhereIF(!string.IsNullOrEmpty(keywords), x => x.{nameField.Name}.Contains(keywords!))");
    }
    sb.AppendLine($"                .Select(x => new {entity.EntityName}GetOutputDto");
    sb.AppendLine("                {");
    sb.AppendLine("                    Id = x.Id,");
    foreach (var field in entity.Fields.Where(f => !f.IsTreeField))
    {
        // 枚举字段需要转换为 int
        if (field.IsEnum)
            sb.AppendLine($"                    {field.Name} = (int)x.{field.Name},");
        else
            sb.AppendLine($"                    {field.Name} = x.{field.Name},");
    }
    if (entity.IsTree)
    {
        sb.AppendLine("                    ParentId = x.ParentId,");
    }
    sb.AppendLine("                });");
    sb.AppendLine();
    sb.AppendLine("            return await query.ToListAsync();");
    sb.AppendLine("        }");

    sb.AppendLine("    }");
    sb.AppendLine("}");

    return sb.ToString();
}

string GenerateModelTs(EntityInfo entity, List<EnumInfo> enums)
{
    var sb = new StringBuilder();
    sb.AppendLine($"/** {entity.EntityComment}实体接口 */");
    sb.AppendLine($"export interface {entity.EntityName} {{");
    sb.AppendLine("  id: string;");
    sb.AppendLine("  creationTime: string;");
    sb.AppendLine("  creatorId?: string;");

    foreach (var field in entity.Fields.Where(f => !f.IsTreeField))
    {
        var tsType = MapToTsType(field.Type, field.IsEnum);
        sb.AppendLine($"  /** {field.Comment} */");
        sb.AppendLine($"  {ToLower(field.Name)}: {(field.Nullable ? tsType + " | null" : tsType)};");
    }

    sb.AppendLine("  orderNum: number;");
    sb.AppendLine("  state: boolean;");
    sb.AppendLine("  remark?: string | null;");

    if (entity.IsTree)
    {
        sb.AppendLine("  parentId: string | null;");
        sb.AppendLine($"  children?: {entity.EntityName}[];");
    }

    sb.AppendLine("}");

    // 创建输入
    sb.AppendLine();
    sb.AppendLine($"/** {entity.EntityComment}创建输入 */");
    sb.AppendLine($"export interface {entity.EntityName}CreateInput {{");
    // 树形实体需要 parentId
    if (entity.IsTree)
    {
        sb.AppendLine("  parentId?: string | null;");
    }
    foreach (var field in entity.Fields.Where(f => !f.IsTreeField))
    {
        var tsType = MapToTsType(field.Type, field.IsEnum);
        sb.AppendLine($"  {ToLower(field.Name)}: {(field.Nullable ? tsType + " | null" : tsType)};");
    }
    sb.AppendLine("  orderNum?: number;");
    sb.AppendLine("  state?: boolean;");
    sb.AppendLine("  remark?: string | null;");
    sb.AppendLine("}");

    // 更新输入
    sb.AppendLine();
    sb.AppendLine($"/** {entity.EntityComment}更新输入 */");
    sb.AppendLine($"export interface {entity.EntityName}UpdateInput {{");
    sb.AppendLine("  id: string;");
    // 树形实体需要 parentId
    if (entity.IsTree)
    {
        sb.AppendLine("  parentId?: string | null;");
    }
    foreach (var field in entity.Fields.Where(f => !f.IsTreeField))
    {
        var tsType = MapToTsType(field.Type, field.IsEnum);
        sb.AppendLine($"  {ToLower(field.Name)}: {(field.Nullable ? tsType + " | null" : tsType)};");
    }
    sb.AppendLine("  orderNum?: number;");
    sb.AppendLine("  state?: boolean;");
    sb.AppendLine("  remark?: string | null;");
    sb.AppendLine("}");

    // 列表查询参数 - 仅包含搜索字段和枚举字段
    sb.AppendLine();
    sb.AppendLine($"/** {entity.EntityComment}列表查询参数 */");
    sb.AppendLine($"export interface {entity.EntityName}ListParams {{");
    foreach (var field in entity.Fields.Where(f => f.IsSearchField || f.IsEnum))
    {
        var tsType = MapToTsType(field.Type, field.IsEnum);
        sb.AppendLine($"  {ToLower(field.Name)}?: {tsType};");
    }
    sb.AppendLine("  state?: boolean;");
    sb.AppendLine("  startTime?: string;");
    sb.AppendLine("  endTime?: string;");
    sb.AppendLine("}");

    return sb.ToString();
}

string GenerateIndexTs(EntityInfo entity)
{
    var sb = new StringBuilder();
    if (!entity.IsTree)
    {
        sb.AppendLine("import type { ID, IDS, PageResult } from '#/api/common';");
    }
    else
    {
        sb.AppendLine("import type { ID, IDS } from '#/api/common';");
    }
    sb.AppendLine($"import type {{ {entity.EntityName}, {entity.EntityName}ListParams, {entity.EntityName}CreateInput, {entity.EntityName}UpdateInput }} from './model';");
    sb.AppendLine("import { requestClient } from '#/api/request';");
    sb.AppendLine();
    sb.AppendLine("enum Api {");
    sb.AppendLine($"  root = '/{entity.EntityNameKebab}',");
    sb.AppendLine("}");
    sb.AppendLine();
    if (entity.IsTree)
    {
        sb.AppendLine($"/** {entity.EntityComment}列表 */");
        sb.AppendLine($"export function {entity.EntityNameLower}List(params?: {entity.EntityName}ListParams) {{");
        sb.AppendLine($"  return requestClient.get<{entity.EntityName}[]>(`${{Api.root}}/list`, {{ params }});");
    }
    else
    {
        sb.AppendLine($"/** {entity.EntityComment}分页列表 */");
        sb.AppendLine($"export function {entity.EntityNameLower}List(params?: {entity.EntityName}ListParams) {{");
        sb.AppendLine($"  return requestClient.get<PageResult<{entity.EntityName}>>(Api.root, {{ params }});");
    }
    sb.AppendLine("}");
    sb.AppendLine();
    sb.AppendLine($"/** {entity.EntityComment}详情 */");
    sb.AppendLine($"export function {entity.EntityNameLower}Info({entity.EntityNameLower}Id: ID) {{");
    sb.AppendLine($"  return requestClient.get<{entity.EntityName}>(`${{Api.root}}/${{{entity.EntityNameLower}Id}}`);");
    sb.AppendLine("}");
    sb.AppendLine();
    sb.AppendLine($"/** {entity.EntityComment}新增 */");
    sb.AppendLine($"export function {entity.EntityNameLower}Add(data: {entity.EntityName}CreateInput) {{");
    sb.AppendLine("  return requestClient.postWithMsg<void>(Api.root, data);");
    sb.AppendLine("}");
    sb.AppendLine();
    sb.AppendLine($"/** {entity.EntityComment}更新 */");
    sb.AppendLine($"export function {entity.EntityNameLower}Update(data: {entity.EntityName}UpdateInput) {{");
    sb.AppendLine("  return requestClient.putWithMsg<void>(`${Api.root}/${data.id}`, data);");
    sb.AppendLine("}");
    sb.AppendLine();
    sb.AppendLine($"/** {entity.EntityComment}删除 */");
    sb.AppendLine($"export function {entity.EntityNameLower}Remove({entity.EntityNameLower}Ids: IDS) {{");
    sb.AppendLine("  return requestClient.deleteWithMsg<void>(Api.root, {");
    sb.AppendLine($"    params: {{ ids: {entity.EntityNameLower}Ids }},");
    sb.AppendLine("  });");
    sb.AppendLine("}");
    sb.AppendLine();
    sb.AppendLine($"/** {entity.EntityComment}下拉列表 */");
    sb.AppendLine($"export function {entity.EntityNameLower}SelectList(keywords?: string) {{");
    sb.AppendLine($"  return requestClient.get<{entity.EntityName}[]>(`${{Api.root}}/select-list`, {{");
    sb.AppendLine("    params: keywords ? { keywords } : undefined,");
    sb.AppendLine("  });");
    sb.AppendLine("}");

    return sb.ToString();
}

string GenerateDataTs(EntityInfo entity, List<EnumInfo> enums)
{
    var enumFields = entity.Fields.Where(f => f.IsEnum).ToList();
    var searchFields = entity.Fields.Where(f => f.IsSearchField).ToList();
    var sb = new StringBuilder();
    sb.AppendLine("import type { FormSchemaGetter } from '#/adapter/form';");
    sb.AppendLine("import type { VxeGridProps } from '#/adapter/vxe-table';");
    sb.AppendLine("import { DictEnum } from '#/constants';");
    sb.AppendLine("import { getPopupContainer } from '@vben/utils';");
    sb.AppendLine("import { getDictOptions } from '#/utils/dict';");
    sb.AppendLine("import { renderDict } from '#/utils/render';");
    sb.AppendLine();

    var moduleConst = entity.Module.Replace("-", "_").ToUpperInvariant();

    // querySchema - 搜索字段 + 枚举字段 + 状态 + 时间
    sb.AppendLine("export const querySchema: FormSchemaGetter = () => [");
    foreach (var field in searchFields)
    {
        sb.AppendLine($"  {{ component: 'Input', fieldName: '{ToLower(field.Name)}', label: '{field.Comment}' }},");
    }
    foreach (var field in enumFields)
    {
        var dictConst = $"{moduleConst}_{ToSnakeCaseUpper(field.EnumType?.Replace("Enum", "").Replace(entity.EntityName, ""))}";
        sb.AppendLine($"  {{");
        sb.AppendLine($"    component: 'Select',");
        sb.AppendLine($"    componentProps: {{ getPopupContainer, options: getDictOptions(DictEnum.{dictConst}, true) }},");
        sb.AppendLine($"    fieldName: '{ToLower(field.Name)}',");
        sb.AppendLine($"    label: '{field.Comment}',");
        sb.AppendLine($"  }},");
    }
    sb.AppendLine("  { component: 'Select', componentProps: { getPopupContainer, options: [{ label: '启用', value: true }, { label: '禁用', value: false }] }, fieldName: 'state', label: '状态' },");
    sb.AppendLine("  { component: 'RangePicker', fieldName: 'creationTime', label: '创建时间' },");
    sb.AppendLine("];");
    sb.AppendLine();

    // columns - 根据字段语义选择高价值业务字段 + 标准字段
    var listColumns = SelectListColumns(entity);
    sb.AppendLine("export const columns: VxeGridProps['columns'] = [");
    sb.AppendLine("  { type: 'checkbox', width: 60 },");
    for (var i = 0; i < listColumns.Count; i++)
    {
        var field = listColumns[i];
        if (field.IsEnum)
        {
            var dictConst = $"{moduleConst}_{ToSnakeCaseUpper(field.EnumType?.Replace("Enum", "").Replace(entity.EntityName, ""))}";
            sb.AppendLine($"  {{");
            sb.AppendLine($"    title: '{field.Comment}',");
            sb.AppendLine($"    field: '{ToLower(field.Name)}',");
            sb.AppendLine($"    width: {GetColumnWidth(field)},");
            if (entity.IsTree && i == 0)
            {
                sb.AppendLine($"    treeNode: true,");
            }
            sb.AppendLine($"    slots: {{ default: ({{ row }}) => renderDict(row.{ToLower(field.Name)}, DictEnum.{dictConst}) }},");
            sb.AppendLine($"  }},");
        }
        else
        {
            sb.AppendLine($"  {{ title: '{field.Comment}', field: '{ToLower(field.Name)}', width: {GetColumnWidth(field)}{(entity.IsTree && i == 0 ? ", treeNode: true" : "")} }},");
        }
    }
    sb.AppendLine("  { title: '排序', field: 'orderNum', width: 80 },");
    sb.AppendLine("  { title: '状态', field: 'state', width: 100, slots: { default: ({ row }) => renderDict(String(row.state), DictEnum.SYS_NORMAL_DISABLE) } },");
    sb.AppendLine("  { title: '备注', field: 'remark' },");
    sb.AppendLine("  { title: '创建时间', field: 'creationTime' },");
    sb.AppendLine("  { field: 'action', fixed: 'right', slots: { default: 'action' }, title: '操作', resizable: false, width: 200 },");
    sb.AppendLine("];");
    sb.AppendLine();

    // drawerSchema - 树形parentId + 搜索字段（索引字段必填） + 枚举字段 + 标准字段
    var indexField = entity.Fields.FirstOrDefault(f => f.IsIndex);
    sb.AppendLine("export const drawerSchema: FormSchemaGetter = () => [");
    sb.AppendLine("  { component: 'Input', dependencies: { show: () => false, triggerFields: [''] }, fieldName: 'id' },");
    if (entity.IsTree)
    {
        sb.AppendLine("  {");
        sb.AppendLine("    component: 'TreeSelect',");
        sb.AppendLine("    componentProps: { getPopupContainer },");
        sb.AppendLine("    dependencies: { show: (model) => model.parentId !== '00000000-0000-0000-0000-000000000000', triggerFields: ['parentId'] },");
        sb.AppendLine("    fieldName: 'parentId',");
        sb.AppendLine($"    label: '上级{entity.EntityComment}',");
        sb.AppendLine("    rules: 'selectRequired',");
        sb.AppendLine("  },");
    }
    if (indexField != null)
    {
        sb.AppendLine($"  {{ component: 'Input', fieldName: '{ToLower(indexField.Name)}', label: '{indexField.Comment}', rules: 'required' }},");
    }
    foreach (var field in enumFields)
    {
        var dictConst = $"{moduleConst}_{ToSnakeCaseUpper(field.EnumType?.Replace("Enum", "").Replace(entity.EntityName, ""))}";
        sb.AppendLine($"  {{");
        sb.AppendLine($"    component: 'Select',");
        sb.AppendLine($"    componentProps: {{ getPopupContainer, options: getDictOptions(DictEnum.{dictConst}, true) }},");
        sb.AppendLine($"    fieldName: '{ToLower(field.Name)}',");
        sb.AppendLine($"    label: '{field.Comment}',");
        sb.AppendLine($"    rules: 'required',");
        sb.AppendLine($"  }},");
    }
    sb.AppendLine("  { component: 'InputNumber', fieldName: 'orderNum', label: '排序', defaultValue: 0 },");
    sb.AppendLine("  { component: 'Textarea', fieldName: 'remark', label: '备注' },");
    sb.AppendLine("  { component: 'RadioGroup', componentProps: { buttonStyle: 'solid', options: [{ label: '启用', value: true }, { label: '禁用', value: false }], optionType: 'button' }, defaultValue: true, fieldName: 'state', label: '状态' },");
    sb.AppendLine("];");

    return sb.ToString();
}

string GenerateIndexVue(EntityInfo entity)
{
    var sb = new StringBuilder();
    sb.AppendLine("<script setup lang=\"ts\">");
    sb.AppendLine("import type { VbenFormProps } from '@vben/common-ui';");
    sb.AppendLine("import type { VxeGridProps } from '#/adapter/vxe-table';");
    sb.AppendLine($"import type {{ {entity.EntityName} }} from '#/api/{entity.EntityNameKebab}/model';");
    sb.AppendLine();

    if (entity.IsTree)
    {
        sb.AppendLine("import { nextTick } from 'vue';");
    }
    sb.AppendLine("import { Page, useVbenDrawer } from '@vben/common-ui';");

    if (entity.IsTree)
    {
        sb.AppendLine("import { eachTree } from '@vben/utils';");
    }
    sb.AppendLine("import { Space } from 'antdv-next';");
    sb.AppendLine();
    if (entity.IsTree)
    {
        sb.AppendLine("import { useVbenVxeGrid, VbenTableAction } from '#/adapter/vxe-table';");
    }
    else
    {
        sb.AppendLine("import { useVbenVxeGrid, VbenTableAction, vxeCheckboxChecked } from '#/adapter/vxe-table';");
    }
    sb.AppendLine($"import {{ {entity.EntityNameLower}List, {entity.EntityNameLower}Remove }} from '#/api/{entity.EntityNameKebab}';");
    sb.AppendLine();
    sb.AppendLine("import { columns, querySchema } from './data';");
    sb.AppendLine($"import {entity.EntityNameLower}Drawer from './{entity.EntityNameKebab}-drawer.vue';");
    sb.AppendLine();

    if (entity.IsTree)
    {
        sb.AppendLine("// 空GUID，用于判断根节点");
        sb.AppendLine("const EMPTY_GUID = '00000000-0000-0000-0000-000000000000';");
        sb.AppendLine();
    }

    sb.AppendLine("const formOptions: VbenFormProps = {");
    sb.AppendLine("  commonConfig: { labelWidth: 80, componentProps: { allowClear: true } },");
    sb.AppendLine("  schema: querySchema(),");
    sb.AppendLine("  wrapperClass: 'grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4',");
    if (!entity.IsTree)
    {
        sb.AppendLine("  fieldMappingTime: [['creationTime', ['startTime', 'endTime'], ['YYYY-MM-DD 00:00:00', 'YYYY-MM-DD 23:59:59']]],");
    }
    sb.AppendLine("};");
    sb.AppendLine();

    sb.AppendLine("const gridOptions: VxeGridProps = {");
    if (!entity.IsTree)
    {
        sb.AppendLine("  checkboxConfig: { highlight: true, reserve: true },");
    }
    sb.AppendLine("  columns,");
    sb.AppendLine("  height: 'auto',");
    sb.AppendLine("  keepSource: true,");

    if (entity.IsTree)
    {
        sb.AppendLine("  pagerConfig: { enabled: false },");
        sb.AppendLine("  proxyConfig: {");
        sb.AppendLine("    ajax: {");
        sb.AppendLine("      query: async (_, formValues = {}) => {");
        sb.AppendLine($"        const resp = await {entity.EntityNameLower}List(formValues);");
        sb.AppendLine("        // 将根节点的 parentId 置为 null，以便 vxe-table 正确识别根节点");
        sb.AppendLine("        const items = resp.map((item) => ({");
        sb.AppendLine("          ...item,");
        sb.AppendLine("          parentId: item.parentId === EMPTY_GUID ? null : item.parentId,");
        sb.AppendLine("        }));");
        sb.AppendLine("        return { items };");
        sb.AppendLine("      },");
        sb.AppendLine("      querySuccess: () => {");
        sb.AppendLine("        // 默认展开全部");
        sb.AppendLine("        eachTree(tableApi.grid.getData(), (item) => (item.expand = true));");
        sb.AppendLine("        nextTick(() => {");
        sb.AppendLine("          setExpandOrCollapse(true);");
        sb.AppendLine("        });");
        sb.AppendLine("      },");
        sb.AppendLine("    },");
        sb.AppendLine("  },");
        sb.AppendLine("  rowConfig: { keyField: 'id' },");
        sb.AppendLine("  treeConfig: {");
        sb.AppendLine("    parentField: 'parentId',");
        sb.AppendLine("    rowField: 'id',");
        sb.AppendLine("    transform: true,");
        sb.AppendLine("  },");
    }
    else
    {
        sb.AppendLine("  pagerConfig: {},");
        sb.AppendLine("  proxyConfig: {");
        sb.AppendLine("    ajax: {");
        sb.AppendLine("      query: async ({ page }, formValues = {}) => {");
        sb.AppendLine($"        return await {entity.EntityNameLower}List({{");
        sb.AppendLine("          SkipCount: page.currentPage,");
        sb.AppendLine("          MaxResultCount: page.pageSize,");
        sb.AppendLine("          ...formValues,");
        sb.AppendLine("        });");
        sb.AppendLine("      },");
        sb.AppendLine("    },");
        sb.AppendLine("  },");
        sb.AppendLine("  rowConfig: { keyField: 'id' },");
    }
    sb.AppendLine($"  id: '{entity.Module}-{entity.EntityNameKebab}-index',");
    sb.AppendLine("};");
    sb.AppendLine();

    if (entity.IsTree)
    {
        sb.AppendLine("const [BasicTable, tableApi] = useVbenVxeGrid({");
        sb.AppendLine("  formOptions,");
        sb.AppendLine("  gridOptions,");
        sb.AppendLine("  gridEvents: {");
        sb.AppendLine("    cellDblclick: (e) => {");
        sb.AppendLine("      const { row = {} } = e;");
        sb.AppendLine("      if (!row?.children) return;");
        sb.AppendLine("      const isExpanded = row?.expand;");
        sb.AppendLine("      tableApi.grid.setTreeExpand(row, !isExpanded);");
        sb.AppendLine("      row.expand = !isExpanded;");
        sb.AppendLine("    },");
        sb.AppendLine("    toggleTreeExpand: (e) => {");
        sb.AppendLine("      const { row = {}, expanded } = e;");
        sb.AppendLine("      row.expand = expanded;");
        sb.AppendLine("    },");
        sb.AppendLine("  },");
        sb.AppendLine("});");
    }
    else
    {
        sb.AppendLine($"const [BasicTable, tableApi] = useVbenVxeGrid({{ formOptions, gridOptions }});");
    }
    sb.AppendLine($"const [{entity.EntityName}Drawer, drawerApi] = useVbenDrawer({{ connectedComponent: {entity.EntityNameLower}Drawer }});");
    sb.AppendLine();

    sb.AppendLine($"function handleAdd() {{ drawerApi.setData({{ update: false }}); drawerApi.open(); }}");

    if (entity.IsTree)
    {
        sb.AppendLine();
        sb.AppendLine($"function handleSubAdd(row: {entity.EntityName}) {{");
        sb.AppendLine("  const { id } = row;");
        sb.AppendLine("  drawerApi.setData({ id, update: false });");
        sb.AppendLine("  drawerApi.open();");
        sb.AppendLine("}");
    }

    sb.AppendLine($"async function handleEdit(record: {entity.EntityName}) {{ drawerApi.setData({{ id: record.id, update: true }}); drawerApi.open(); }}");
    sb.AppendLine($"async function handleDelete(row: {entity.EntityName}) {{ await {entity.EntityNameLower}Remove([row.id]); await tableApi.query(); }}");

    if (!entity.IsTree)
    {
        sb.AppendLine("async function handleMultiDelete() {");
        sb.AppendLine("  const rows = tableApi.grid.getCheckboxRecords();");
        sb.AppendLine($"  const ids = rows.map((row: {entity.EntityName}) => row.id);");
        sb.AppendLine($"  await {entity.EntityNameLower}Remove(ids);");
        sb.AppendLine("  await tableApi.query();");
        sb.AppendLine("}");
    }

    if (entity.IsTree)
    {
        sb.AppendLine();
        sb.AppendLine("function setExpandOrCollapse(expand: boolean) {");
        sb.AppendLine("  eachTree(tableApi.grid.getData(), (item) => (item.expand = expand));");
        sb.AppendLine("  tableApi.grid?.setAllTreeExpand(expand);");
        sb.AppendLine("}");
    }

    sb.AppendLine("</script>");
    sb.AppendLine();
    sb.AppendLine("<template>");
    sb.AppendLine("  <Page :auto-content-height=\"true\">");
    if (entity.IsTree)
    {
        sb.AppendLine($"    <BasicTable table-title=\"{entity.EntityComment}列表\" table-title-help=\"双击展开/收起子节点\">");
    }
    else
    {
        sb.AppendLine($"    <BasicTable table-title=\"{entity.EntityComment}列表\">");
    }
    sb.AppendLine("      <template #toolbar-tools>");
    sb.AppendLine("        <Space>");
    if (entity.IsTree)
    {
        sb.AppendLine("          <a-button @click=\"setExpandOrCollapse(false)\">");
        sb.AppendLine("            {{ $t('pages.common.collapse') }}");
        sb.AppendLine("          </a-button>");
        sb.AppendLine("          <a-button @click=\"setExpandOrCollapse(true)\">");
        sb.AppendLine("            {{ $t('pages.common.expand') }}");
        sb.AppendLine("          </a-button>");
    }
    else
    {
        sb.AppendLine($"          <a-button :disabled=\"!vxeCheckboxChecked(tableApi)\" danger type=\"primary\" v-access:code=\"['{entity.Module}:{entity.EntityNameKebab}:remove']\" @click=\"handleMultiDelete\">");
        sb.AppendLine("            {{ $t('pages.common.delete') }}");
        sb.AppendLine("          </a-button>");
    }
    sb.AppendLine($"          <a-button type=\"primary\" v-access:code=\"['{entity.Module}:{entity.EntityNameKebab}:add']\" @click=\"handleAdd\">");
    sb.AppendLine("            {{ $t('pages.common.add') }}");
    sb.AppendLine("          </a-button>");
    sb.AppendLine("        </Space>");
    sb.AppendLine("      </template>");
    sb.AppendLine("      <template #action=\"{ row }\">");
    sb.AppendLine("        <VbenTableAction");
    sb.AppendLine("          :actions=\"[");
    sb.AppendLine("            {");
    sb.AppendLine($"              auth: '{entity.Module}:{entity.EntityNameKebab}:edit',");
    sb.AppendLine("              onClick: () => handleEdit(row),");
    sb.AppendLine("              text: $t('pages.common.edit'),");
    sb.AppendLine("            },");
    if (entity.IsTree)
    {
        sb.AppendLine("            {");
        sb.AppendLine($"              auth: '{entity.Module}:{entity.EntityNameKebab}:add',");
        sb.AppendLine("              class: 'text-success hover:text-success',");
        sb.AppendLine("              onClick: () => handleSubAdd(row),");
        sb.AppendLine("              text: $t('pages.common.add'),");
        sb.AppendLine("            },");
    }
    sb.AppendLine("            {");
    sb.AppendLine($"              auth: '{entity.Module}:{entity.EntityNameKebab}:remove',");
    sb.AppendLine("              danger: true,");
    sb.AppendLine("              popConfirm: {");
    sb.AppendLine("                title: '确认删除？',");
    sb.AppendLine("                confirm: () => handleDelete(row),");
    sb.AppendLine("              },");
    sb.AppendLine("              text: $t('pages.common.delete'),");
    sb.AppendLine("            },");
    sb.AppendLine("          ]\"");
    sb.AppendLine("          align=\"center\"");
    sb.AppendLine("        />");
    sb.AppendLine("      </template>");
    sb.AppendLine("    </BasicTable>");
    sb.AppendLine($"    <{entity.EntityName}Drawer @reload=\"tableApi.query()\" />");
    sb.AppendLine("  </Page>");
    sb.AppendLine("</template>");

    return sb.ToString();
}

string GenerateDrawerVue(EntityInfo entity)
{
    var sb = new StringBuilder();
    sb.AppendLine("<script setup lang=\"ts\">");
    if (entity.IsTree)
    {
        sb.AppendLine($"import type {{ {entity.EntityName} }} from '#/api/{entity.EntityNameKebab}/model';");
    }
    sb.AppendLine("import { computed, ref } from 'vue';");
    sb.AppendLine("import { useVbenDrawer } from '@vben/common-ui';");
    sb.AppendLine("import { $t } from '@vben/locales';");
    if (entity.IsTree)
    {
        sb.AppendLine("import { addFullName, cloneDeep, listToTree } from '@vben/utils';");
    }
    else
    {
        sb.AppendLine("import { cloneDeep } from '@vben/utils';");
    }
    sb.AppendLine("import { useVbenForm } from '#/adapter/form';");
    sb.AppendLine($"import type {{ {entity.EntityName}CreateInput, {entity.EntityName}UpdateInput }} from '#/api/{entity.EntityNameKebab}/model';");
    if (entity.IsTree)
    {
        sb.AppendLine($"import {{ {entity.EntityNameLower}Add, {entity.EntityNameLower}Info, {entity.EntityNameLower}List, {entity.EntityNameLower}Update }} from '#/api/{entity.EntityNameKebab}';");
    }
    else
    {
        sb.AppendLine($"import {{ {entity.EntityNameLower}Add, {entity.EntityNameLower}Info, {entity.EntityNameLower}Update }} from '#/api/{entity.EntityNameKebab}';");
    }
    sb.AppendLine("import { defaultFormValueGetter, useBeforeCloseDiff } from '#/utils/popup';");
    sb.AppendLine("import { drawerSchema } from './data';");
    sb.AppendLine();
    sb.AppendLine("const emit = defineEmits<{ reload: [] }>();");
    sb.AppendLine();
    sb.AppendLine("interface DrawerProps { id?: number | string; update: boolean; }");
    sb.AppendLine("const isUpdate = ref(false);");
    sb.AppendLine("const title = computed(() => isUpdate.value ? $t('pages.common.edit') : $t('pages.common.add'));");
    sb.AppendLine();
    sb.AppendLine("const [BasicForm, formApi] = useVbenForm({");
    sb.AppendLine("  commonConfig: { componentProps: { class: 'w-full' }, formItemClass: 'col-span-2', labelWidth: 80 },");
    sb.AppendLine("  schema: drawerSchema(),");
    sb.AppendLine("  showDefaultActions: false,");
    sb.AppendLine("  wrapperClass: 'grid-cols-2',");
    sb.AppendLine("});");
    sb.AppendLine();

    if (entity.IsTree)
    {
        sb.AppendLine($"async function get{entity.EntityName}Tree(entityId?: number | string) {{");
        sb.AppendLine($"  const ret = await {entity.EntityNameLower}List({{}});");
        sb.AppendLine("  // 编辑时排除自己（防止选择自己作为父级）");
        sb.AppendLine($"  const filtered = isUpdate.value && entityId ? ret.filter((item) => item.id !== entityId) : ret;");
        sb.AppendLine("  const treeData = listToTree(filtered, { id: 'id', pid: 'parentId' });");
        sb.AppendLine("  // 添加完整路径名称 如 xx-xx-xx");
        var nameField = entity.Fields.FirstOrDefault(f => f.Name == "Name") ?? entity.Fields.FirstOrDefault(f => f.IsIndex);
        if (nameField != null)
        {
            sb.AppendLine($"  addFullName(treeData, '{ToLower(nameField.Name)}', ' / ');");
        }
        sb.AppendLine("  return treeData;");
        sb.AppendLine("}");
        sb.AppendLine();
        sb.AppendLine($"async function init{entity.EntityName}Select(entityId?: number | string) {{");
        sb.AppendLine($"  const treeData = await get{entity.EntityName}Tree(entityId);");
        sb.AppendLine("  formApi.updateSchema([");
        sb.AppendLine("    {");
        sb.AppendLine("      componentProps: {");
        if (nameField != null)
        {
            sb.AppendLine($"        fieldNames: {{ label: '{ToLower(nameField.Name)}', value: 'id' }},");
        }
        sb.AppendLine("        showSearch: true,");
        sb.AppendLine("        treeData,");
        sb.AppendLine("        treeDefaultExpandAll: true,");
        sb.AppendLine("        treeLine: { showLeafIcon: false },");
        sb.AppendLine("        // 选中后显示在输入框的值");
        sb.AppendLine("        treeNodeLabelProp: 'fullName',");
        sb.AppendLine("      },");
        sb.AppendLine("      fieldName: 'parentId',");
        sb.AppendLine("    },");
        sb.AppendLine("  ]);");
        sb.AppendLine("}");
        sb.AppendLine();
    }

    sb.AppendLine("const { onBeforeClose, markInitialized, resetInitialized } = useBeforeCloseDiff({");
    sb.AppendLine("  initializedGetter: defaultFormValueGetter(formApi), currentGetter: defaultFormValueGetter(formApi),");
    sb.AppendLine("});");
    sb.AppendLine();
    sb.AppendLine("const [BasicDrawer, drawerApi] = useVbenDrawer({");
    sb.AppendLine("  onBeforeClose,");
    sb.AppendLine("  onClosed: handleClosed,");
    sb.AppendLine("  onConfirm: handleConfirm,");
    sb.AppendLine("  async onOpenChange(isOpen) {");
    sb.AppendLine("    if (!isOpen) return null;");
    sb.AppendLine("    drawerApi.drawerLoading(true);");
    sb.AppendLine("    const { id, update } = drawerApi.getData() as DrawerProps;");
    sb.AppendLine("    isUpdate.value = update;");
    if (entity.IsTree)
    {
        sb.AppendLine();
        sb.AppendLine("    if (id) {");
        sb.AppendLine("      await formApi.setFieldValue('parentId', id);");
        sb.AppendLine("      if (update) {");
        sb.AppendLine($"        const record = await {entity.EntityNameLower}Info(id);");
        sb.AppendLine("        await formApi.setValues(record);");
        sb.AppendLine("      }");
        sb.AppendLine("    }");
        sb.AppendLine();
        sb.AppendLine($"    // 初始化{entity.EntityComment}树选择");
        sb.AppendLine($"    await init{entity.EntityName}Select(id);");
    }
    else
    {
        sb.AppendLine("    if (id && update) {");
        sb.AppendLine($"      const record = await {entity.EntityNameLower}Info(id);");
        sb.AppendLine("      await formApi.setValues(record);");
        sb.AppendLine("    }");
    }
    sb.AppendLine("    await markInitialized();");
    sb.AppendLine("    drawerApi.drawerLoading(false);");
    sb.AppendLine("  },");
    sb.AppendLine("});");
    sb.AppendLine();
    sb.AppendLine("async function handleConfirm() {");
    sb.AppendLine("  try {");
    sb.AppendLine("    drawerApi.lock(true);");
    sb.AppendLine("    const { valid } = await formApi.validate();");
    sb.AppendLine("    if (!valid) return;");
    sb.AppendLine($"    const data = cloneDeep(await formApi.getValues()) as {entity.EntityName}CreateInput | {entity.EntityName}UpdateInput;");
    sb.AppendLine($"    await (isUpdate.value ? {entity.EntityNameLower}Update(data as {entity.EntityName}UpdateInput) : {entity.EntityNameLower}Add(data as {entity.EntityName}CreateInput));");
    sb.AppendLine("    resetInitialized();");
    sb.AppendLine("    emit('reload');");
    sb.AppendLine("    drawerApi.close();");
    sb.AppendLine("  } catch (error) {");
    sb.AppendLine("    console.error(error);");
    sb.AppendLine("  } finally {");
    sb.AppendLine("    drawerApi.lock(false);");
    sb.AppendLine("  }");
    sb.AppendLine("}");
    sb.AppendLine();
    sb.AppendLine("async function handleClosed() { await formApi.resetForm(); resetInitialized(); }");
    sb.AppendLine("</script>");
    sb.AppendLine();
    sb.AppendLine("<template>");
    sb.AppendLine("  <BasicDrawer :title=\"title\" class=\"w-[600px]\">");
    sb.AppendLine("    <BasicForm />");
    sb.AppendLine("  </BasicDrawer>");
    sb.AppendLine("</template>");

    return sb.ToString();
}

string MapToTsType(string csType, bool isEnum)
{
    if (isEnum) return "number";
    return csType switch
    {
        "string" => "string",
        "int" => "number",
        "long" => "number",
        "bool" => "boolean",
        "Guid" => "string",
        "DateTime" => "string",
        "decimal" => "number",
        "double" => "number",
        _ => "string"
    };
}

// ==================================== 辅助函数 ====================================

void WriteFile(string path, string content)
{
    File.WriteAllText(path, content);
    Console.WriteLine($"  已创建: {Path.GetFileName(path)}");
}

string ToPascal(string value)
{
    if (string.IsNullOrEmpty(value)) return "";
    // 处理连字符：product-manage → ProductManage
    var parts = value.Split('-', '_');
    return string.Join("", parts.Select(p =>
        string.IsNullOrEmpty(p) ? "" :
        p.Length == 1 ? p.ToUpperInvariant() :
        char.ToUpperInvariant(p[0]) + p.Substring(1).ToLowerInvariant()));
}

string ToLower(string value)
{
    if (string.IsNullOrEmpty(value)) return "";
    if (value.Length == 1) return value.ToLowerInvariant();
    return char.ToLowerInvariant(value[0]) + value.Substring(1);
}

string ToKebabCase(string value)
{
    if (string.IsNullOrEmpty(value)) return "";
    // 驼峰转短横线：ProductCategory → product-category
    var result = new StringBuilder();
    for (var i = 0; i < value.Length; i++)
    {
        var c = value[i];
        if (char.IsUpper(c))
        {
            if (i > 0) result.Append('-');
            result.Append(char.ToLowerInvariant(c));
        }
        else
        {
            result.Append(c);
        }
    }
    return result.ToString();
}

string ToSnakeCaseUpper(string? value)
{
    if (string.IsNullOrEmpty(value)) return "";
    // 驼峰转下划线分隔大写：Test1Type → TEST1_TYPE
    var result = new StringBuilder();
    for (var i = 0; i < value.Length; i++)
    {
        var c = value[i];
        if (char.IsUpper(c) && i > 0)
        {
            result.Append('_');
        }
        result.Append(char.ToUpperInvariant(c));
    }
    return result.ToString();
}

ParsedArgs ParseArgs(IList<string> args)
{
    var result = new ParsedArgs();
    for (var i = 0; i < args.Count; i++)
    {
        var arg = args[i];
        if (arg == "--entity" && i + 1 < args.Count) { result.EntityPath = args[i + 1]; i++; }
        else if (arg == "--module" && i + 1 < args.Count) { result.Module = args[i + 1]; i++; }
        else if (arg == "--enum" && i + 1 < args.Count) { result.EnumPath = args[i + 1]; i++; }
        else if (arg == "--base-path" && i + 1 < args.Count) { result.BasePath = args[i + 1]; i++; }
    }
    return result;
}

void PrintUsage()
{
    Console.WriteLine("用法: dotnet run --file generate_crud.cs -- --entity <路径> --module <模块名> [--enum <枚举路径>] [--base-path <根路径>]");
}

string? ResolveBasePath(string? explicitBasePath)
{
    if (!string.IsNullOrWhiteSpace(explicitBasePath)) return Path.GetFullPath(explicitBasePath);
    var currentDir = Directory.GetCurrentDirectory();
    if (Directory.Exists(Path.Combine(currentDir, "Yi.Abp"))) return currentDir;
    var parentDir = Directory.GetParent(currentDir);
    if (parentDir != null && Directory.Exists(Path.Combine(parentDir.FullName, "Yi.Abp"))) return parentDir.FullName;
    return null;
}

// ==================================== 核心类 ====================================

class EntityInfo
{
    public string EntityName { get; set; } = "";
    public string EntityNameLower { get; set; } = "";   // 驼峰小写：productCategory
    public string EntityNameKebab { get; set; } = "";   // 短横线：product-category
    public string Module { get; set; } = "";
    public string ModuleNamespace { get; set; } = "";
    public string EntityComment { get; set; } = "";
    public bool IsTree { get; set; }
    public bool HasOrderNum { get; set; } = true;
    public bool HasState { get; set; } = true;
    public bool HasAudited { get; set; } = true;
    public bool HasSoftDelete { get; set; } = true;
    public List<FieldInfo> Fields { get; set; } = new();
    public List<string> EnumTypes { get; set; } = new();
}

class FieldInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Comment { get; set; } = "";
    public bool Nullable { get; set; }
    public bool IsIndex { get; set; }      // 索引字段，用于唯一性验证
    public bool IsSearchField { get; set; } // 搜索字段，用于 Contains 查询
    public bool IsTreeField { get; set; }
    public bool IsEnum { get; set; }
    public string? EnumType { get; set; }
}

class EnumInfo
{
    public string EnumName { get; set; } = "";
    public string EnumNameLower { get; set; } = "";
    public List<EnumValueInfo> Values { get; set; } = new();
}

class EnumValueInfo
{
    public string Name { get; set; } = "";
    public int Value { get; set; }
    public string Label { get; set; } = "";
}

class ParsedArgs
{
    public string? EntityPath { get; set; }
    public string? Module { get; set; }
    public string? EnumPath { get; set; }
    public string? BasePath { get; set; }
}
