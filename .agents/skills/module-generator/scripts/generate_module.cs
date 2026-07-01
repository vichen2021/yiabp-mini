#!/usr/bin/env dotnet
#:property PublishAot=false
#:property PackAsTool=false
/*
 * ABP Module Generator - Generate complete 5-layer module structure for YiAbp project.
 *
 * Usage:
 *     dotnet run --file generate_module.cs -- <module-name> [--dry-run] [--base-path <path>]
 *
 * Examples:
 *     dotnet run --file generate_module.cs -- Payment
 *     dotnet run --file generate_module.cs -- content-management
 *     dotnet run --file generate_module.cs -- ContentManagement --dry-run
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

var parsedArgs = ParseArgs(Environment.GetCommandLineArgs().Skip(1).ToList());

if (string.IsNullOrWhiteSpace(parsedArgs.ModuleName))
{
    PrintUsage();
    Environment.Exit(1);
}

var nameInfo = ConvertName(parsedArgs.ModuleName);
Console.WriteLine(
    string.Format(
        "Module name: {0} (PascalCase) / {1} (kebab-case)",
        nameInfo.Pascal,
        nameInfo.Kebab));

var basePath = ResolveBasePath(parsedArgs.BasePath);
if (string.IsNullOrWhiteSpace(basePath))
{
    Console.WriteLine("Error: Could not find YiAbp project root. Use --base-path to specify.");
    Environment.Exit(1);
}

Console.WriteLine("Project root: " + basePath);
Console.WriteLine();

var codeRoot = GetCodeRoot(basePath);
Console.WriteLine("Code root: " + codeRoot);
Console.WriteLine();

var templates = BuildTemplates();
var layers = BuildLayers(nameInfo.Pascal);

Console.WriteLine("=== Step 1: Generating module files ===");
GenerateModuleFiles(codeRoot, nameInfo, parsedArgs.DryRun, templates, layers);
Console.WriteLine();

Console.WriteLine("=== Step 2: Updating main module classes ===");
UpdateMainModuleClasses(codeRoot, nameInfo.Pascal, parsedArgs.DryRun);
Console.WriteLine();

Console.WriteLine("=== Step 3: Updating main project files ===");
UpdateMainProjectFiles(codeRoot, nameInfo, parsedArgs.DryRun);
Console.WriteLine();

Console.WriteLine("=== Step 4: Updating solution file ===");
UpdateSolutionFile(codeRoot, nameInfo, parsedArgs.DryRun);
Console.WriteLine();

Console.WriteLine("=== Step 5: Configuring dynamic API ===");
UpdateDynamicApi(codeRoot, nameInfo, parsedArgs.DryRun);
Console.WriteLine();

if (parsedArgs.DryRun)
{
    Console.WriteLine("=== DRY RUN COMPLETE ===");
    Console.WriteLine("No files were created or modified.");
}
else
{
    Console.WriteLine("=== GENERATION COMPLETE ===");
    Console.WriteLine(
        string.Format(
            "Module '{0}' has been created at: module/{1}/",
            nameInfo.Pascal,
            nameInfo.Kebab));
    Console.WriteLine();
    Console.WriteLine("Next steps:");
    Console.WriteLine("  1. Create entity classes in Domain/Entities/");
    Console.WriteLine("  2. Create DTO classes in Application.Contracts/Dtos/");
    Console.WriteLine("  3. Create service interfaces in Application.Contracts/IServices/");
    Console.WriteLine("  4. Create service implementations in Application/Services/");
    Console.WriteLine("  5. Use /crud-generator-fast to generate full CRUD functionality");
}

ParsedArgs ParseArgs(IList<string> args)
{
    var result = new ParsedArgs();

    for (var i = 0; i < args.Count; i++)
    {
        var arg = args[i];

        if (string.Equals(arg, "--dry-run", StringComparison.OrdinalIgnoreCase))
        {
            result.DryRun = true;
            continue;
        }

        if (string.Equals(arg, "--base-path", StringComparison.OrdinalIgnoreCase))
        {
            if (i + 1 >= args.Count)
            {
                Console.WriteLine("Error: --base-path requires a value.");
                Environment.Exit(1);
            }

            result.BasePath = args[i + 1];
            i++;
            continue;
        }

        if (arg.StartsWith("--", StringComparison.Ordinal))
        {
            Console.WriteLine("Error: Unknown option: " + arg);
            Environment.Exit(1);
        }

        if (string.IsNullOrWhiteSpace(result.ModuleName))
        {
            result.ModuleName = arg;
        }
    }

    return result;
}

void PrintUsage()
{
    Console.WriteLine("Usage: dotnet run --file generate_module.cs -- <module-name> [--dry-run] [--base-path <path>]");
    Console.WriteLine("Examples:");
    Console.WriteLine("    dotnet run --file generate_module.cs -- Payment");
    Console.WriteLine("    dotnet run --file generate_module.cs -- content-management --dry-run");
}

NameInfo ConvertName(string name)
{
    var result = new NameInfo();

    if (name.IndexOf('-') >= 0 || name.IndexOf('_') >= 0)
    {
        var parts = Regex.Split(name, "[-_]").Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
        result.Pascal = string.Concat(parts.Select(ToPascalPart));
        result.Kebab = string.Join("-", parts.Select(p => p.ToLowerInvariant()));
        return result;
    }

    if (char.IsUpper(name[0]))
    {
        result.Pascal = name;
        result.Kebab = Regex.Replace(name, "(?<!^)([A-Z])", "-$1").ToLowerInvariant();
        return result;
    }

    result.Pascal = char.ToUpperInvariant(name[0]) + name.Substring(1);
    result.Kebab = name.ToLowerInvariant();
    return result;
}

string ToPascalPart(string value)
{
    if (string.IsNullOrEmpty(value))
    {
        return string.Empty;
    }

    if (value.Length == 1)
    {
        return char.ToUpperInvariant(value[0]).ToString();
    }

    return char.ToUpperInvariant(value[0]) + value.Substring(1).ToLowerInvariant();
}

string? ResolveBasePath(string? explicitBasePath)
{
    if (!string.IsNullOrWhiteSpace(explicitBasePath))
    {
        return Path.GetFullPath(explicitBasePath);
    }

    var candidateRoots = new List<string>();
    candidateRoots.Add(Directory.GetCurrentDirectory());
    var sourceFilePath = GetSourceFilePath();
    if (!string.IsNullOrWhiteSpace(sourceFilePath))
    {
        var sourceDir = Path.GetDirectoryName(Path.GetFullPath(sourceFilePath));
        if (!string.IsNullOrWhiteSpace(sourceDir))
        {
            candidateRoots.Add(sourceDir);
        }
    }

    foreach (var arg in Environment.GetCommandLineArgs())
    {
        if (!arg.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
        {
            continue;
        }

        try
        {
            var fullPath = Path.GetFullPath(arg);
            if (File.Exists(fullPath))
            {
                var dir = Path.GetDirectoryName(fullPath);
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    candidateRoots.Add(dir);
                }
            }
        }
        catch
        {
        }
    }

    foreach (var candidate in candidateRoots.Distinct(StringComparer.OrdinalIgnoreCase))
    {
        var found = FindProjectRoot(candidate);
        if (!string.IsNullOrWhiteSpace(found))
        {
            return found;
        }
    }

    return null;
}

string GetSourceFilePath([CallerFilePath] string path = "")
{
    return path;
}

string? FindProjectRoot(string startDir)
{
    var dir = new DirectoryInfo(Path.GetFullPath(startDir));

    while (dir != null)
    {
        var yiAbp = Path.Combine(dir.FullName, "Yi.Abp");
        if (Directory.Exists(yiAbp) && File.Exists(Path.Combine(yiAbp, "common.props")))
        {
            return dir.FullName;
        }

        var hasProjectRootMarkers =
            File.Exists(Path.Combine(dir.FullName, "common.props")) ||
            File.Exists(Path.Combine(dir.FullName, "Yi.Abp.slnx"));

        if (hasProjectRootMarkers)
        {
            return dir.Parent != null ? dir.Parent.FullName : dir.FullName;
        }

        dir = dir.Parent;
    }

    return null;
}

string GetCodeRoot(string basePath)
{
    if (Directory.Exists(Path.Combine(basePath, "src")) &&
        Directory.Exists(Path.Combine(basePath, "module")))
    {
        return basePath;
    }

    var yiAbp = Path.Combine(basePath, "Yi.Abp");
    if (Directory.Exists(yiAbp))
    {
        return yiAbp;
    }

    return basePath;
}

Dictionary<string, string> BuildTemplates()
{
    return new Dictionary<string, string>
    {
        {
            "domain_shared_module",
            @"using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Yi.Module.{Pascal}.Domain.Shared
{
    [DependsOn(typeof(AbpDddDomainSharedModule))]
    public class YiModule{Pascal}DomainSharedModule : AbpModule
    {

    }
}
"
        },
        {
            "domain_shared_csproj",
            @"<Project Sdk=""Microsoft.NET.Sdk"">
  <Import Project=""..\..\..\common.props"" />
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Volo.Abp.Ddd.Domain.Shared"" Version=""$(AbpVersion)"" />
  </ItemGroup>
</Project>
"
        },
        {
            "domain_module",
            @"using Volo.Abp.Domain;
using Volo.Abp.Modularity;
using Yi.Module.{Pascal}.Domain.Shared;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.{Pascal}.Domain
{
    [DependsOn(typeof(YiModule{Pascal}DomainSharedModule),
        typeof(AbpDddDomainModule),
        typeof(YiFrameworkSqlSugarCoreAbstractionsModule))]
    public class YiModule{Pascal}DomainModule : AbpModule
    {

    }
}
"
        },
        {
            "domain_csproj",
            @"<Project Sdk=""Microsoft.NET.Sdk"">
  <Import Project=""..\..\..\common.props"" />
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include=""Volo.Abp.Ddd.Domain"" Version=""$(AbpVersion)"" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\..\framework\sqlsugar\Yi.Framework.SqlSugarCore.Abstractions\Yi.Framework.SqlSugarCore.Abstractions.csproj"" />
    <ProjectReference Include=""..\Yi.Module.{Pascal}.Domain.Shared\Yi.Module.{Pascal}.Domain.Shared.csproj"" />
  </ItemGroup>
</Project>
"
        },
        {
            "application_contracts_module",
            @"using Volo.Abp.Modularity;
using Yi.Module.{Pascal}.Domain.Shared;
using Yi.Framework.Ddd.Application.Contracts;

namespace Yi.Module.{Pascal}.Application.Contracts
{
    [DependsOn(typeof(YiModule{Pascal}DomainSharedModule),
        typeof(YiFrameworkDddApplicationContractsModule))]
    public class YiModule{Pascal}ApplicationContractsModule : AbpModule
    {

    }
}
"
        },
        {
            "application_contracts_csproj",
            @"<Project Sdk=""Microsoft.NET.Sdk"">
  <Import Project=""..\..\..\common.props"" />
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\..\framework\ddd\Yi.Framework.Ddd.Application.Contracts\Yi.Framework.Ddd.Application.Contracts.csproj"" />
    <ProjectReference Include=""..\Yi.Module.{Pascal}.Domain.Shared\Yi.Module.{Pascal}.Domain.Shared.csproj"" />
  </ItemGroup>
</Project>
"
        },
        {
            "application_module",
            @"using Volo.Abp.Modularity;
using Yi.Module.{Pascal}.Application.Contracts;
using Yi.Module.{Pascal}.Domain;
using Yi.Framework.Ddd.Application;

namespace Yi.Module.{Pascal}.Application
{
    [DependsOn(typeof(YiModule{Pascal}ApplicationContractsModule),
        typeof(YiModule{Pascal}DomainModule),
        typeof(YiFrameworkDddApplicationModule))]
    public class YiModule{Pascal}ApplicationModule : AbpModule
    {

    }
}
"
        },
        {
            "application_csproj",
            @"<Project Sdk=""Microsoft.NET.Sdk"">
  <Import Project=""..\..\..\common.props"" />
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>

  <ItemGroup>
    <ProjectReference Include=""..\..\..\framework\ddd\Yi.Framework.Ddd.Application\Yi.Framework.Ddd.Application.csproj"" />
    <ProjectReference Include=""..\Yi.Module.{Pascal}.Application.Contracts\Yi.Module.{Pascal}.Application.Contracts.csproj"" />
    <ProjectReference Include=""..\Yi.Module.{Pascal}.Domain\Yi.Module.{Pascal}.Domain.csproj"" />
  </ItemGroup>
</Project>
"
        },
        {
            "sqlsugarcore_module",
            @"using Yi.Framework.SqlSugarCore;
using Volo.Abp.Modularity;

namespace Yi.Module.{Pascal}.SqlSugarCore
{
    [DependsOn(typeof(YiFrameworkSqlSugarCoreModule))]
    public class YiModule{Pascal}SqlSugarCoreModule : AbpModule
    {

    }
}
"
        },
        {
            "sqlsugarcore_csproj",
            @"<Project Sdk=""Microsoft.NET.Sdk"">
  <Import Project=""..\..\..\common.props"" />
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include=""..\..\..\framework\sqlsugar\Yi.Framework.SqlSugarCore\Yi.Framework.SqlSugarCore.csproj"" />
    <ProjectReference Include=""..\Yi.Module.{Pascal}.Domain\Yi.Module.{Pascal}.Domain.csproj"" />
  </ItemGroup>
</Project>
"
        }
    };
}

List<LayerDefinition> BuildLayers(string pascalName)
{
    return new List<LayerDefinition>
    {
        new LayerDefinition
        {
            Name = "Domain.Shared",
            Dirs = new string[0],
            Files = new List<FileDefinition>
            {
                new FileDefinition("YiModule" + pascalName + "DomainSharedModule.cs", "domain_shared_module"),
                new FileDefinition("Yi.Module." + pascalName + ".Domain.Shared.csproj", "domain_shared_csproj")
            }
        },
        new LayerDefinition
        {
            Name = "Domain",
            Dirs = new[] { "Entities" },
            Files = new List<FileDefinition>
            {
                new FileDefinition("YiModule" + pascalName + "DomainModule.cs", "domain_module"),
                new FileDefinition("Yi.Module." + pascalName + ".Domain.csproj", "domain_csproj")
            }
        },
        new LayerDefinition
        {
            Name = "Application.Contracts",
            Dirs = new[] { "Dtos", "IServices" },
            Files = new List<FileDefinition>
            {
                new FileDefinition("YiModule" + pascalName + "ApplicationContractsModule.cs", "application_contracts_module"),
                new FileDefinition("Yi.Module." + pascalName + ".Application.Contracts.csproj", "application_contracts_csproj")
            }
        },
        new LayerDefinition
        {
            Name = "Application",
            Dirs = new[] { "Services" },
            Files = new List<FileDefinition>
            {
                new FileDefinition("YiModule" + pascalName + "ApplicationModule.cs", "application_module"),
                new FileDefinition("Yi.Module." + pascalName + ".Application.csproj", "application_csproj")
            }
        },
        new LayerDefinition
        {
            Name = "SqlSugarCore",
            Dirs = new[] { "Repositories" },
            Files = new List<FileDefinition>
            {
                new FileDefinition("YiModule" + pascalName + "SqlSugarCoreModule.cs", "sqlsugarcore_module"),
                new FileDefinition("Yi.Module." + pascalName + ".SqlSugarCore.csproj", "sqlsugarcore_csproj")
            }
        }
    };
}

void GenerateModuleFiles(
    string codeRoot,
    NameInfo nameInfo,
    bool dryRun,
    Dictionary<string, string> templates,
    List<LayerDefinition> layers)
{
    var modulePath = Path.Combine(codeRoot, "module", nameInfo.Kebab);

    if (Directory.Exists(modulePath))
    {
        Console.WriteLine("Error: Module directory already exists: " + modulePath);
        Environment.Exit(1);
    }

    if (dryRun)
    {
        Console.WriteLine("[DRY RUN] Would create module at: " + modulePath);
        return;
    }

    Directory.CreateDirectory(modulePath);

    foreach (var layer in layers)
    {
        var projectName = "Yi.Module." + nameInfo.Pascal + "." + layer.Name;
        var layerPath = Path.Combine(modulePath, projectName);
        Directory.CreateDirectory(layerPath);

        foreach (var dir in layer.Dirs)
        {
            Directory.CreateDirectory(Path.Combine(layerPath, dir));
        }

        foreach (var file in layer.Files)
        {
            var filePath = Path.Combine(layerPath, file.FileName);
            var content = RenderTemplate(templates[file.TemplateKey], nameInfo);
            File.WriteAllText(filePath, content, new UTF8Encoding(false));
            Console.WriteLine("  Created: " + Path.GetRelativePath(codeRoot, filePath));
        }
    }
}

string RenderTemplate(string template, NameInfo nameInfo)
{
    return template
        .Replace("{Pascal}", nameInfo.Pascal)
        .Replace("{Kebab}", nameInfo.Kebab);
}

void UpdateMainModuleClasses(string codeRoot, string pascalName, bool dryRun)
{
    var mainModules = new Dictionary<string, ModuleTarget>
    {
        {
            "src/Yi.Abp.Domain.Shared/YiAbpDomainSharedModule.cs",
            new ModuleTarget("YiModule" + pascalName + "DomainSharedModule", "Domain.Shared")
        },
        {
            "src/Yi.Abp.Domain/YiAbpDomainModule.cs",
            new ModuleTarget("YiModule" + pascalName + "DomainModule", "Domain")
        },
        {
            "src/Yi.Abp.Application.Contracts/YiAbpApplicationContractsModule.cs",
            new ModuleTarget("YiModule" + pascalName + "ApplicationContractsModule", "Application.Contracts")
        },
        {
            "src/Yi.Abp.Application/YiAbpApplicationModule.cs",
            new ModuleTarget("YiModule" + pascalName + "ApplicationModule", "Application")
        },
        {
            "src/Yi.Abp.SqlSugarCore/YiAbpSqlSugarCoreModule.cs",
            new ModuleTarget("YiModule" + pascalName + "SqlSugarCoreModule", "SqlSugarCore")
        }
    };

    foreach (var entry in mainModules)
    {
        var relPath = entry.Key;
        var filePath = Path.Combine(codeRoot, relPath);

        if (!File.Exists(filePath))
        {
            Console.WriteLine("  Warning: Main module file not found: " + relPath);
            continue;
        }

        if (dryRun)
        {
            Console.WriteLine("[DRY RUN] Would update: " + relPath);
            continue;
        }

        var content = File.ReadAllText(filePath);
        var usingStatement = "using Yi.Module." + pascalName + "." + entry.Value.LayerName + ";";

        if (content.Contains(usingStatement))
        {
            Console.WriteLine("  Skip (already exists): " + relPath);
            continue;
        }

        var lines = content.Split('\n').ToList();
        var usingLines = lines
            .Select((line, index) => new { line, index })
            .Where(x => x.line.StartsWith("using ") && !x.line.StartsWith("using System"))
            .Select(x => x.index)
            .ToList();

        if (usingLines.Count > 0)
        {
            var insertIndex = usingLines[usingLines.Count - 1] + 1;
            for (var i = 0; i < usingLines.Count; i++)
            {
                var currentLine = lines[usingLines[i]];
                if (string.CompareOrdinal(currentLine, usingStatement) > 0)
                {
                    insertIndex = usingLines[i];
                    break;
                }
            }

            lines.Insert(insertIndex, usingStatement);
        }
        else
        {
            var namespaceIndex = lines.FindIndex(line => line.StartsWith("namespace "));
            if (namespaceIndex >= 0)
            {
                lines.Insert(namespaceIndex, usingStatement);
            }
        }

        content = string.Join("\n", lines);

        var dependsMatch = Regex.Match(content, @"\[DependsOn\(([\s\S]*?)\)\]");
        if (dependsMatch.Success)
        {
            var existingDeps = dependsMatch.Groups[1].Value;
            var dependency = "typeof(" + entry.Value.ModuleClass + ")";

            if (existingDeps.Contains(dependency))
            {
                Console.WriteLine("  Skip (dependency exists): " + relPath);
                continue;
            }

            var newDeps = existingDeps.TrimEnd() + ",\n        " + dependency;
            content = content.Replace(existingDeps, newDeps);
        }

        File.WriteAllText(filePath, content, new UTF8Encoding(false));
        Console.WriteLine("  Updated: " + relPath);
    }
}

void UpdateMainProjectFiles(string codeRoot, NameInfo nameInfo, bool dryRun)
{
    var mainProjects = new Dictionary<string, string>
    {
        { "src/Yi.Abp.Domain.Shared/Yi.Abp.Domain.Shared.csproj", "Domain.Shared" },
        { "src/Yi.Abp.Domain/Yi.Abp.Domain.csproj", "Domain" },
        { "src/Yi.Abp.Application.Contracts/Yi.Abp.Application.Contracts.csproj", "Application.Contracts" },
        { "src/Yi.Abp.Application/Yi.Abp.Application.csproj", "Application" },
        { "src/Yi.Abp.SqlSugarCore/Yi.Abp.SqlSugarCore.csproj", "SqlSugarCore" }
    };

    // Standard XML indentation: ItemGroup uses 2 spaces, child elements use 4 spaces
    const string itemGroupIndent = "  ";    // 2 spaces
    const string elementIndent = "    ";    // 4 spaces

    foreach (var entry in mainProjects)
    {
        var relPath = entry.Key;
        var layerName = entry.Value;
        var filePath = Path.Combine(codeRoot, relPath);

        if (!File.Exists(filePath))
        {
            Console.WriteLine("  Warning: Main project file not found: " + relPath);
            continue;
        }

        if (dryRun)
        {
            Console.WriteLine("[DRY RUN] Would update: " + relPath);
            continue;
        }

        var content = File.ReadAllText(filePath);
        var projectRef =
            "<ProjectReference Include=\"..\\..\\module\\" +
            nameInfo.Kebab +
            "\\Yi.Module." +
            nameInfo.Pascal +
            "." +
            layerName +
            "\\Yi.Module." +
            nameInfo.Pascal +
            "." +
            layerName +
            ".csproj\" />";

        if (content.Contains(projectRef))
        {
            Console.WriteLine("  Skip (already exists): " + relPath);
            continue;
        }

        // Find existing ItemGroup containing ProjectReference (not PackageReference)
        // Match ItemGroup that has at least one ProjectReference element
        var itemGroupMatch = Regex.Match(
            content,
            @"<ItemGroup>\s*(?:\s*<ProjectReference[^>]*/>\s*)+\s*</ItemGroup>");

        if (itemGroupMatch.Success)
        {
            // Find the closing </ItemGroup> tag position within this match
            var itemGroupEnd = itemGroupMatch.Value.LastIndexOf("</ItemGroup>");
            if (itemGroupEnd >= 0)
            {
                // Convert relative position to absolute position in content
                itemGroupEnd += itemGroupMatch.Index;
                // Insert with standard 4-space indent, followed by newline
                content = content.Insert(itemGroupEnd, elementIndent + projectRef + "\n");
            }
        }
        else
        {
            // No existing ProjectReference ItemGroup, create a new one with standard indentation
            var newItemGroup =
                itemGroupIndent + "<ItemGroup>\n" +
                elementIndent + projectRef + "\n" +
                itemGroupIndent + "</ItemGroup>\n\n" +
                "</Project>";
            content = content.Replace("</Project>", newItemGroup);
        }

        File.WriteAllText(filePath, content, new UTF8Encoding(false));
        Console.WriteLine("  Updated: " + relPath);
    }
}

