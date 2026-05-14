#!/usr/bin/env dotnet
#:property PublishAot=false
#:property PackAsTool=false

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

var parsedArgs = ParseArgs(Environment.GetCommandLineArgs().Skip(1).ToList());

if (string.IsNullOrWhiteSpace(parsedArgs.Path))
{
    PrintUsage();
    Environment.Exit(1);
}

var targetPath = Path.GetFullPath(parsedArgs.Path);
if (!File.Exists(targetPath) && !Directory.Exists(targetPath))
{
    Console.WriteLine($"错误: 路径不存在: {targetPath}");
    Environment.Exit(1);
}

var files = ResolveEntityFiles(targetPath).ToList();
if (files.Count == 0)
{
    Console.WriteLine($"未找到实体文件: {targetPath}");
    Environment.Exit(1);
}

var totalErrors = 0;
var totalWarnings = 0;

Console.WriteLine("=== YiABP 实体规范检查 ===");
Console.WriteLine($"扫描路径: {targetPath}");
Console.WriteLine($"实体文件: {files.Count}");
Console.WriteLine();

foreach (var file in files.OrderBy(x => x))
{
    var result = CheckEntityFile(file);
    totalErrors += result.Errors.Count;
    totalWarnings += result.Warnings.Count;

    Console.WriteLine($"[{(result.Errors.Count == 0 ? "OK" : "FAIL")}] {Path.GetFileName(file)}");
    foreach (var error in result.Errors)
    {
        Console.WriteLine($"  ERROR {error}");
    }
    foreach (var warning in result.Warnings)
    {
        Console.WriteLine($"  WARN  {warning}");
    }
    Console.WriteLine();
}

Console.WriteLine("=== 检查完成 ===");
Console.WriteLine($"错误: {totalErrors}");
Console.WriteLine($"警告: {totalWarnings}");

Environment.Exit(totalErrors > 0 ? 1 : 0);

CheckResult CheckEntityFile(string file)
{
    var result = new CheckResult();
    var content = File.ReadAllText(file);
    var fileName = Path.GetFileNameWithoutExtension(file);

    var classMatch = Regex.Match(content, @"public\s+(?:partial\s+)?class\s+(\w+)\s*:\s*([^\r\n\{]+)");
    if (!classMatch.Success)
    {
        result.Errors.Add("未找到 public class 声明或缺少继承声明。");
        return result;
    }

    var className = classMatch.Groups[1].Value;
    var inheritance = classMatch.Groups[2].Value;
    var members = inheritance.Split(',').Select(x => x.Trim()).Where(x => x.Length > 0).ToList();

    if (fileName != className)
    {
        result.Errors.Add($"文件名与类名不一致: 文件为 {fileName}.cs，类为 {className}。");
    }

    var isAggregateRoot = className.EndsWith("AggregateRoot", StringComparison.Ordinal);
    var isEntity = className.EndsWith("Entity", StringComparison.Ordinal);

    if (!isAggregateRoot && !isEntity)
    {
        result.Errors.Add("实体类名必须以 AggregateRoot 或 Entity 结尾。");
    }

    if (isAggregateRoot && !Regex.IsMatch(inheritance, @"\bAggregateRoot\s*<\s*Guid\s*>", RegexOptions.Compiled))
    {
        result.Errors.Add("AggregateRoot 后缀类必须继承 AggregateRoot<Guid>。");
    }

    if (isEntity && !Regex.IsMatch(inheritance, @"\bEntity\s*<\s*Guid\s*>", RegexOptions.Compiled))
    {
        result.Errors.Add("Entity 后缀类必须继承 Entity<Guid>。");
    }

    if (!content.Contains("[SugarTable("))
    {
        result.Errors.Add("缺少 [SugarTable(...)] 表映射。");
    }

    var idPropertyMatch = Regex.Match(content, @"\[SugarColumn\([^\)]*IsPrimaryKey\s*=\s*true[^\)]*\)\]\s*\r?\n\s*public\s+override\s+Guid\s+Id\s*\{\s*get;\s*protected\s+set;\s*\}", RegexOptions.Multiline);
    if (!idPropertyMatch.Success)
    {
        result.Errors.Add("缺少标准 Guid 主键: [SugarColumn(IsPrimaryKey = true)] public override Guid Id { get; protected set; }。");
    }

    var implementedInterfaces = members.Skip(1).ToHashSet(StringComparer.Ordinal);
    CheckInterfaceField(result, content, implementedInterfaces, "ISoftDelete", "bool", "IsDeleted");
    CheckInterfaceField(result, content, implementedInterfaces, "IAuditedObject", "DateTime", "CreationTime");
    CheckInterfaceField(result, content, implementedInterfaces, "IAuditedObject", "Guid?", "CreatorId");
    CheckInterfaceField(result, content, implementedInterfaces, "IAuditedObject", "Guid?", "LastModifierId");
    CheckInterfaceField(result, content, implementedInterfaces, "IAuditedObject", "DateTime?", "LastModificationTime");
    CheckInterfaceField(result, content, implementedInterfaces, "IOrderNum", "int", "OrderNum");
    CheckInterfaceField(result, content, implementedInterfaces, "IState", "bool", "State");

    if (HasProperty(content, "IsDeleted") && !implementedInterfaces.Contains("ISoftDelete"))
    {
        result.Warnings.Add("包含 IsDeleted 字段但未实现 ISoftDelete。");
    }

    if (HasProperty(content, "OrderNum") && !implementedInterfaces.Contains("IOrderNum"))
    {
        result.Warnings.Add("包含 OrderNum 字段但未实现 IOrderNum。");
    }

    if (HasProperty(content, "State") && !implementedInterfaces.Contains("IState"))
    {
        result.Warnings.Add("包含 State 字段但未实现 IState。");
    }

    foreach (var property in FindPublicProperties(content))
    {
        if (!HasXmlSummaryImmediatelyBefore(content, property.Index))
        {
            result.Errors.Add($"public 属性缺少 XML summary: {property.Name}。");
        }

        if (property.Type.EndsWith("Enum", StringComparison.Ordinal) && !property.Type.Contains("?"))
        {
            continue;
        }

        if (property.Name.EndsWith("Enum", StringComparison.Ordinal) && !property.Type.EndsWith("Enum", StringComparison.Ordinal))
        {
            result.Warnings.Add($"字段名像枚举但类型不是 Enum 后缀: {property.Name} ({property.Type})。");
        }
    }

    return result;
}

