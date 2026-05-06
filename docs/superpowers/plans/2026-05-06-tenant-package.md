# 租户套餐模块实施计划

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** 在 tenant-management 模块中新增租户套餐功能，实现套餐 CRUD、菜单关联、租户套餐同步。

**Architecture:** 新增 TenantPackageAggregateRoot + TenantPackageMenuEntity 关联表，扩展 TenantService 和 MenuService，取消注释前端套餐相关代码。

**Tech Stack:** .NET 10 + ABP 10 + SqlSugar + Vue 3 + Vben Admin

---

## 文件结构

### 新增文件

```
Yi.Abp/module/tenant-management/
├── Yi.Module.TenantManagement.Domain/
│   └── Entities/
│       ├── TenantPackageAggregateRoot.cs
│       └── TenantPackageMenuEntity.cs
├── Yi.Module.TenantManagement.Application.Contracts/
│   ├── IServices/
│   │   └── ITenantPackageService.cs
│   └── Dtos/TenantPackage/
│       ├── TenantPackageGetOutputDto.cs
│       ├── TenantPackageGetListOutputDto.cs
│       ├── TenantPackageGetListInputVo.cs
│       ├── TenantPackageCreateInputVo.cs
│       └── TenantPackageUpdateInputVo.cs
├── Yi.Module.TenantManagement.Application/
│   └── Services/
│       └── TenantPackageService.cs
```

### 修改文件

```
Yi.Abp/module/tenant-management/
├── Yi.Module.TenantManagement.Domain/
│   └── TenantAggregateRoot.cs
├── Yi.Module.TenantManagement.Application/
│   └── TenantService.cs
└── Yi.Module.Rbac.Application/
    └── Services/System/MenuService.cs

Yi.Vben5/apps/web-antd/src/views/system/tenant/
├── tenant-drawer.vue
├── data.tsx
└── index.vue
```

---

## Task 1: 创建 TenantPackageAggregateRoot 实体

**Files:**
- Create: `Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Domain/Entities/TenantPackageAggregateRoot.cs`

- [ ] **Step 1: 创建实体类文件**

```csharp
using SqlSugar;
using Volo.Abp.Auditing;
using Volo.Abp.Domain.Entities;
using Yi.Framework.Core.Data;

namespace Yi.Module.TenantManagement.Domain.Entities;

/// <summary>
/// 租户套餐
/// </summary>
[SugarTable("TenantPackage")]
[SugarIndex($"index_{nameof(PackageName)}", nameof(PackageName), OrderByType.Asc)]
public class TenantPackageAggregateRoot : AggregateRoot<Guid>, ISoftDelete, IAuditedObject, IOrderNum, IState
{
    [SugarColumn(IsPrimaryKey = true)]
    public override Guid Id { get; protected set; }

    /// <summary>
    /// 套餐名称
    /// </summary>
    [SugarColumn(Length = 50)]
    public string PackageName { get; set; } = string.Empty;

    /// <summary>
    /// 菜单树选择是否父子关联
    /// </summary>
    public bool MenuCheckStrictly { get; set; } = true;

    /// <summary>
    /// 状态
    /// </summary>
    public bool State { get; set; } = true;

    /// <summary>
    /// 排序号
    /// </summary>
    public int OrderNum { get; set; } = 0;

    /// <summary>
    /// 备注
    /// </summary>
    [SugarColumn(Length = 500, IsNullable = true)]
    public string? Remark { get; set; }

    /// <summary>
    /// 软删除
    /// </summary>
    public bool IsDeleted { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTime CreationTime { get; set; } = DateTime.Now;

    /// <summary>
    /// 创建者ID
    /// </summary>
    public Guid? CreatorId { get; set; }

    /// <summary>
    /// 最后修改者ID
    /// </summary>
    public Guid? LastModifierId { get; set; }

    /// <summary>
    /// 最后修改时间
    /// </summary>
    public DateTime? LastModificationTime { get; set; }

    /// <summary>
    /// 套餐关联的菜单（导航属性）
    /// </summary>
    [Navigate(typeof(TenantPackageMenuEntity), nameof(TenantPackageMenuEntity.PackageId), nameof(TenantPackageMenuEntity.MenuId))]
    public List<MenuAggregateRoot>? Menus { get; set; }
}
```

