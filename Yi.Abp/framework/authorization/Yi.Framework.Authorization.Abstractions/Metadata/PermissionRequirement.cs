namespace Yi.Framework.Authorization.Abstractions.Metadata
{
    /// <summary>
    /// 权限要求。仅用于权限校验决策，不包含操作记录信息。
    /// </summary>
    public sealed class PermissionRequirement
    {
        /// <summary>
        /// 是否忽略权限检查。
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// 是否成功解析权限码。
        /// </summary>
        public bool IsResolved { get; set; }

        /// <summary>
        /// 完整权限码，例如 system:user:add。
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 权限码来源，用于诊断。
        /// </summary>
        public string? Source { get; set; }

        /// <summary>
        /// 未解析原因，用于诊断。
        /// </summary>
        public string? UnresolvedReason { get; set; }
    }
}
