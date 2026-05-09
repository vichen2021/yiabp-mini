# 阿里云 OSS 文件存储配置切换设计方案

## 背景

当前项目文件管理基于 ABP Blob Storing，本地文件存储 Provider 配置在 RBAC 应用层模块中。

现有实现具备一个较好的迁移基础：业务代码没有直接依赖本地文件系统路径，而是统一通过 `IBlobContainer<FileManagementContainer>` 读写文件。因此本次改造重点不是重写文件上传下载逻辑，而是将 Blob Provider 从固定 FileSystem 改造为可配置的 FileSystem / Aliyun OSS 二选一。

本项目是开源项目，当前无线上数据负担，因此方案不设计复杂的历史数据迁移、双写、灰度切换和回滚补偿流程。默认可以接受在版本升级后由部署者选择使用本地存储或 OSS，并以新环境配置为准。

---

## 目标

- 支持通过配置选择文件存储 Provider：`FileSystem` 或 `Aliyun`。
- 保留当前文件上传、下载、删除接口行为，尽量不影响前端和业务调用方。
- 保留本地存储作为默认值，降低开源项目本地启动门槛。
- 生产部署可通过配置切换到阿里云 OSS。
- Bucket 建议使用私有读写，由后端继续代理下载，保证权限控制和接口兼容。
- 不引入用户级、租户级运行时动态切换，避免过度设计。

---

## 非目标

- 不实现在线数据迁移工具。
- 不实现同一运行实例内的运行时 Provider 热切换。
- 不实现按用户、按租户、按文件维度选择不同存储。
- 不实现 OSS 前端直传。
- 不强制使用 OSS 公网直链或 CDN。
- 不改造现有文件元数据表结构。

---

## 当前实现分析

### 文件容器

文件容器定义在：

```text
Yi.Abp/module/rbac/Yi.Module.Rbac.Domain/File/FileManagementContainer.cs
```

容器名：

```text
file-management
```

该容器是当前文件管理模块的唯一 Blob 容器。

### 当前 Provider 配置

当前配置位于：

```text
Yi.Abp/module/rbac/Yi.Module.Rbac.Application/YiModuleRbacApplicationModule.cs
```

当前逻辑固定使用 FileSystem Provider：

```text
BasePath = wwwroot/FileStorage
```

### 文件读写调用点

主要调用点：

```text
Yi.Abp/module/rbac/Yi.Module.Rbac.Domain/Managers/FileManager.cs
Yi.Abp/module/rbac/Yi.Module.Rbac.Application/Services/System/FileService.cs
```

当前核心操作：

| 操作 | 当前方式 |
|------|----------|
| 上传 | `_blobContainer.SaveAsync(entity.Id.ToString(), content, overwrite)` |
| 下载 | `_blobContainer.GetAsync(id.ToString())` |
| 删除 | `_blobContainer.DeleteAsync(id.ToString())` |

BlobName 当前使用文件 `Id` 字符串，不依赖原始文件名。这一点对切换 OSS 有利，可以避免中文文件名、特殊字符、路径穿越等问题。

---

## 总体设计

采用 ABP Blob Storing 的 Provider 配置扩展能力，在 `FileManagementContainer` 上根据配置选择不同 Provider。

整体结构：

```text
FileService / FileManager
        |
        v
IBlobContainer<FileManagementContainer>
        |
        v
AbpBlobStoringOptions
        |
        +-- FileSystem Provider
        |
        +-- Aliyun OSS Provider
```

业务层继续只依赖 `IBlobContainer<FileManagementContainer>`，不感知底层存储差异。

---

## 配置设计

### appsettings 配置结构

建议在主启动项目配置文件中增加：

```json
{
  "BlobStoring": {
    "Provider": "FileSystem",
    "FileSystem": {
      "BasePath": "wwwroot/FileStorage"
    },
    "Aliyun": {
      "AccessKeyId": "",
      "AccessKeySecret": "",
      "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
      "BucketName": "yiabp-mini",
      "CreateBucketIfNotExists": false
    }
  }
}
```

### 配置项说明

| 配置项 | 说明 | 默认建议 |
|--------|------|----------|
| `BlobStoring:Provider` | 当前使用的文件存储 Provider | `FileSystem` |
| `BlobStoring:FileSystem:BasePath` | 本地文件保存目录 | `wwwroot/FileStorage` |
| `BlobStoring:Aliyun:AccessKeyId` | 阿里云 AccessKeyId | 由部署者配置 |
| `BlobStoring:Aliyun:AccessKeySecret` | 阿里云 AccessKeySecret | 由部署者配置，不能提交真实密钥 |
| `BlobStoring:Aliyun:Endpoint` | OSS Endpoint | 按 Bucket 所在地域配置 |
| `BlobStoring:Aliyun:BucketName` | OSS Bucket 名称 | 由部署者配置 |
| `BlobStoring:Aliyun:CreateBucketIfNotExists` | Bucket 不存在时是否创建 | 开源项目建议 `false` |