- [ ] **Step 2: 验证实体类编译**

Run: `dotnet build Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Domain/Yi.Module.TenantManagement.Domain.csproj --no-restore`

Expected: Build succeeded

---

## Task 2: 创建 TenantPackageMenuEntity 关联表实体

**Files:**
- Create: `Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Domain/Entities/TenantPackageMenuEntity.cs`

- [ ] **Step 1: 创建关联表实体类文件**

```csharp
using SqlSugar;
using Volo.Abp.Domain.Entities;

namespace Yi.Module.TenantManagement.Domain.Entities;

/// <summary>
/// 租户套餐菜单关系表
/// </summary>
[SugarTable("TenantPackageMenu")]
public class TenantPackageMenuEntity : Entity<Guid>
{
    [SugarColumn(IsPrimaryKey = true)]
    public override Guid Id { get; protected set; }

    /// <summary>
    /// 套餐ID
    /// </summary>
    public Guid PackageId { get; set; }

    /// <summary>
    /// 菜单ID
    /// </summary>
    public Guid MenuId { get; set; }
}
```

- [ ] **Step 2: 验证编译**

Run: `dotnet build Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Domain/Yi.Module.TenantManagement.Domain.csproj --no-restore`

Expected: Build succeeded

---

## Task 3: 修改 TenantAggregateRoot 新增 PackageId 字段

**Files:**
- Modify: `Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Domain/TenantAggregateRoot.cs`

- [ ] **Step 1: 在 TenantAggregateRoot 中添加 PackageId 字段**

在 `TenantAggregateRoot.cs` 的字段区域添加：

```csharp
/// <summary>
/// 关联套餐ID
/// </summary>
public Guid? PackageId { get; set; }
```

- [ ] **Step 2: 验证编译**

Run: `dotnet build Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Domain/Yi.Module.TenantManagement.Domain.csproj --no-restore`

Expected: Build succeeded

---

## Task 4: 使用 crud-generator-plus 生成基础 CRUD

**Files:**
- Generated by script

- [ ] **Step 1: 调用脚本生成 CRUD**

Run: `dotnet run --file .claude/skills/crud-generator-plus/scripts/generate_crud.cs -- --entity "Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Domain/Entities/TenantPackageAggregateRoot.cs" --module "tenant-management"`

Expected: 生成 12 个文件（后端 7 + 前端 5 + 种子数据 Agent）

---

## Task 5: 补充 TenantPackageService 的 MenuIds 处理逻辑

**Files:**
- Modify: `Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Application/TenantPackageService.cs`

- [ ] **Step 1: 在 CreateAsync 中处理 MenuIds 关联表**

在 `TenantPackageService.CreateAsync` 方法末尾添加：

```csharp
// 处理套餐菜单关联
if (input.MenuIds != null && input.MenuIds.Count > 0)
{
    var packageMenus = input.MenuIds.Select(menuId => new TenantPackageMenuEntity
    {
        PackageId = entity.Id,
        MenuId = menuId
    }).ToList();
    await _tenantPackageMenuRepository.InsertRangeAsync(packageMenus);
}
```

需要注入 `_tenantPackageMenuRepository`：

```csharp
private readonly ISqlSugarRepository<TenantPackageMenuEntity> _tenantPackageMenuRepository;
```

- [ ] **Step 2: 在 UpdateAsync 中处理 MenuIds 关联表**

在 `TenantPackageService.UpdateAsync` 方法中，更新实体后添加：

```csharp
// 先删除旧的关联
await _tenantPackageMenuRepository.DeleteAsync(x => x.PackageId == id);

// 重新插入新的关联
if (input.MenuIds != null && input.MenuIds.Count > 0)
{
    var packageMenus = input.MenuIds.Select(menuId => new TenantPackageMenuEntity
    {
        PackageId = id,
        MenuId = menuId
    }).ToList();
    await _tenantPackageMenuRepository.InsertRangeAsync(packageMenus);
}
```

- [ ] **Step 3: 在 GetAsync/GetListAsync 中填充 MenuIds**

在输出 DTO 映射后，查询关联表填充 `MenuIds`：

