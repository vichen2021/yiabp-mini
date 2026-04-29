using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Yi.Framework.Operation.Abstractions.Attributes;
using Yi.Framework.Operation.Abstractions.Enums;
using Yi.Framework.Operation.Abstractions.Metadata;
using Yi.Framework.Operation.Abstractions.Permissions;

namespace Yi.Framework.Operation.Core.Metadata
{
    /// <summary>
    /// 默认Action元数据解析器
    /// 权限和日志共用同一套解析规则
    /// </summary>
    public class DefaultActionMetadataResolver : IActionMetadataResolver
    {
        private readonly ActionMetadataCache _cache;
        private readonly PermissionOptions _options;

        public DefaultActionMetadataResolver(
            ActionMetadataCache cache,
            IOptions<PermissionOptions> options)
        {
            _cache = cache;
            _options = options.Value;
        }

        public ActionMetadata Resolve(ControllerActionDescriptor descriptor)
        {
            var cacheKey = $"MVC:{descriptor.ControllerTypeInfo.FullName}:{descriptor.MethodInfo.Name}";
            if (_cache.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            var metadata = ParseMetadata(descriptor.ControllerTypeInfo.AsType(), descriptor.MethodInfo);
            _cache.Set(cacheKey, metadata);
            return metadata;
        }

        public ActionMetadata Resolve(Type serviceType, MethodInfo methodInfo)
        {
            var cacheKey = $"Service:{serviceType.FullName}:{methodInfo.Name}";
            if (_cache.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            var metadata = ParseMetadata(serviceType, methodInfo);
            _cache.Set(cacheKey, metadata);
            return metadata;
        }

        private ActionMetadata ParseMetadata(Type serviceType, MethodInfo methodInfo)
        {
            var metadata = new ActionMetadata();

            // 1. 检查特性标记
            // 显式权限特性（优先级最高）
            var permissionAttr = methodInfo.GetCustomAttributes<PermissionAttribute>().FirstOrDefault();
            if (permissionAttr != null)
            {
                metadata.ExplicitPermissionCode = permissionAttr.Code;
            }
            else
            {
                // 2. 检查配置映射表（优先级低于显式特性，高于自动推断）
                var mappingKey = $"{serviceType.FullName}.{methodInfo.Name}";
                if (_options.Mappings.TryGetValue(mappingKey, out var mappedCode))
                {
                    metadata.ExplicitPermissionCode = mappedCode;
                }
            }

            // 显式日志特性
            var operLogAttr = methodInfo.GetCustomAttribute<OperLogAttribute>();
            if (operLogAttr != null)
            {
                metadata.ExplicitLogInfo = (operLogAttr.Title, operLogAttr.OperType);
            }

            // 忽略特性（支持类级和方法级）
            metadata.IgnorePermission = methodInfo.GetCustomAttribute<IgnorePermissionAttribute>() != null
                || serviceType.GetCustomAttribute<IgnorePermissionAttribute>() != null;
            metadata.IgnoreLog = methodInfo.GetCustomAttribute<IgnoreOperLogAttribute>() != null
                || serviceType.GetCustomAttribute<IgnoreOperLogAttribute>() != null;

            // 3. 解析模块名
            metadata.ModuleName = InferModuleName(serviceType);

            // 4. 解析实体名
            metadata.EntityName = InferEntityName(serviceType);

            // 5. 解析操作名
            metadata.ActionName = InferActionName(methodInfo.Name);

            // 无法推断操作名时，标记为未解析
            if (metadata.ActionName == null)
            {
                metadata.IsResolved = false;
                return metadata;
            }

            metadata.IsWriteOperation = IsWriteOperation(metadata.ActionName);

            // 6. 推断操作类型和日志标题
            if (metadata.ExplicitLogInfo == null)
            {
                metadata.OperType = InferOperType(metadata.ActionName);
                var operLogEntityAttr = serviceType.GetCustomAttribute<OperLogEntityAttribute>();
                metadata.LogTitle = InferLogTitle(operLogEntityAttr?.DisplayName ?? metadata.EntityName, metadata.OperType);
            }

            // 7. 推断权限码（仅当无显式权限时）
            if (string.IsNullOrEmpty(metadata.ExplicitPermissionCode))
            {
                metadata.PermissionCode = $"{metadata.ModuleName}:{metadata.EntityName}:{metadata.ActionName}";
            }

            metadata.IsResolved = true;

            return metadata;
        }

        /// <summary>
        /// 推断模块名（目前基于命名空间，后续可扩展 RemoteServiceName）
        /// </summary>
        private string InferModuleName(Type serviceType)
        {
            var ns = serviceType.Namespace ?? "";

            // 根据命名空间推断
            if (ns.Contains(".TenantManagement")) return "system";
            if (ns.Contains(".Services.System")) return "system";
            if (ns.Contains(".Services.Monitor")) return "monitor";
            if (ns.Contains(".Services.RecordLog")) return "log";
            if (ns.Contains(".Services.Authentication")) return "auth";
            if (ns.Contains(".AuditLogging")) return "monitor";

            // 默认为 system
            return "system";
        }

        /// <summary>
        /// 推断实体名
        /// </summary>
        private string InferEntityName(Type serviceType)
        {
            var serviceName = serviceType.Name;
            if (serviceName.EndsWith("Service"))
            {
                serviceName = serviceName.Substring(0, serviceName.Length - 7);
            }

            return EntityNameNormalizer.Normalize(serviceName);
        }

        /// <summary>
        /// 推断操作名（权限码的 action 部分）
        /// 统一规则：精确匹配 + HTTP 方法前缀匹配
        /// </summary>
        private string? InferActionName(string methodName)
        {
            // 1. 精确匹配标准 CRUD 方法
            return methodName switch
            {
                "GetListAsync" or "GetSelectDataListAsync" => "list",
                "GetAsync" => "detail",
                "CreateAsync" => "add",
                "UpdateAsync" => "edit",
                "DeleteAsync" => "delete",
                "GetExportExcelAsync" or "ExportAsync" => "export",
                "PostImportExcelAsync" or "ImportAsync" => "import",
                _ => InferByPrefix(methodName)
            };
        }

        /// <summary>
        /// 前缀匹配补充规则
        /// Post* -> add, Put* -> edit, Delete* -> delete
        /// </summary>
        private string? InferByPrefix(string methodName)
        {
            // HTTP 方法前缀映射
            if (methodName.StartsWith("Post"))
            {
                // PostImportExcelAsync -> import
                if (methodName.Contains("Import")) return "import";
                return "add";
            }
            if (methodName.StartsWith("Put"))
            {
                return "edit";
            }
            if (methodName.StartsWith("Delete"))
            {
                return "delete";
            }
            if (methodName.StartsWith("Get"))
            {
                // GetExportExcelAsync -> export
                if (methodName.Contains("Export")) return "export";
                // 其他 Get 方法不生成权限码（查询类）
                return null;
            }

            // 方法名前缀映射
            if (methodName.StartsWith("Create") || methodName.StartsWith("Add") || methodName.StartsWith("Insert"))
            {
                return "add";
            }
            if (methodName.StartsWith("Update") || methodName.StartsWith("Edit") || methodName.StartsWith("Modify"))
            {
                return "edit";
            }
            if (methodName.StartsWith("Remove") || methodName.StartsWith("Clear"))
            {
                return "delete";
            }
            if (methodName.StartsWith("Export"))
            {
                return "export";
            }
            if (methodName.StartsWith("Import"))
            {
                return "import";
            }

            return null;
        }

        /// <summary>
        /// 判断是否写操作
        /// </summary>
        private bool IsWriteOperation(string actionName)
        {
            return actionName is "add" or "edit" or "delete" or "import" or "export";
        }

        /// <summary>
        /// 推断操作类型（用于日志）
        /// </summary>
        private OperEnum? InferOperType(string actionName)
        {
            return actionName switch
            {
                "add" => OperEnum.Insert,
                "edit" => OperEnum.Update,
                "delete" => OperEnum.Delete,
                "export" => OperEnum.Export,
                "import" => OperEnum.Import,
                _ => null
            };
        }

        /// <summary>
        /// 推断日志标题
        /// </summary>
        private string? InferLogTitle(string entityName, OperEnum? operType)
        {
            if (operType == null) return null;

            var actionDesc = operType switch
            {
                OperEnum.Insert => "添加",
                OperEnum.Update => "更新",
                OperEnum.Delete => "删除",
                OperEnum.Export => "导出",
                OperEnum.Import => "导入",
                OperEnum.Clear => "清空",
                _ => "操作"
            };

            return $"{actionDesc}{entityName}";
        }
    }
}