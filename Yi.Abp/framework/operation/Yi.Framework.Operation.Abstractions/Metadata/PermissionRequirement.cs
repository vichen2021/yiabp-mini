namespace Yi.Framework.Operation.Abstractions.Metadata
{
    /// <summary>
    /// 权限要求（权限解析输出）
    /// 仅用于权限校验决策，不包含日志信息
    /// </summary>
    public sealed class PermissionRequirement
    {
        /// <summary>
        /// 是否忽略权限检查 ([IgnorePermission])
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// 是否成功解析权限码
        /// </summary>
        public bool IsResolved { get; set; }

        /// <summary>
        /// 权限码（完整：system:user:add）
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 权限码来源（用于诊断）
        /// 例如："ExplicitPermission", "PermissionAction+Resource", "BaseMethodPermissionAction"
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// 未解析原因（用于诊断日志）
        /// 例如："No PermissionResource on class", "No PermissionAction on method"
        /// </summary>
        public string? UnresolvedReason { get; set; }
    }
}