```csharp
// 查询套餐关联的菜单ID
var menuIds = await _tenantPackageMenuRepository._DbQueryable
    .Where(x => x.PackageId == entity.Id)
    .Select(x => x.MenuId)
    .ToListAsync();

dto.MenuIds = menuIds;
```

---

## Task 6: 添加 TenantPackageService 删除保护逻辑

**Files:**
- Modify: `Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Application/TenantPackageService.cs`

- [ ] **Step 1: 覆盖 DeleteAsync 方法添加保护逻辑**

```csharp
public override async Task DeleteAsync(IEnumerable<Guid> ids)
{
    // 检查是否有租户引用该套餐
    var hasReference = await _repository._DbQueryable
        .Where(x => ids.Contains(x.PackageId.Value))
        .AnyAsync();

    if (hasReference)
    {
        throw new UserFriendlyException("租户套餐已被使用，无法删除");
    }

    // 删除套餐关联的菜单
    await _tenantPackageMenuRepository.DeleteAsync(x => ids.Contains(x.PackageId));

    // 删除套餐
    await base.DeleteAsync(ids);
}
```

需要注入 `TenantAggregateRoot` 仓储：

```csharp
private readonly ISqlSugarRepository<TenantAggregateRoot, Guid> _tenantRepository;
```

---

## Task 7: 添加 TenantPackageService 菜单树查询方法

**Files:**
- Modify: `Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Application.Contracts/ITenantPackageService.cs`
- Modify: `Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Application/TenantPackageService.cs`

- [ ] **Step 1: 在接口中定义方法**

```csharp
/// <summary>
/// 获取套餐菜单树
/// </summary>
/// <param name="packageId">套餐ID，0 表示新增模式</param>
Task<MenuTreeResultDto> GetMenuTreeAsync(Guid packageId);
```

需要定义 DTO：

```csharp
public class MenuTreeResultDto
{
    public List<Guid> CheckedKeys { get; set; } = new();
    public List<MenuTreeDto> Menus { get; set; } = new();
}
```

- [ ] **Step 2: 在服务中实现方法**

```csharp
public async Task<MenuTreeResultDto> GetMenuTreeAsync(Guid packageId)
{
    var result = new MenuTreeResultDto();

    // 查询全部菜单（排除租户管理菜单）
    var menus = await _menuRepository._DbQueryable
        .Where(x => x.MenuName != "租户管理")
        .ToListAsync();

    result.Menus = menus.TreeDtoBuild();

    // 如果是编辑模式，查询套餐已选的菜单
    if (packageId != Guid.Empty)
    {
        var checkedKeys = await _tenantPackageMenuRepository._DbQueryable
            .Where(x => x.PackageId == packageId)
            .Select(x => x.MenuId)
            .ToListAsync();
        result.CheckedKeys = checkedKeys;
    }

    return result;
}
```

---

## Task 8: 添加 TenantService.SyncPackageAsync 方法

**Files:**
- Modify: `Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Application.Contracts/ITenantService.cs`
- Modify: `Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Application/TenantService.cs`

- [ ] **Step 1: 在接口中定义方法**

```csharp
/// <summary>
/// 同步套餐菜单到租户
/// </summary>
Task SyncPackageAsync(Guid tenantId, Guid packageId);
```

- [ ] **Step 2: 在服务中实现方法**

```csharp
public async Task SyncPackageAsync(Guid tenantId, Guid packageId)
{
    // 查询套餐关联的菜单ID
    var packageMenuIds = await _tenantPackageMenuRepository._DbQueryable
        .Where(x => x.PackageId == packageId)
        .Select(x => x.MenuId)
        .ToListAsync();

    // 查询租户下的所有角色
    var roles = await _roleRepository._DbQueryable
        .Where(x => x.TenantId == tenantId)
        .ToListAsync();

    foreach (var role in roles)
    {
        // 管理员角色：全量替换
        if (role.RoleCode == "admin")
        {
            // 删除该角色的所有菜单关联
            await _roleMenuRepository.DeleteAsync(x => x.RoleId == role.Id);

            // 插入套餐的菜单
            var roleMenus = packageMenuIds.Select(menuId => new RoleMenuEntity
            {
                RoleId = role.Id,
                MenuId = menuId
            }).ToList();
            await _roleMenuRepository.InsertRangeAsync(roleMenus);
        }
        else
        {
            // 其他角色：裁剪不在套餐中的菜单
            await _roleMenuRepository._Db.Deleteable<RoleMenuEntity>()
                .Where(x => x.RoleId == role.Id)
                .Where(x => !packageMenuIds.Contains(x.MenuId))
                .ExecuteCommandAsync();
        }
    }
}
```

