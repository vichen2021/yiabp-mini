using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Yi.Framework.Operation.Abstractions.Attributes;
using Yi.Framework.Operation.Abstractions.Enums;
using Yi.Framework.Operation.Abstractions.Metadata;

namespace Yi.Framework.Operation.Core.Metadata
{
    /// <summary>
    /// 默认Action元数据解析器
    /// </summary>
    public class DefaultActionMetadataResolver : IActionMetadataResolver
    {
        private readonly ActionMetadataCache _cache;

        public DefaultActionMetadataResolver(ActionMetadataCache cache)
        {
            _cache = cache;
        }

        public ActionMetadata Resolve(ControllerActionDescriptor descriptor)
        {
            // 优先从缓存获取
            var cacheKey = GetCacheKey(descriptor);
            if (_cache.TryGetValue(cacheKey, out var cached))
            {
                return cached;
            }

            var metadata = ParseMetadata(descriptor);
            _cache.Set(cacheKey, metadata);
            return metadata;
        }

        private ActionMetadata ParseMetadata(ControllerActionDescriptor descriptor)
        {
            var metadata = new ActionMetadata();

            // 1. 检查特性标记
            var methodInfo = descriptor.MethodInfo;

            // 显式权限特性
            var permissionAttr = methodInfo.GetCustomAttributes<PermissionAttribute>().FirstOrDefault();
            if (permissionAttr != null)
            {
                metadata.ExplicitPermissionCode = permissionAttr.Code;
            }

            // 显式日志特性
            var operLogAttr = methodInfo.GetCustomAttribute<OperLogAttribute>();
            if (operLogAttr != null)
            {
                metadata.ExplicitLogInfo = (operLogAttr.Title, operLogAttr.OperType);
            }

            // 忽略特性
            metadata.IgnorePermission = methodInfo.GetCustomAttribute<IgnorePermissionAttribute>() != null;

            // 2. 解析模块名
            metadata.ModuleName = InferModuleName(descriptor);

            // 3. 解析实体名
            metadata.EntityName = InferEntityName(descriptor);

            // 4. 解析操作名
            metadata.ActionName = InferActionName(descriptor.MethodInfo.Name);

            // 无法推断操作名时，标记为未解析
            if (metadata.ActionName == null)
            {
                metadata.IsResolved = false;
                return metadata;
            }

            metadata.IsWriteOperation = IsWriteOperation(metadata.ActionName);

            // 5. 推断操作类型和日志标题
            if (metadata.ExplicitLogInfo == null)
            {
                metadata.OperType = InferOperType(metadata.ActionName);
                metadata.LogTitle = InferLogTitle(metadata.EntityName, metadata.OperType);
            }

            // 6. 推断权限码
            metadata.PermissionCode = $"{metadata.ModuleName}:{metadata.EntityName}:{metadata.ActionName}";
            metadata.IsResolved = true;

            return metadata;
        }

        /// <summary>
        /// 推断模块名
        /// </summary>
        private string InferModuleName(ControllerActionDescriptor descriptor)
        {
            var controllerType = descriptor.ControllerTypeInfo;
            var ns = controllerType.Namespace ?? "";

            // 根据命名空间推断
            if (ns.Contains(".TenantManagement")) return "system";
            if (ns.Contains(".Services.System")) return "system";
            if (ns.Contains(".Services.Monitor")) return "monitor";
            if (ns.Contains(".Services.RecordLog")) return "log";
            if (ns.Contains(".Services.Authentication")) return "auth";

            // 默认为 system
            return "system";
        }

        /// <summary>
        /// 推断实体名
        /// </summary>
        private string InferEntityName(ControllerActionDescriptor descriptor)
        {
            var controllerType = descriptor.ControllerTypeInfo;

            // 从控制器名推断：UserService → user
            var controllerName = controllerType.Name;
            if (controllerName.EndsWith("Service"))
            {
                controllerName = controllerName.Substring(0, controllerName.Length - 7);
            }

            return EntityNameNormalizer.Normalize(controllerName);
        }

        /// <summary>
        /// 推断操作名
        /// 无法识别的方法返回 null，不强行标记为 other
        /// </summary>
        private string? InferActionName(string methodName)
        {
            // 方法名映射
            return methodName switch
            {
                "GetListAsync" or "GetAsync" or "GetSelectDataListAsync" => "list",
                "CreateAsync" => "add",
                "UpdateAsync" => "edit",
                "DeleteAsync" => "delete",
                "GetExportExcelAsync" or "ExportAsync" => "export",
                "PostImportExcelAsync" or "ImportAsync" => "import",
                _ => null // 无法识别时不强行推断
            };
        }

        /// <summary>
        /// 判断是否写操作
        /// </summary>
        private bool IsWriteOperation(string actionName)
        {
            return actionName is "add" or "edit" or "delete" or "import" or "export";
        }

        /// <summary>
        /// 推断操作类型
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
                _ => ""
            };

            return $"{actionDesc}{entityName}";
        }

        private string GetCacheKey(ControllerActionDescriptor descriptor)
        {
            return $"{descriptor.ControllerTypeInfo.FullName}:{descriptor.MethodInfo.Name}";
        }
    }
}