### Provider 可选值

建议支持以下值：

| 值 | 含义 |
|----|------|
| `FileSystem` | 使用 ABP 本地文件系统 Blob Provider |
| `Aliyun` | 使用 ABP 阿里云 OSS Blob Provider |

Provider 字符串比较应忽略大小写。

### 默认策略

默认使用 `FileSystem`。

原因：

- 开源项目本地启动成本低。
- 不要求贡献者准备 OSS 账号。
- 不泄露云厂商密钥。
- 对现有开发体验影响最小。

---

## 依赖设计

### Application 项目包引用

目标项目：

```text
Yi.Abp/module/rbac/Yi.Module.Rbac.Application/Yi.Module.Rbac.Application.csproj
```

保留本地 Provider：

```xml
<PackageReference Include="Volo.Abp.BlobStoring.FileSystem" Version="$(AbpVersion)" />
```

新增阿里云 Provider：

```xml
<PackageReference Include="Volo.Abp.BlobStoring.Aliyun" Version="$(AbpVersion)" />
```

如果当前 ABP 版本或包名存在差异，以项目实际 ABP 版本可用包为准。

### 模块依赖

目标文件：

```text
Yi.Abp/module/rbac/Yi.Module.Rbac.Application/YiModuleRbacApplicationModule.cs
```

当前依赖：

```text
AbpBlobStoringFileSystemModule
```

改造后应同时依赖：

```text
AbpBlobStoringFileSystemModule
AbpBlobStoringAliyunModule
```

同时需要增加 Aliyun Provider 对应命名空间引用。

---

## 模块配置设计

### 配置位置

本次仍将 `FileManagementContainer` 的 Provider 配置放在 `YiModuleRbacApplicationModule.ConfigureServices` 中。

原因：

- 当前文件管理能力属于 RBAC 模块应用层。
- 现有配置已经在该模块中，改动集中。
- `FileManager` 和 `FileService` 无需感知 Provider。

### 配置逻辑

伪代码如下：

```csharp
var configuration = context.Services.GetConfiguration();
var provider = configuration["BlobStoring:Provider"] ?? "FileSystem";

Configure<AbpBlobStoringOptions>(options =>
{
    options.Containers.Configure<FileManagementContainer>(container =>
    {
        if (string.Equals(provider, "Aliyun", StringComparison.OrdinalIgnoreCase))
        {
            container.UseAliyun(aliyun =>
            {
                aliyun.AccessKeyId = configuration["BlobStoring:Aliyun:AccessKeyId"];
                aliyun.AccessKeySecret = configuration["BlobStoring:Aliyun:AccessKeySecret"];
                aliyun.Endpoint = configuration["BlobStoring:Aliyun:Endpoint"];
                aliyun.BucketName = configuration["BlobStoring:Aliyun:BucketName"];
                aliyun.CreateBucketIfNotExists = configuration.GetValue<bool>("BlobStoring:Aliyun:CreateBucketIfNotExists");
            });
        }
        else
        {
            container.UseFileSystem(fileSystem =>
            {
                fileSystem.BasePath = configuration["BlobStoring:FileSystem:BasePath"] ?? "wwwroot/FileStorage";
            });
        }
    });
});
```

实际实现时应根据当前 ABP Aliyun Provider 的 Options 类型和属性名调整。

---

## 接口兼容设计

### 上传接口

当前单文件上传返回：

```text
/api/file/get/{id}
```

改造后保持不变。

无论底层使用本地存储还是 OSS，前端仍然通过后端地址访问文件。

### 下载接口

当前下载通过后端读取 Blob Stream 后返回 `FileStreamResult`。

改造后保持不变。

好处：

- 前端无需适配 OSS 地址。
- 权限模型保持不变。
- Bucket 可以设置为私有。
- 可以避免公开暴露 ObjectName。

### 删除接口

当前删除同时删除文件元数据和 Blob。

改造后保持不变。

---

## OSS Bucket 建议

### 访问权限

建议 Bucket 设置为：

```text
私有读写
```

原因：

- 当前系统通过后端接口下载文件。
- 权限校验仍由系统控制。
- 避免上传文件被公开访问。

### ObjectName 规则

继续使用当前 BlobName：

```text
{FileId}
```

例如：

```text
3f3d6a8f-6d38-4db2-bd3c-6d64f3c88a12
```

不建议在本阶段改为按文件名保存。

