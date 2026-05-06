# 租户套餐模块设计文档

## 概述

在现有 `tenant-management` 模块中新增租户套餐功能，用于定义租户可访问的菜单集合。套餐与租户一对一关联，切换套餐时自动同步菜单权限。

## 参考

- RuoYi-Vue-Plus 的 `sys_tenant_package` 实现
- 项目现有 `RoleMenuEntity` 关联表模式

## 实体设计

### TenantPackageAggregateRoot（新增）

表名：`TenantPackage`

| 字段 | 类型 | 默认值 | 说明 |
|------|------|--------|------|
| Id | Guid | - | 主键 |
| PackageName | string | - | 套餐名称 |
| MenuCheckStrictly | bool | true | 菜单树选择是否父子关联 |
| Status | bool | true | 状态（启用/禁用） |
| Remark | string? | null | 备注（最大500字符） |
| OrderNum | int | 0 | 排序号 |
| IsDeleted | bool | false | 软删除 |
| CreationTime | DateTime | DateTime.Now | 创建时间 |
| CreatorId | Guid? | null | 创建者ID |
| LastModifierId | Guid? | null | 最后修改者ID |
| LastModificationTime | DateTime? | null | 最后修改时间 |

实现接口：`ISoftDelete`, `IAuditedObject`, `IOrderNum`, `IState`

注意：`MenuIds` 不作为实体字段存储，通过 `TenantPackageMenuEntity` 关联表管理。

### TenantPackageMenuEntity（新增，关联表）

表名：`TenantPackageMenu`

| 字段 | 类型 | 说明 |
|------|------|------|
| Id | Guid | 主键 |
| PackageId | Guid | 套餐ID |
| MenuId | Guid | 菜单ID |

与 `RoleMenuEntity` 模式一致。

### TenantAggregateRoot（修改）

新增字段：

| 字段 | 类型 | 说明 |
|------|------|------|
| PackageId | Guid? | 关联套餐ID |

## 后端服务设计

### TenantPackageService（新增）

继承 `YiCrudAppService<TenantPackageAggregateRoot, ...>`，实现 `ITenantPackageService`。

#### DTO 设计

**CreateInputVo / UpdateInputVo**：包含 `PackageName`、`MenuCheckStrictly`、`Status`、`Remark`、`OrderNum`、`MenuIds`（`List<Guid>`）。

**GetOutputDto / GetListOutputDto**：包含实体全部字段 + `MenuIds`（`List<Guid>`，从关联表查询填充）。

**GetListInputVo**：继承 `PagedAllResultRequestDto`，添加 `PackageName`、`Status` 过滤条件。

#### 标准 CRUD

基类 `YiCrudAppService` 提供，覆盖以下方法：

**GetListAsync** — 添加 `packageName` 模糊查询过滤

**DeleteAsync** — 删除前检查是否有租户引用该套餐，如有则抛出异常 `"租户套餐已被使用"`

#### 自定义方法

**ChangeStatusAsync(Guid packageId, bool status)** — 更新套餐状态

**GetMenuTreeSelectAsync(Guid packageId)** — 返回菜单树 + 套餐已选菜单 ID 列表（checkedKeys）。packageId 为 0 时返回全部菜单（新增模式）。

### TenantService（修改）

#### SyncPackageAsync(Guid tenantId, Guid packageId)

同步套餐菜单到租户，逻辑：

1. 查询套餐关联的菜单 ID 列表（从 `TenantPackageMenu` 表）
2. 查询租户下的所有角色
3. 遍历角色：
   - **管理员角色**（RoleCode == "admin"）：删除该角色的全部 `RoleMenu`，重新插入套餐的菜单
   - **其他角色**：删除该角色中不在套餐菜单列表中的 `RoleMenu`（裁剪多余权限）

### MenuService（修改）

新增 `GetTenantPackageMenuTreeAsync(Guid packageId)` 方法：

- 返回完整的菜单树 + packageId 对应套餐已选的菜单 ID 列表
- 供前端套餐编辑页面的菜单树选择器使用
- packageId 为 0 时返回全部菜单（新增模式使用）

## API 端点

基于 ABP 动态 API 自动映射，`TenantPackageService` 映射为 `/api/tenant/package/*`：

| HTTP 方法 | 路径 | 对应方法 |
|-----------|------|----------|
| GET | /api/tenant/package | GetListAsync |
| GET | /api/tenant/package/{id} | GetAsync |
| POST | /api/tenant/package | CreateAsync |
| PUT | /api/tenant/package/{id} | UpdateAsync |
| DELETE | /api/tenant/package | DeleteAsync (批量) |
| PUT | /api/tenant/package/changeStatus | ChangeStatusAsync |
| GET | /api/tenant/package/select-list | GetSelectDataListAsync |