void UpdateSolutionFile(string codeRoot, NameInfo nameInfo, bool dryRun)
{
    var slnxPath = Path.Combine(codeRoot, "Yi.Abp.slnx");
    if (!File.Exists(slnxPath))
    {
        Console.WriteLine("  Warning: Solution file not found: Yi.Abp.slnx");
        return;
    }

    if (dryRun)
    {
        Console.WriteLine("[DRY RUN] Would update: Yi.Abp.slnx");
        return;
    }

    var content = File.ReadAllText(slnxPath);
    var folderName = "/module/" + nameInfo.Kebab + "/";

    if (content.Contains(folderName))
    {
        Console.WriteLine("  Skip (folder exists in solution): " + folderName);
        return;
    }

    var folderXml =
        "<Folder Name=\"/module/" + nameInfo.Kebab + "/\">\n" +
        "  <Project Path=\"module/" + nameInfo.Kebab + "/Yi.Module." + nameInfo.Pascal + ".Application.Contracts/Yi.Module." + nameInfo.Pascal + ".Application.Contracts.csproj\" />\n" +
        "  <Project Path=\"module/" + nameInfo.Kebab + "/Yi.Module." + nameInfo.Pascal + ".Application/Yi.Module." + nameInfo.Pascal + ".Application.csproj\" />\n" +
        "  <Project Path=\"module/" + nameInfo.Kebab + "/Yi.Module." + nameInfo.Pascal + ".Domain.Shared/Yi.Module." + nameInfo.Pascal + ".Domain.Shared.csproj\" />\n" +
        "  <Project Path=\"module/" + nameInfo.Kebab + "/Yi.Module." + nameInfo.Pascal + ".Domain/Yi.Module." + nameInfo.Pascal + ".Domain.csproj\" />\n" +
        "  <Project Path=\"module/" + nameInfo.Kebab + "/Yi.Module." + nameInfo.Pascal + ".SqlSugarCore/Yi.Module." + nameInfo.Pascal + ".SqlSugarCore.csproj\" />\n" +
        "</Folder>\n";

    var moduleFolderMatches = Regex.Matches(content, "<Folder Name=\"/module/[^\"]+/\">");
    if (moduleFolderMatches.Count > 0)
    {
        var lastMatch = moduleFolderMatches[moduleFolderMatches.Count - 1];
        var searchStart = lastMatch.Index;
        var depth = 1;
        var nextFolderPos = content.IndexOf("<Folder", searchStart + 1, StringComparison.Ordinal);
        var endPos = content.IndexOf("</Folder>", searchStart + 1, StringComparison.Ordinal);

        while (depth > 0 && endPos >= 0)
        {
            if (nextFolderPos >= 0 && nextFolderPos < endPos)
            {
                depth++;
                nextFolderPos = content.IndexOf("<Folder", nextFolderPos + 1, StringComparison.Ordinal);
            }
            else
            {
                depth--;
                if (depth > 0)
                {
                    endPos = content.IndexOf("</Folder>", endPos + 1, StringComparison.Ordinal);
                }
            }
        }

        if (endPos >= 0)
        {
            var insertPos = endPos + "</Folder>".Length;
            content = content.Insert(insertPos, "\n" + folderXml);
        }
    }
    else
    {
        var parentMatch = Regex.Match(content, "<Folder Name=\"/module/\">");
        if (parentMatch.Success)
        {
            var endPos = content.IndexOf("</Folder>", parentMatch.Index + 1, StringComparison.Ordinal);
            if (endPos >= 0)
            {
                var insertPos = endPos + "</Folder>".Length;
                content = content.Insert(insertPos, "\n" + folderXml);
            }
        }
        else
        {
            content = content.Replace("</Solution>", folderXml + "</Solution>");
        }
    }

    File.WriteAllText(slnxPath, content, new UTF8Encoding(false));
    Console.WriteLine("  Updated: Yi.Abp.slnx");
}

