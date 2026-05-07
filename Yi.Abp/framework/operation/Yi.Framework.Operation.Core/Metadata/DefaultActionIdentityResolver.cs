using System.Reflection;
using Microsoft.AspNetCore.Mvc.Controllers;
using Yi.Framework.Operation.Abstractions.Attributes;
using Yi.Framework.Operation.Abstractions.Metadata;

namespace Yi.Framework.Operation.Core.Metadata
{
    /// <summary>
    /// 默认 Action 身份解析器
    /// 只解析稳定事实（服务类型、方法、模块、资源），不做权限或日志决策
    /// </summary>
    public class DefaultActionIdentityResolver : IActionIdentityResolver
    {
        private readonly ActionIdentityCache _cache;

        public DefaultActionIdentityResolver(ActionIdentityCache cache)
        {
            _cache = cache;
        }

        public ActionIdentity Resolve(ControllerActionDescriptor descriptor)
        {
            var cacheKey = $"MVC:{descriptor.ControllerTypeInfo.FullName}:{descriptor.MethodInfo.Name}";
            if (_cache.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            var identity = ParseIdentity(descriptor.ControllerTypeInfo.AsType(), descriptor.MethodInfo);
            _cache.Set(cacheKey, identity);
            return identity;
        }

        public ActionIdentity Resolve(Type serviceType, MethodInfo methodInfo)
        {
            var cacheKey = $"Service:{serviceType.FullName}:{methodInfo.Name}";
            if (_cache.TryGetValue(cacheKey, out var cached) && cached != null)
            {
                return cached;
            }

            var identity = ParseIdentity(serviceType, methodInfo);
            _cache.Set(cacheKey, identity);
            return identity;
        }

        private ActionIdentity ParseIdentity(Type serviceType, MethodInfo methodInfo)
        {
            var identity = new ActionIdentity
            {
                ServiceType = serviceType,
                MethodInfo = methodInfo
            };

            // 1. 解析类级 PermissionResource
            var resourceAttr = serviceType.GetCustomAttribute<PermissionResourceAttribute>();
            if (resourceAttr != null)
            {
                identity.Module = resourceAttr.Module;
                identity.Resource = resourceAttr.Resource;
                identity.HasExplicitResource = true;
            }

            // 2. 解析方法级 PermissionAction
            var actionAttr = methodInfo.GetCustomAttribute<PermissionActionAttribute>();
            if (actionAttr != null)
            {
                identity.CrudAction = actionAttr.Action;
                identity.HasExplicitAction = true;
            }

            // 3. 识别 CRUD 方法（仅当无显式 PermissionAction 时）
            if (!identity.HasExplicitAction)
            {
                identity.CrudAction = InferCrudAction(methodInfo.Name);
                identity.IsCrudAction = identity.CrudAction != null;
            }

            // 4. 推断 Resource（仅当无显式声明时）
            if (!identity.HasExplicitResource)
            {
                identity.Resource = InferResourceName(serviceType);
            }

            // 5. 推断 Module（仅当无显式声明时）
            if (!identity.HasExplicitResource)
            {
                identity.Module = InferModuleName(serviceType);
            }

            // 6. 解析 RemoteServiceName（从 ABP 配置）
            identity.RemoteServiceName = GetRemoteServiceName(serviceType);

            return identity;
        }

        /// <summary>
        /// 推断 CRUD 动作（仅识别确定的 CRUD 方法）
        /// </summary>
        private string? InferCrudAction(string methodName)
        {
            return methodName switch
            {
                // 标准查询
                "GetListAsync" or "GetSelectDataListAsync" => "query",
                "GetAsync" => "query",

                // 标准写入
                "CreateAsync" => "add",
                "UpdateAsync" => "edit",

                // DeleteAsync 是特殊情况：基类有两个 DeleteAsync 方法
                // 单参数 DeleteAsync(TKey) 和批量 DeleteAsync(IEnumerable<TKey>)
                // 都映射为 remove
                "DeleteAsync" => "remove",

                // Excel
                "GetExportExcelAsync" => "export",
                "PostImportExcelAsync" => "import",

                // 明确动作前缀推断
                _ => InferByPrefix(methodName)
            };
        }

        /// <summary>
        /// 前缀推断（仅保留明确语义的前缀）
        /// </summary>
        private string? InferByPrefix(string methodName)
        {
            if (methodName.StartsWith("Create") || methodName.StartsWith("Add") || methodName.StartsWith("Insert"))
                return "add";
            if (methodName.StartsWith("Update") || methodName.StartsWith("Edit") || methodName.StartsWith("Modify"))
                return "edit";
            if (methodName.StartsWith("Delete") || methodName.StartsWith("Remove") || methodName.StartsWith("Clear"))
                return "remove";
            if (methodName.StartsWith("Export"))
                return "export";
            if (methodName.StartsWith("Import"))
                return "import";

            return null;
        }

        /// <summary>
        /// 推断资源名（从服务类型名）
        /// </summary>
        private string? InferResourceName(Type serviceType)
        {
            var serviceName = serviceType.Name;
            if (serviceName.EndsWith("Service"))
            {
                serviceName = serviceName.Substring(0, serviceName.Length - 7);
            }

            return EntityNameNormalizer.Normalize(serviceName);
        }

        /// <summary>
        /// 推断模块名（从 RemoteServiceName 或命名空间）
        /// 注意：这是 fallback，优先使用 PermissionResource
        /// </summary>
        private string? InferModuleName(Type serviceType)
        {
            // 1. 从 RemoteServiceName 推断
            var remoteServiceName = GetRemoteServiceName(serviceType);
            if (!string.IsNullOrEmpty(remoteServiceName))
            {
                // kebab-case 转 camelCase 或保持原样
                // audit-logging -> monitor (需要映射)
                // tenant-management -> system (需要映射)
                return MapRemoteServiceNameToModule(remoteServiceName);
            }

            // 2. 从命名空间推断（Legacy，Phase 5 删除）
            var ns = serviceType.Namespace ?? "";
            if (ns.Contains("Yi.Module.Rbac")) return "system";
            if (ns.Contains("Yi.Module.TenantManagement")) return "system";
            if (ns.Contains("Yi.Module.AuditLogging")) return "monitor";
            if (ns.Contains("Yi.Module.SettingManagement")) return "system";

            // 3. Framework 服务
            if (ns.Contains(".TenantManagement")) return "system";
            if (ns.Contains(".Services.System")) return "system";
            if (ns.Contains(".Services.Monitor")) return "monitor";
            if (ns.Contains(".AuditLogging")) return "monitor";

            // 4. 未解析返回 null（Phase 5 后不再有默认 system）
            return null;
        }

        /// <summary>
        /// RemoteServiceName 到权限模块前缀的映射
        /// </summary>
        private string? MapRemoteServiceNameToModule(string remoteServiceName)
        {
            return remoteServiceName switch
            {
                "rbac" => "system",
                "tenant-management" => "system",
                "audit" => "monitor",
                "setting-management" => "system",
                _ => remoteServiceName.Replace("-", "") // fallback：去除 kebab-case
            };
        }

        /// <summary>
        /// 获取 ABP RemoteServiceName（从类型配置推断）
        /// </summary>
        private string? GetRemoteServiceName(Type serviceType)
        {
            // ABP 动态控制器配置的 RemoteServiceName 通常通过 Assembly 配置
            // 这里从类型所在 Assembly 的约定推断
            var assembly = serviceType.Assembly;
            var assemblyName = assembly.GetName().Name;

            if (assemblyName == null) return null;

            // Yi.Module.Rbac.Application -> rbac
            // Yi.Module.AuditLogging.Application -> audit
            if (assemblyName.StartsWith("Yi.Module."))
            {
                var modulePart = assemblyName.Substring("Yi.Module.".Length);
                if (modulePart.Contains(".Application"))
                {
                    modulePart = modulePart.Substring(0, modulePart.IndexOf(".Application"));
                }
                return modulePart.ToLowerInvariant();
            }

            return null;
        }
    }
}