IEnumerable<string> ResolveEntityFiles(string targetPath)
{
    if (File.Exists(targetPath))
    {
        if (IsEntityFile(targetPath)) yield return targetPath;
        yield break;
    }

    foreach (var file in Directory.EnumerateFiles(targetPath, "*.cs", SearchOption.AllDirectories))
    {
        if (IsEntityFile(file)) yield return file;
    }
}

bool IsEntityFile(string file)
{
    var name = Path.GetFileName(file);
    return name.EndsWith("AggregateRoot.cs", StringComparison.Ordinal) || name.EndsWith("Entity.cs", StringComparison.Ordinal);
}

void CheckInterfaceField(CheckResult result, string content, HashSet<string> implementedInterfaces, string interfaceName, string expectedType, string fieldName)
{
    if (!implementedInterfaces.Contains(interfaceName)) return;

    if (!HasProperty(content, fieldName))
    {
        result.Errors.Add($"实现 {interfaceName} 但缺少字段 {fieldName}。");
        return;
    }

    var property = FindPublicProperties(content).FirstOrDefault(x => x.Name == fieldName);
    if (property is null) return;

    if (NormalizeType(property.Type) != NormalizeType(expectedType))
    {
        result.Errors.Add($"字段 {fieldName} 类型不符合 {interfaceName}: 当前 {property.Type}，期望 {expectedType}。");
    }
}

bool HasProperty(string content, string propertyName)
{
    return Regex.IsMatch(content, $@"public\s+[\w\.<>\?]+\s+{Regex.Escape(propertyName)}\s*\{{", RegexOptions.Multiline);
}

IEnumerable<PropertyInfo> FindPublicProperties(string content)
{
    var regex = new Regex(@"(?:\s*\[[^\]]+\]\s*\r?\n)*\s*public\s+(?:override\s+)?(?<type>[\w\.<>\?]+)\s+(?<name>\w+)\s*\{\s*get;\s*(?:protected\s+)?set;[^\}]*\}", RegexOptions.Multiline);
    foreach (Match match in regex.Matches(content))
    {
        yield return new PropertyInfo(match.Groups["name"].Value, match.Groups["type"].Value, match.Index);
    }
}

bool HasXmlSummaryImmediatelyBefore(string content, int propertyIndex)
{
    var before = content[..propertyIndex];
    var start = Math.Max(0, before.Length - 1200);
    var tail = before[start..];
    return Regex.IsMatch(tail, @"///\s*<summary>[\s\S]*?///\s*</summary>\s*(?:\[[^\]]+\]\s*)*$");
}

string NormalizeType(string type)
{
    return type.Replace(" ", "").Trim();
}

ParsedArgs ParseArgs(List<string> args)
{
    var result = new ParsedArgs();
    for (var i = 0; i < args.Count; i++)
    {
        switch (args[i])
        {
            case "--path" when i + 1 < args.Count:
                result.Path = args[++i];
                break;
            case "--entity" when i + 1 < args.Count:
                result.Path = args[++i];
                break;
        }
    }
    return result;
}

void PrintUsage()
{
    Console.WriteLine("用法:");
    Console.WriteLine("  dotnet run --file .claude/skills/crud-generator-plus/scripts/check_entities.cs -- --path <实体目录或实体文件>");
    Console.WriteLine();
    Console.WriteLine("示例:");
    Console.WriteLine("  dotnet run --file .claude/skills/crud-generator-plus/scripts/check_entities.cs -- --path Yi.Abp/module/media-management/Yi.Module.MediaManagement.Domain/Entities");
}

record ParsedArgs
{
    public string Path { get; set; } = string.Empty;
}

record PropertyInfo(string Name, string Type, int Index);

class CheckResult
{
    public List<string> Errors { get; } = new();
    public List<string> Warnings { get; } = new();
}