void UpdateDynamicApi(string codeRoot, NameInfo nameInfo, bool dryRun)
{
    var webModulePath = Path.Combine(codeRoot, "src/Yi.Abp.Web/YiAbpWebModule.cs");
    if (!File.Exists(webModulePath))
    {
        Console.WriteLine("  Warning: YiAbpWebModule.cs not found");
        return;
    }

    if (dryRun)
    {
        Console.WriteLine("[DRY RUN] Would update: YiAbpWebModule.cs");
        return;
    }

    var content = File.ReadAllText(webModulePath);
    var usingStatement = "using Yi.Module." + nameInfo.Pascal + ".Application;";

    if (content.Contains(usingStatement))
    {
        Console.WriteLine("  Skip (using exists): YiAbpWebModule.cs");
    }
    else
    {
        var lines = content.Split('\n').ToList();
        var usingLines = lines
            .Select((line, index) => new { line, index })
            .Where(x => x.line.StartsWith("using Yi.Framework.") || x.line.StartsWith("using Yi.Module."))
            .Select(x => x.index)
            .ToList();

        if (usingLines.Count > 0)
        {
            var insertIndex = usingLines[usingLines.Count - 1] + 1;
            for (var i = 0; i < usingLines.Count; i++)
            {
                var currentLine = lines[usingLines[i]];
                if (string.CompareOrdinal(currentLine, usingStatement) > 0)
                {
                    insertIndex = usingLines[i];
                    break;
                }
            }

            lines.Insert(insertIndex, usingStatement);
            content = string.Join("\n", lines);
        }
    }

    var apiConfigBase =
        "options.ConventionalControllers.Create(typeof(YiModule" +
        nameInfo.Pascal +
        "ApplicationModule).Assembly,";
    var apiConfigLambda =
        "options => options.RemoteServiceName = \"" +
        nameInfo.Kebab +
        "\");";

    // Check if already exists (flexible match for different indent styles)
    if (content.Contains(apiConfigBase) && Regex.IsMatch(content, Regex.Escape(apiConfigLambda)))
    {
        Console.WriteLine("  Skip (API config exists): YiAbpWebModule.cs");
    }
    else
    {
        var preconfigureMatch = Regex.Match(
            content,
            @"PreConfigure<AbpAspNetCoreMvcOptions>\s*\(\s*options\s*=>\s*\{");

        if (preconfigureMatch.Success)
        {
            var unifiedMatch = Regex.Match(content, @"//统一前缀");
            if (unifiedMatch.Success)
            {
                // Standard indent: 16 spaces for first line, 20 spaces for second line
                const string indentLine1 = "                ";  // 16 spaces
                const string indentLine2 = "                    ";  // 20 spaces

                // Find all existing ConventionalControllers.Create calls BEFORE unifiedMatch
                var searchContent = content.Substring(0, unifiedMatch.Index);
                var createMatches = Regex.Matches(
                    searchContent,
                    @"options\.ConventionalControllers\.Create[\s\S]*?\);");

                // Determine insertion position based on alphabetical order
                int lastPrecedingEndPos = -1;
                int insertBeforePos = -1;

                foreach (Match match in createMatches)
                {
                    var existingContent = match.Value;
                    var remoteNameMatch = Regex.Match(
                        existingContent,
                        "RemoteServiceName\\s*=\\s*\"([^\"]+)\"");

                    if (remoteNameMatch.Success)
                    {
                        var existingName = remoteNameMatch.Groups[1].Value;

                        // Compare: if new module name comes BEFORE existing name alphabetically
                        if (string.CompareOrdinal(nameInfo.Kebab, existingName) < 0)
                        {
                            // Insert BEFORE this existing call
                            insertBeforePos = match.Index;
                            break;
                        }
                        else
                        {
                            // new module comes AFTER this existing call
                            lastPrecedingEndPos = match.Index + match.Length;
                        }
                    }
                }

                // Build formatted config
                var formattedConfig =
                    indentLine1 + "options.ConventionalControllers.Create(typeof(YiModule" + nameInfo.Pascal + "ApplicationModule).Assembly," + "\n" +
                    indentLine2 + "options => options.RemoteServiceName = \"" + nameInfo.Kebab + "\");" + "\n\n";

                if (insertBeforePos >= 0)
                {
                    // Insert BEFORE an existing call
                    // Find the beginning of the line (after the previous newline)
                    int lineStartPos = insertBeforePos;
                    while (lineStartPos > 0 && content[lineStartPos - 1] != '\n')
                    {
                        lineStartPos--;
                    }
                    content = content.Insert(lineStartPos, formattedConfig);
                }
                else if (lastPrecedingEndPos >= 0)
                {
                    // Insert AFTER all preceding calls, just before "//统一前缀"
                    int insertPos = lastPrecedingEndPos;

                    // Skip any whitespace (spaces) after ");"
                    while (insertPos < unifiedMatch.Index && content[insertPos] == ' ')
                    {
                        insertPos++;
                    }

                    // Move past the newline
                    if (insertPos < unifiedMatch.Index && content[insertPos] == '\n')
                    {
                        insertPos++;
                    }

                    // Insert with a leading newline to separate from previous call
                    var configWithLeadingNewline = "\n" + formattedConfig;
                    content = content.Insert(insertPos, configWithLeadingNewline);
                }
                else
                {
                    // No existing calls, insert before "//统一前缀"
                    content = content.Insert(unifiedMatch.Index, formattedConfig);
                }
            }
            else
            {
                Console.WriteLine("  Warning: Could not find '//统一前缀' comment marker");
            }
        }
    }

    File.WriteAllText(webModulePath, content, new UTF8Encoding(false));
    Console.WriteLine("  Updated: YiAbpWebModule.cs");
}

class ParsedArgs
{
    public string ModuleName { get; set; } = string.Empty;
    public bool DryRun { get; set; }
    public string? BasePath { get; set; }
}

class NameInfo
{
    public string Pascal { get; set; } = string.Empty;
    public string Kebab { get; set; } = string.Empty;
}

class FileDefinition
{
    public FileDefinition(string fileName, string templateKey)
    {
        FileName = fileName;
        TemplateKey = templateKey;
    }

    public string FileName { get; private set; }
    public string TemplateKey { get; private set; }
}

class LayerDefinition
{
    public string Name { get; set; } = string.Empty;
    public string[] Dirs { get; set; } = Array.Empty<string>();
    public List<FileDefinition> Files { get; set; } = new List<FileDefinition>();
}

class ModuleTarget
{
    public ModuleTarget(string moduleClass, string layerName)
    {
        ModuleClass = moduleClass;
        LayerName = layerName;
    }

    public string ModuleClass { get; private set; }
    public string LayerName { get; private set; }
}