菜单树端点（通过 MenuService）：

| HTTP 方法 | 路径 | 说明 |
|-----------|------|------|
| GET | /api/menu/tenant-package-menu-treeselect/{packageId} | 套餐菜单树 |

同步端点（通过 TenantService）：

| HTTP 方法 | 路径 | 说明 |
|-----------|------|------|
| GET | /api/tenant/syncTenantPackage | syncTenantPackage(tenantId, packageId) |

## 前端适配

前端已有完整代码，需取消注释以下内容：

### tenant-drawer.vue
- 取消注释 `packageSelectList` import
- 取消注释 `setupPackageSelect` 函数
- 取消注释模板中的 `packageId` 下拉选择框

### data.tsx
- 取消注释 `packageId` 列定义
- 取消注释 `packageId` drawer schema 字段

### index.vue
- 取消注释 `tenantSyncPackage` import
- 取消注释 `handleSync` 函数
- 取消注释模板中的同步按钮

## 业务规则

1. **删除保护**：套餐被租户引用时不可删除
2. **名称唯一**：套餐名称不可重复
3. **同步机制**：租户切换套餐后自动同步菜单权限
4. **状态控制**：禁用的套餐不可选择，已关联的租户不受影响

## 实施步骤

### Step 1：生成实体类

手动创建 `TenantPackageAggregateRoot.cs` 和 `TenantPackageMenuEntity.cs`（关联表不适合用脚本生成）。

### Step 2：使用 /crud-generator-plus 生成 CRUD

```
/crud-generator-plus 基于以下实体类生成 CRUD：
- 实体：Yi.Abp/module/tenant-management/Yi.Module.TenantManagement.Domain/Entities/TenantPackageAggregateRoot.cs
- 模块：tenant-management
```

脚本会自动生成：
- 后端 DTOs + Service（7 个文件）
- 前端 API + Views（5 个文件）
- 种子数据（菜单 + 权限）

### Step 3：手动补充逻辑

脚本生成后需手动补充：
1. `TenantPackageService` 中覆盖 `DeleteAsync`（添加删除保护）
2. `TenantPackageService` 中覆盖 `CreateAsync` / `UpdateAsync`（处理 MenuIds 关联表）
3. `TenantService` 中新增 `SyncPackageAsync`
4. `MenuService` 中新增 `GetTenantPackageMenuTreeAsync`
5. `TenantAggregateRoot` 中新增 `PackageId` 字段
6. 取消注释前端 tenant 视图中的套餐相关代码

## 文件清单

### 后端新增文件

```
Yi.Abp/module/tenant-management/
├── Yi.Module.TenantManagement.Domain/
│   └── Entities/
│       ├── TenantPackageAggregateRoot.cs
│       └── TenantPackageMenuEntity.cs
├── Yi.Module.TenantManagement.Application.Contracts/
│   ├── IServices/
│   │   └── ITenantPackageService.cs
│   └── Dtos/
│       └── TenantPackage/
│           ├── TenantPackageGetOutputDto.cs
│           ├── TenantPackageGetListOutputDto.cs
│           ├── TenantPackageGetListInputVo.cs
│           ├── TenantPackageCreateInputVo.cs
│           └── TenantPackageUpdateInputVo.cs
├── Yi.Module.TenantManagement.Application/
│   └── Services/
│       └── TenantPackageService.cs
└── Yi.Module.TenantManagement.SqlSugarCore/
    └── (无需新增，基类仓储自动支持)
```

### 后端修改文件

```
Yi.Abp/module/tenant-management/
├── Yi.Module.TenantManagement.Domain/
│   └── TenantAggregateRoot.cs              ← 新增 PackageId 字段
├── Yi.Module.TenantManagement.Application/
│   └── TenantService.cs                    ← 新增 SyncPackageAsync
└── Yi.Module.Rbac.Application/
    └── Services/System/MenuService.cs      ← 新增 GetTenantPackageMenuTreeAsync
```

### 前端修改文件（取消注释）

```
Yi.Vben5/apps/web-antd/src/
├── views/system/tenant/
│   ├── tenant-drawer.vue
│   ├── data.tsx
│   └── index.vue
```

### 配置修改

`YiAbpWebModule.cs` 中已有 `tenant-management` 模块的 Dynamic API 注册，`TenantPackageService` 会自动映射为 `/api/tenant/package/*` 端点（与 `TenantService` 共享同一 RemoteServiceName 和 RootPath）。
