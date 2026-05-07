using Yi.Framework.Operation.Abstractions.Enums;

namespace Yi.Framework.Operation.Abstractions.Metadata
{
    /// <summary>
    /// 操作日志要求（日志解析输出）
    /// 仅用于日志决策，不包含权限信息
    /// </summary>
    public sealed class OperationLogRequirement
    {
        /// <summary>
        /// 是否忽略日志记录 ([IgnoreOperLog])
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// 是否应该记录日志
        /// </summary>
        public bool ShouldLog { get; set; }

        /// <summary>
        /// 操作类型
        /// </summary>
        public OperEnum? OperType { get; set; }

        /// <summary>
        /// 日志标题
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 是否保存请求数据
        /// </summary>
        public bool SaveRequestData { get; set; }

        /// <summary>
        /// 是否保存响应数据
        /// </summary>
        public bool SaveResponseData { get; set; }

        /// <summary>
        /// 是否为写操作（用于 AutoLogWriteOperations 控制）
        /// </summary>
        public bool IsWriteOperation { get; set; }
    }
}