原因：

- 当前业务已经以 `Id` 作为 BlobName。
- 避免文件名重复问题。
- 避免特殊字符和编码问题。
- 保持 FileSystem 与 OSS 行为一致。

### CORS

如果继续由后端代理下载，OSS Bucket 暂不需要为前端访问配置 CORS。

如果未来改为前端直传或直连访问，再单独设计 CORS、签名 URL、CDN 和防盗链策略。

---

## 安全设计

### 密钥管理

真实的 `AccessKeyId` 和 `AccessKeySecret` 不能提交到仓库。

推荐方式：

- 本地开发使用 `appsettings.Development.json` 或用户机密配置。
- Docker / 生产环境使用环境变量覆盖配置。
- CI/CD 使用 Secret 注入。

### 最小权限

建议为项目单独创建 RAM 用户，授权范围限制到指定 Bucket。

建议权限：

- 读取 Object
- 写入 Object
- 删除 Object
- 查询 Bucket 基础信息

不建议使用主账号 AccessKey。

### 后端代理访问

当前方案继续由后端返回文件流，因此文件访问权限仍可以复用系统已有认证、授权和操作日志机制。

---

## 开源项目默认配置策略

由于项目是开源项目，推荐仓库内默认配置为本地存储：

```json
{
  "BlobStoring": {
    "Provider": "FileSystem",
    "FileSystem": {
      "BasePath": "wwwroot/FileStorage"
    },
    "Aliyun": {
      "AccessKeyId": "",
      "AccessKeySecret": "",
      "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
      "BucketName": "",
      "CreateBucketIfNotExists": false
    }
  }
}
```

文档中说明部署者如需 OSS，只需要覆盖：

```json
{
  "BlobStoring": {
    "Provider": "Aliyun",
    "Aliyun": {
      "AccessKeyId": "your-access-key-id",
      "AccessKeySecret": "your-access-key-secret",
      "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
      "BucketName": "your-bucket-name",
      "CreateBucketIfNotExists": false
    }
  }
}
```

---

## 数据迁移策略

本项目当前无线上数据负担，因此不设计正式数据迁移流程。

### 新环境

新环境直接按配置启动即可：

- `Provider = FileSystem`：文件保存到本地目录。
- `Provider = Aliyun`：文件保存到 OSS Bucket。

### 已有本地开发文件

如果开发者已有本地测试文件，切换到 OSS 后旧文件可能无法读取。

处理方式：

- 开发环境可清空测试文件元数据后重新上传。
- 或保持 `Provider = FileSystem` 继续使用本地文件。
- 如确需迁移，可临时编写一次性脚本读取本地 Blob 后写入 OSS。

### 不做双写

本次不做 FileSystem 与 OSS 双写。

原因：

- 无线上数据负担。
- 双写会引入一致性问题。
- 开源项目默认应保持简单易懂。

---

## 异常处理设计

### Provider 配置非法

如果 `BlobStoring:Provider` 为空或无法识别，建议回退到 `FileSystem`。

可识别值只有：

- `FileSystem`
- `Aliyun`

### OSS 必填项缺失

当 `Provider = Aliyun` 时，以下配置应视为必填：

- `AccessKeyId`
- `AccessKeySecret`
- `Endpoint`
- `BucketName`

推荐在启动时校验，缺失则抛出明确异常，避免运行到上传时才失败。

异常信息示例：

```text
BlobStoring:Aliyun:BucketName is required when BlobStoring:Provider is Aliyun.
```

### 上传失败

当前 `FileManager.CreateAsync` 先写文件元数据，再保存 Blob。

如果 OSS 保存失败，可能出现元数据已写入但 Blob 不存在的问题。

本次 Provider 切换不是根因，但建议后续优化：

- 先保存 Blob，成功后写元数据。
- 或在 Blob 保存失败时删除已插入元数据。
- 或通过 UnitOfWork 和异常补偿保持一致性。

在当前无线上数据负担背景下，该问题可作为后续优化项。

---

## 测试方案

### 本地 Provider 测试

配置：

```json
{
  "BlobStoring": {
    "Provider": "FileSystem"
  }
}
```

验证：

- 上传单个文件成功。
- 上传后返回 `/api/file/get/{id}`。
- 通过返回地址可访问文件。
- 下载接口可下载文件且文件名正确。
- 删除文件后再次访问返回错误。
- 本地目录 `wwwroot/FileStorage` 下产生文件。

### OSS Provider 测试

配置：

```json
{
  "BlobStoring": {
    "Provider": "Aliyun",
    "Aliyun": {
      "AccessKeyId": "***",
      "AccessKeySecret": "***",
      "Endpoint": "oss-cn-hangzhou.aliyuncs.com",
      "BucketName": "***",
      "CreateBucketIfNotExists": false
    }
  }
}
```

