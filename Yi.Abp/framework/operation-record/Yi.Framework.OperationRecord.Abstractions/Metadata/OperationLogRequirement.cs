using Yi.Framework.OperationRecord.Abstractions.Enums;

namespace Yi.Framework.OperationRecord.Abstractions.Metadata
{
    /// <summary>
    /// 操作记录要求。仅用于操作记录决策，不包含权限信息。
    /// </summary>
    public sealed class OperationLogRequirement
    {
        /// <summary>
        /// 是否忽略操作记录。
        /// </summary>
        public bool Ignore { get; set; }

        /// <summary>
        /// 是否应该写入操作记录。
        /// </summary>
        public bool ShouldLog { get; set; }

        /// <summary>
        /// 操作类型。
        /// </summary>
        public OperEnum? OperType { get; set; }

        /// <summary>
        /// 操作记录标题。
        /// </summary>
        public string? Title { get; set; }

        /// <summary>
        /// 是否保存请求数据。
        /// </summary>
        public bool SaveRequestData { get; set; }

        /// <summary>
        /// 是否保存响应数据。
        /// </summary>
        public bool SaveResponseData { get; set; }

        /// <summary>
        /// 是否为写操作。
        /// </summary>
        public bool IsWriteOperation { get; set; }

        /// <summary>
        /// 解析来源，例如 IgnoreOperLog、OperLog。
        /// </summary>
        public string? Source { get; set; }
    }
}
