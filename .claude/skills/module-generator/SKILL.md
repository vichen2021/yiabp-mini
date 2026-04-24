---
name: module-generator
description: Generate complete ABP module structure with minimal token overhead. Creates 5-layer projects, updates main module dependencies, solution file, and dynamic API configuration. Use when user wants to create a new module, mentions "新建模块"、"创建模块"、"module"、"add module"、"generate module". Triggers for both simple names (Payment) and compound names (content-management, ContentManagement).
---

# Module Generator

Generates complete ABP module structures by delegating to C# script execution.

## Workflow

### Step 1: Validate Input

Check module name format:
- Accepts PascalCase (e.g., `Payment`, `ContentManagement`)
- Accepts kebab-case (e.g., `payment`, `content-management`)
- Accepts snake_case (converted to PascalCase)

### Step 2: Check Module Exists

Before generating, verify `module/{kebab-name}/` doesn't already exist. If it does, inform the user and stop.

### Step 3: Execute Generation Script

Run the C# file directly:

```bash
dotnet run --file .claude/skills/module-generator/scripts/generate_module.cs -- <module-name>
```

The script handles:
1. Name conversion (PascalCase ↔ kebab-case)
2. Creating 5-layer project structure
3. Generating module classes and csproj files
4. Creating subdirectories (Entities, Dtos, IServices, Services, Repositories)
5. Updating main module DependsOn declarations
6. Adding ProjectReference to main csproj files
7. Updating solution file (.slnx)
8. Configuring dynamic API in YiAbpWebModule.cs

### Step 4: Verify Build

After generation, verify the build succeeds:

```bash
cd Yi.Abp && dotnet build
```

If build fails:
1. Read error messages carefully
2. Fix issues (common: missing references, wrong namespace)
3. Re-run build until successful

### Step 5: Self-Check

Verify the following automatically:

- [ ] Module directory created at `module/{kebab-name}/`
- [ ] 5 project folders exist with correct names
- [ ] Each project has `.csproj` and module class file
- [ ] Subdirectories created (Entities, Dtos, IServices, Services, Repositories)
- [ ] Solution file contains new module folder
- [ ] Build passes without errors

If any check fails, fix immediately before reporting results.

### Step 6: Report Results

Show the user:
- Generated module path
- List of created files (summary, not full paths)
- Build status (passed/failed)
- Next steps for entity/DTO/service creation

## Output Format

After successful generation:

```
Module '{PascalName}' created successfully.

Location: module/{kebab-name}/

Generated:
- 5 project layers (Domain.Shared, Domain, Application.Contracts, Application, SqlSugarCore)
- Module classes with proper DependsOn
- Project files with correct references
- Directory structure (Entities, Dtos, IServices, Services, Repositories)

Updated:
- Main module classes (5 files)
- Main project files (5 files)
- Solution file (Yi.Abp.slnx)
- Dynamic API configuration (YiAbpWebModule.cs)

Next steps:
1. Create entity classes in Domain/Entities/
2. Use /crud-generator-fast to generate full CRUD functionality
```

## Error Handling

| Scenario | Action |
|----------|--------|
| Module exists | Stop, inform user, suggest deleting or using different name |
| C# script execution error | Show error message, suggest checking .NET installation |
| Main module files missing | Script logs warning, continues generation |

## Dry Run Mode

For validation without changes:

```bash
dotnet run --file .claude/skills/module-generator/scripts/generate_module.cs -- <name> --dry-run
```

Useful when user asks "what would be created" or wants to preview changes.

## Examples

| Input | PascalCase | kebab-case | Module Path |
|-------|------------|------------|-------------|
| `Payment` | Payment | payment | `module/payment/` |
| `content-management` | ContentManagement | content-management | `module/content-management/` |
| `ContentManagement` | ContentManagement | content-management | `module/content-management/` |