需要注入：
- `_tenantPackageMenuRepository`（ISqlSugarRepository<TenantPackageMenuEntity>）
- `_roleRepository`（ISqlSugarRepository<RoleAggregateRoot, Guid>）
- `_roleMenuRepository`（ISqlSugarRepository<RoleMenuEntity>）

---

## Task 9: 添加 MenuService.GetTenantPackageMenuTreeAsync 方法

**Files:**
- Modify: `Yi.Abp/module/rbac/Yi.Module.Rbac.Application.Contracts/IServices/IMenuService.cs`
- Modify: `Yi.Abp/module/rbac/Yi.Module.Rbac.Application/Services/System/MenuService.cs`

- [ ] **Step 1: 在接口中定义方法**

```csharp
/// <summary>
/// 获取租户套餐菜单树
/// </summary>
/// <param name="packageId">套餐ID，0 表示新增模式</param>
Task<ActionResult> GetTenantPackageMenuTreeAsync(Guid packageId);
```

- [ ] **Step 2: 在服务中实现方法**

```csharp
[Route("menu/tenantPackageMenuTreeselect/{packageId}")]
public async Task<ActionResult> GetTenantPackageMenuTreeAsync(Guid packageId)
{
    // 查询全部菜单（排除租户管理菜单）
    var menus = await _repository._DbQueryable
        .Where(x => x.MenuName != "租户管理")
        .ToListAsync();

    var menuTrees = menus.TreeDtoBuild();

    List<Guid> checkedKeys = new();

    // 如果是编辑模式，查询套餐已选的菜单
    if (packageId != Guid.Empty)
    {
        checkedKeys = await _tenantPackageMenuRepository._DbQueryable
            .Where(x => x.PackageId == packageId)
            .Select(x => x.MenuId)
            .ToListAsync();
    }

    return new JsonResult(new
    {
        checkedKeys,
        menus = menuTrees
    });
}
```

需要注入 `_tenantPackageMenuRepository`（跨模块引用，需要引用 tenant-management 的 Application.Contracts）。

---

## Task 10: 前端取消注释套餐相关代码

**Files:**
- Modify: `Yi.Vben5/apps/web-antd/src/views/system/tenant/tenant-drawer.vue`
- Modify: `Yi.Vben5/apps/web-antd/src/views/system/tenant/data.tsx`
- Modify: `Yi.Vben5/apps/web-antd/src/views/system/tenant/index.vue`

- [ ] **Step 1: 取消注释 tenant-drawer.vue**

找到被注释的代码块，删除注释符号：
- `packageSelectList` import
- `setupPackageSelect` 函数
- `packageId` 相关字段

- [ ] **Step 2: 取消注释 data.tsx**

取消注释：
- `packageId` 列定义
- `packageId` drawer schema

- [ ] **Step 3: 取消注释 index.vue**

取消注释：
- `tenantSyncPackage` import
- `handleSync` 函数
- 同步按钮

---

## Task 11: 构建验证

- [ ] **Step 1: 后端构建**

Run: `dotnet build Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Application/Yi.Module.TenantManagement.Application.csproj --no-restore`

Expected: Build succeeded

- [ ] **Step 2: 后端整体构建**

Run: `dotnet build Yi.Abp/Yi.Abp.slnx`

Expected: Build succeeded

- [ ] **Step 3: 前端类型检查**

Run: `cd Yi.Vben5 && pnpm run check:type --filter="@vben/web-antd"`

Expected: 无 TypeScript 错误

---

## Task 12: 提交

- [ ] **Step 1: 提交后端代码**

```bash
git add Yi.Abp/module/tenant-management/
git add Yi.Abp/module/rbac/Yi.Module.Rbac.Application/
git commit -m "feat(tenant): 新增租户套餐模块后端实现"
```

- [ ] **Step 2: 提交前端代码**

```bash
git add Yi.Vben5/apps/web-antd/src/views/system/tenant/
git commit -m "feat(tenant): 启用前端租户套餐功能"
```