# 基础能力配置

本文说明后端常用基础能力的配置方式，包括种子数据、配置来源、文件存储 Provider 切换、阿里云 OSS、本地文件存储和阿里云短信。

## 租户套餐

租户套餐、租户初始化、套餐同步和租户级文件存储属于多租户专题。详见 [多租户专题](/guide/backend/multi-tenancy)。

## 种子数据

种子数据用于初始化系统运行必须存在的基础数据，例如角色、用户、部门、岗位、菜单、参数配置、默认租户和租户套餐。

后端种子数据实现 `IDataSeedContributor`，并通过 `SeedAsync(DataSeedContext context)` 执行：

```csharp
public class RoleDataSeed : IDataSeedContributor, ITransientDependency
{
    public async Task SeedAsync(DataSeedContext context)
    {
        var isHost = context.TenantId is null;
        // Host 初始化平台超级管理员，Tenant 初始化租户管理员
    }
}
```

常见种子数据位置：

| 目录 | 说明 |
|------|------|
| `Yi.Module.Rbac.SqlSugarCore/DataSeeds` | 角色、用户、菜单、部门、岗位、系统参数等 RBAC 基础数据 |
| `Yi.Module.TenantManagement.SqlSugarCore/DataSeeds` | 默认租户、默认租户套餐等多租户基础数据 |

### 执行上下文

`DataSeedContext.TenantId` 用于区分宿主机和租户：

| 上下文 | `TenantId` | 典型数据 |
|--------|------------|----------|
| 宿主机 | `null` | 平台超级管理员、平台菜单、默认租户、租户套餐 |
| 租户 | 有值 | 租户管理员、租户内菜单、租户内角色权限 |

涉及多租户的数据不要只按“表是否为空”判断。宿主和租户可能使用相同实体结构，但语义不同，例如宿主机角色码为 `superadmin`，租户管理员角色码为 `admin`。

### 编写规则

- 种子数据必须幂等，多次执行不能产生重复数据。
- 使用稳定的业务键判断是否存在，例如 `RoleCode`、`UserName`、`MenuName`、`ConfigKey`。
- 需要升级历史数据时，应在种子中兼容旧值并迁移到新语义。
- 不要在种子中写入真实密钥、真实手机号、生产连接串等敏感数据。
- 租户侧种子不要创建平台治理能力，例如 `superadmin` 角色、租户管理菜单、租户套餐菜单。

## Config 与 Setting

项目中存在三类容易混淆的配置来源：

| 类型 | 存储位置 | 适用场景 | 是否适合租户覆盖 |
|------|----------|----------|------------------|
| `IConfiguration` / `appsettings.json` | 配置文件、环境变量、部署平台配置 | 应用启动、数据库连接、Redis、全局默认值 | 不适合由租户页面直接修改 |
| 系统参数 `ConfigService` | 业务数据库 `Config` 表 | 业务开关、可在后台维护的简单键值参数 | 按当前业务表设计决定 |
| ABP `Setting` | Setting 管理体系 | 模块级配置、可继承、可加密、可按租户覆盖 | 适合 |

### appsettings.json

`appsettings.json` 适合放应用启动和部署级配置，例如：

- 数据库连接
- Redis
- 日志和审计
- 默认文件存储 Provider
- 默认 OSS 配置
- 第三方服务默认配置

这类配置通常由运维或部署环境控制。业务页面不应直接改写 `appsettings.json`。

### 系统参数 Config

系统参数由 `ConfigService` 维护，入口通常是后台的参数配置页面。它适合简单业务键值，例如：

```http
GET /config/config-key/{configKey}
```

使用建议：

- 适合业务开关、文本参数、前后台都能理解的简单键值。
- `ConfigKey` 必须唯一。
- 不适合存储密钥、连接串、租户级 Provider 配置。
- 如果参数需要继承、加密或租户覆盖，优先使用 `Setting`。

### ABP Setting

`Setting` 适合模块级配置，支持定义默认值、继承和加密。以文件管理 OSS 为例：

```csharp
new SettingDefinition(
    FileManagementSettingNames.Aliyun.AccessKeySecret,
    isInherited: true,
    isEncrypted: true)
```

读取时通常使用 `ISettingProvider`，写入时使用 `ISettingManager`：

