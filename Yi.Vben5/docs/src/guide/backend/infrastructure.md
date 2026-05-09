# 基础能力配置

本文说明后端常用基础能力的配置方式，包括租户套餐、文件存储 Provider 切换、阿里云 OSS、本地文件存储和阿里云短信。

## 租户套餐

租户套餐用于在宿主端维护一组可授权给租户的菜单范围。租户初始化或套餐同步时，系统会根据套餐中选择的宿主菜单，在租户数据库中匹配对应菜单并更新角色菜单关系。

### 主要服务

| 服务 | 说明 |
|------|------|
| `TenantPackageService` | 维护租户套餐、套餐菜单关系、套餐菜单树 |
| `TenantService.InitAsync` | 初始化租户数据库、种子数据和管理员账号 |
| `TenantService.SyncPackageAsync` | 将套餐菜单同步到指定租户 |

### 权限与日志

租户套餐服务声明：

```csharp
[PermissionResource("system", "tenantPackage")]
[OperLogEntity("租户套餐")]
public class TenantPackageService : YiCrudAppService<...>
{
}
```

租户服务中的初始化和套餐同步使用显式动作和操作记录：

```csharp
[PermissionAction(PermissionActionEnum.Edit)]
[OperLog("同步租户套餐", OperEnum.Update)]
public async Task SyncPackageAsync(Guid tenantId, Guid packageId)
{
}
```

### 使用流程

1. 在宿主端进入租户套餐页面。
2. 新增或编辑套餐，选择允许租户使用的菜单。
3. 创建租户时绑定套餐。
4. 初始化租户时，系统会创建租户数据库、执行种子数据、创建租户管理员。
5. 如果租户绑定套餐，则同步套餐菜单；否则同步全部租户可用菜单到管理员角色。

### 注意事项

- 套餐菜单保存的是宿主端菜单 ID。
- 租户数据库初始化后，租户内菜单 ID 会重新生成。
- 同步套餐时不会跨库复用宿主菜单 ID，而是根据菜单信息匹配租户本地菜单。
- 已被租户引用的套餐不允许删除。

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
- [前端上传组件](/guide/frontend/components/upload)