验证：

- 应用启动成功。
- 上传文件成功。
- OSS Bucket 中出现对应 Object。
- ObjectName 与文件 `Id` 一致。
- 通过 `/api/file/get/{id}` 可访问文件。
- `DownloadAsync` 下载文件 ContentType 正确。
- 删除文件后 OSS Object 被删除。

### 错误配置测试

验证以下情况：

- `Provider = Aliyun` 但 `BucketName` 为空，应启动失败或抛出明确异常。
- `AccessKeySecret` 错误，上传应失败并返回明确错误日志。
- `Endpoint` 错误，上传应失败并记录连接错误。

---

## 实施步骤

### 第一步：增加 Aliyun Provider 依赖

修改：

```text
Yi.Abp/module/rbac/Yi.Module.Rbac.Application/Yi.Module.Rbac.Application.csproj
```

新增：

```xml
<PackageReference Include="Volo.Abp.BlobStoring.Aliyun" Version="$(AbpVersion)" />
```

保留：

```xml
<PackageReference Include="Volo.Abp.BlobStoring.FileSystem" Version="$(AbpVersion)" />
```

### 第二步：增加模块依赖

修改：

```text
Yi.Abp/module/rbac/Yi.Module.Rbac.Application/YiModuleRbacApplicationModule.cs
```

在 `[DependsOn]` 中同时注册：

```text
AbpBlobStoringFileSystemModule
AbpBlobStoringAliyunModule
```

### 第三步：改造 BlobStoring 配置

将固定 `UseFileSystem` 改为按 `BlobStoring:Provider` 判断。

默认：

```text
FileSystem
```

OSS：

```text
Aliyun
```

### 第四步：增加 appsettings 示例配置

建议至少修改主 Web 项目的配置示例：

```text
Yi.Abp/src/Yi.Abp.Web/appsettings.json
```

如测试项目需要上传文件，也同步增加：

```text
Yi.Abp/test/Yi.Module.Rbac.Test/appsettings.json
Yi.Abp/test/Yi.Abp.Test/appsettings.json
```

### 第五步：补充 README 或部署说明

说明内容：

- 默认使用本地文件系统。
- 如何切换到阿里云 OSS。
- 密钥不要提交到仓库。
- Bucket 建议私有读写。

---

## 推荐实现结构

为了避免 `YiModuleRbacApplicationModule` 中配置逻辑过长，可以选择引入一个内部静态方法：

```text
ConfigureFileBlobStoring(context)
```

职责：

- 读取配置。
- 判断 Provider。
- 校验 OSS 必填项。
- 配置 `FileManagementContainer`。

是否拆方法由实现时决定。若改动较小，也可以直接保留在 `ConfigureServices` 中。

---

## 后续优化方向

### 文件元数据与 Blob 一致性

当前上传流程存在元数据先落库、Blob 后保存的风险。

后续可调整为：

```text
保存 Blob 成功 -> 插入文件元数据
```

或在 Blob 保存失败时执行补偿删除。

### 大文件上传

当前上传会将文件完整读入内存。

后续可考虑：

- 非图片文件流式上传。
- 图片文件保持内存压缩。
- 大文件分片上传。
- OSS 前端直传。

### OSS 直链访问

如果未来需要减少应用服务器流量，可设计：

- 私有 Bucket 签名 URL。
- CDN 域名。
- URL 过期时间。
- 文件访问权限校验后生成临时地址。

### 租户级存储策略

如果后续需要 SaaS 多租户差异化存储，可扩展文件表字段：

| 字段 | 说明 |
|------|------|
| `StorageProvider` | 文件实际存储 Provider |
| `BlobName` | 实际 Blob/Object 名称 |
| `BucketName` | OSS Bucket 名称 |
| `Endpoint` | OSS Endpoint |

并引入自定义 `IFileObjectStorage` 路由层。

本阶段不建议提前实现。

---

## 设计结论

本次改造推荐采用“配置级 Provider 切换”方案：

```text
默认 FileSystem，生产可配置为 Aliyun OSS。
```

该方案对当前项目最合适：

- 改动集中在模块依赖、包引用和 BlobStoring 配置。
- `FileManager`、`FileService` 基本无需调整。
- 前端接口无需调整。
- 本地开发体验不变。
- 开源项目不会强依赖云服务。
- 无线上数据负担，因此无需复杂迁移方案。

最终效果：

```text
开发者克隆项目后默认本地存储即可运行；部署者需要 OSS 时，仅配置 Provider 和 OSS 参数即可切换。
```