```csharp
var provider = await _settingProvider.GetOrNullAsync(FileManagementSettingNames.Provider);
await _settingManager.SetGlobalAsync(FileManagementSettingNames.Provider, "Aliyun");
```

租户级 Setting 可以通过当前租户上下文读取，也可以显式按租户写入。租户未设置时，如果 Setting 定义允许继承，会回退到上级配置。

推荐选择：

| 需求 | 推荐 |
|------|------|
| 应用启动必须读取 | `appsettings.json` |
| 后台维护简单业务参数 | `ConfigService` |
| 敏感配置 | `Setting`，并启用加密 |
| 租户可覆盖配置 | `Setting` |
| 模块默认配置 | `SettingDefinitionProvider` |

## 文件存储 Provider 切换

文件管理基于 ABP Blob Storing，业务代码统一通过 `IBlobContainer<FileManagementContainer>` 读写文件。

当前支持两种 Provider：

| Provider | 说明 |
|----------|------|
| `FileSystem` | 本地文件系统存储 |
| `Aliyun` | 阿里云 OSS 存储 |

### 配置结构

```json
{
  "BlobStoring": {
    "Provider": "FileSystem",
    "PathPrefix": "default",
    "FileSystem": {
      "BasePath": "wwwroot/FileStorage"
    },
    "Aliyun": {
      "AccessKeyId": "",
      "AccessKeySecret": "",
      "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
      "ContainerName": "",
      "CreateContainerIfNotExists": false
    }
  }
}
```

### 配置项说明

| 配置项 | 说明 |
|--------|------|
| `BlobStoring:Provider` | 当前使用的存储 Provider，可选 `FileSystem` 或 `Aliyun` |
| `BlobStoring:PathPrefix` | 业务层 BlobName 前缀，默认 `default` |
| `BlobStoring:FileSystem:BasePath` | 本地文件保存目录 |
| `BlobStoring:Aliyun:AccessKeyId` | 阿里云 AccessKeyId |
| `BlobStoring:Aliyun:AccessKeySecret` | 阿里云 AccessKeySecret |
| `BlobStoring:Aliyun:Endpoint` | OSS Endpoint |
| `BlobStoring:Aliyun:ContainerName` | OSS Bucket/容器名称 |
| `BlobStoring:Aliyun:CustomDomain` | OSS 自定义访问域名 |
| `BlobStoring:Aliyun:CreateContainerIfNotExists` | 容器不存在时是否自动创建 |

### 本地文件存储

使用本地文件系统时：

```json
{
  "BlobStoring": {
    "Provider": "FileSystem",
    "PathPrefix": "default",
    "FileSystem": {
      "BasePath": "wwwroot/FileStorage"
    }
  }
}
```

文件会保存到 `BasePath` 对应目录下，并继续保留 ABP Blob Storing 的容器和多租户隔离规则。

### 阿里云 OSS 存储

使用阿里云 OSS 时：

```json
{
  "BlobStoring": {
    "Provider": "Aliyun",
    "PathPrefix": "default",
    "Aliyun": {
      "AccessKeyId": "your-access-key-id",
      "AccessKeySecret": "your-access-key-secret",
      "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
      "ContainerName": "your-bucket-name",
      "CustomDomain": "https://cdn.example.com",
      "CreateContainerIfNotExists": false
    }
  }
}
```

文件上传时，业务层生成的 `StorageKey` 形如：

```text
default/{fileId}
```

ABP Blob Storing 会继续添加多租户隔离前缀，因此宿主端文件在 OSS 中通常类似：

```text
host/default/{fileId}
```

租户端文件会使用租户隔离前缀，例如：

```text
{tenant-prefix}/default/{fileId}
```

### 租户级 OSS 配置

宿主机可以提供全局默认文件存储配置；租户也可以在自己的上下文中覆盖文件存储配置。配置入口位于文件管理页面的“OSS 存储设置”。

前端接口已归入文件模块：

```http
GET /file-management/oss-settings
PUT /file-management/oss-settings
```

可配置字段：

