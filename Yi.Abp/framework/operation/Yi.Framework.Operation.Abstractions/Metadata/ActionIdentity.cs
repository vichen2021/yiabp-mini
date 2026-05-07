using System.Reflection;

namespace Yi.Framework.Operation.Abstractions.Metadata
{
    /// <summary>
    /// Action 身份信息（稳定事实，不含权限或日志决策）
    /// 只解析确定的事实，不判断是否有权限、是否记录日志
    /// </summary>
    public sealed class ActionIdentity
    {
        /// <summary>
        /// 服务类型（Controller 或 ApplicationService）
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// 执行方法
        /// </summary>
        public MethodInfo MethodInfo { get; set; }

        /// <summary>
        /// 模块名（来自 PermissionResource 或 RemoteServiceName 推断）
        /// 未解析时为 null
        /// </summary>
        public string? Module { get; set; }

        /// <summary>
        /// 资源名（来自 PermissionResource 或服务类型名推断）
        /// 未解析时为 null
        /// </summary>
        public string? Resource { get; set; }

        /// <summary>
        /// CRUD 动作名（query, add, edit, remove, export, import）
        /// 仅当为已知 CRUD 方法或有显式 PermissionAction 时有值
        /// </summary>
        public string? CrudAction { get; set; }

        /// <summary>
        /// 是否为已知 CRUD 方法
        /// </summary>
        public bool IsCrudAction { get; set; }

        /// <summary>
        /// ABP RemoteServiceName（来自 Conventional Controller 配置）
        /// </summary>
        public string? RemoteServiceName { get; set; }

        /// <summary>
        /// 是否成功解析（Module 和 Resource 都有值）
        /// </summary>
        public bool IsResolved => !string.IsNullOrEmpty(Module) && !string.IsNullOrEmpty(Resource);

        /// <summary>
        /// 是否有类级 PermissionResource 声明
        /// </summary>
        public bool HasExplicitResource { get; set; }

        /// <summary>
        /// 是否有方法级 PermissionAction 声明
        /// </summary>
        public bool HasExplicitAction { get; set; }

        /// <summary>
        /// 生成缓存 Key
        /// </summary>
        public string GetCacheKey()
        {
            return $"Identity:{ServiceType.FullName}:{MethodInfo.Name}";
        }
    }
}