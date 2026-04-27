using Yi.Framework.Security.Abstractions.Enums;

namespace Yi.Framework.Security.Abstractions.Metadata
{
    /// <summary>
    /// Action元数据结果类
    /// </summary>
    public class ActionMetadata
    {
        /// <summary>模块名：system, monitor, log</summary>
        public string ModuleName { get; set; }

        /// <summary>实体名：user, role, dict</summary>
        public string EntityName { get; set; }

        /// <summary>操作名：list, add, edit, delete, export</summary>
        public string ActionName { get; set; }

        /// <summary>操作类型（用于日志记录）</summary>
        public OperEnum? OperType { get; set; }

        /// <summary>日志标题</summary>
        public string? LogTitle { get; set; }

        /// <summary>是否写操作</summary>
        public bool IsWriteOperation { get; set; }

        /// <summary>推断的权限码：system:user:add</summary>
        public string PermissionCode { get; set; }

        /// <summary>是否成功解析</summary>
        public bool IsResolved { get; set; }

        /// <summary>显式定义的权限码（优先使用）</summary>
        public string? ExplicitPermissionCode { get; set; }

        /// <summary>显式定义的日志信息（优先使用）</summary>
        public (string Title, OperEnum OperType)? ExplicitLogInfo { get; set; }

        /// <summary>是否忽略权限检查</summary>
        public bool IgnorePermission { get; set; }

        /// <summary>是否忽略日志记录</summary>
        public bool IgnoreLog { get; set; }
    }
}