| 字段 | 说明 |
|------|------|
| `Provider` | 文件存储 Provider，可选 `FileSystem` 或 `Aliyun` |
| `PathPrefix` | 业务层 `StorageKey` 前缀 |
| `AccessKeyId` | 阿里云 AccessKeyId |
| `AccessKeySecret` | 阿里云 AccessKeySecret，读取时不回显，留空表示不修改原密钥 |
| `Endpoint` | OSS Endpoint |
| `ContainerName` | OSS Bucket 名称 |
| `CustomDomain` | 自定义访问域名，为空时使用默认 Bucket 域名 |
| `CreateContainerIfNotExists` | Bucket 不存在时是否自动创建 |

解析优先级：

```text
当前租户 Setting
  -> appsettings.json 中的 BlobStoring 配置
```

因此，租户未配置 OSS 时会使用宿主或应用默认配置；租户配置后，上传、下载 URL 解析和删除会按当前租户设置执行。

### 文件 URL 与迁移

文件 URL 解析支持本地文件、阿里云 OSS 默认域名和阿里云 OSS 直连地址。配置 `CustomDomain` 后，返回给前端的文件 URL 会优先使用自定义域名。

当从本地文件系统切换到 OSS 时，可以通过文件模块提供的迁移能力把本地文件迁移到 OSS。迁移前需要先确认当前租户的 OSS 配置完整，包括 `Provider=Aliyun`、`Endpoint`、`ContainerName`、`AccessKeyId` 和 `AccessKeySecret`。

### 文件元数据

文件表会记录以下关键字段：

| 字段 | 说明 |
|------|------|
| `StorageKey` | 业务层 BlobName，例如 `default/{fileId}` |
| `Provider` | 上传时使用的 Provider，例如 `FileSystem` 或 `Aliyun` |
| `Extension` | 文件后缀 |
| `FileType` | 文件分类 |
| `Hash` | 文件内容 Hash |
| `ContentType` | MIME 类型 |

`Provider` 用于列表展示文件来源。当前下载和删除仍使用当前配置的 BlobContainer，不做按文件 Provider 路由读取。

### 敏感配置建议

`AccessKeyId` 和 `AccessKeySecret` 不应提交真实值。生产环境建议使用环境变量、部署平台密钥或未提交到仓库的本地配置文件覆盖。

## 阿里云短信

阿里云短信配置与 OSS 文件存储配置分开维护。短信使用 `AliyunOptions`，OSS 使用 `BlobStoring:Aliyun`。

### 配置结构

```json
{
  "AliyunOptions": {
    "AccessKeyId": "",
    "AccessKeySecret": "",
    "Sms": {
      "SignName": "",
      "TemplateCode": ""
    }
  }
}
```

### 配置项说明

| 配置项 | 说明 |
|--------|------|
| `AliyunOptions:AccessKeyId` | 阿里云短信 AccessKeyId |
| `AliyunOptions:AccessKeySecret` | 阿里云短信 AccessKeySecret |
| `AliyunOptions:Sms:SignName` | 短信签名 |
| `AliyunOptions:Sms:TemplateCode` | 短信模板 Code |

### 使用方式

业务服务通过 `IAliyunSmsManager` 发送短信：

```csharp
public class AccountService
{
    private readonly IAliyunSmsManager _aliyunSmsManager;

    public async Task SendCodeAsync(string phoneNumber, string code)
    {
        await _aliyunSmsManager.SendSmsAsync(phoneNumber, code);
    }
}
```

`AliyunSmsManager` 会在发送前校验必要配置，并在发送后校验阿里云返回码。返回码不是 `OK` 时，会记录警告日志并抛出用户友好的异常。

### 与 OSS 配置的边界

| 能力 | 配置节点 | 说明 |
|------|----------|------|
| 阿里云短信 | `AliyunOptions` | 用于验证码等短信发送 |
| 阿里云 OSS | `BlobStoring:Aliyun` | 用于文件存储 Provider |

不要把 OSS 配置放到 `AliyunOptions` 下，也不要把短信签名和模板放到 `BlobStoring` 下。

## 相关文档

- [权限与日志](/guide/backend/permission)
- [模块开发](/guide/backend/module)
- [枚举/系统字典](/guide/backend/enum)
- [多租户专题](/guide/backend/multi-tenancy)
- [前端上传组件](/guide/frontend/components/upload)
