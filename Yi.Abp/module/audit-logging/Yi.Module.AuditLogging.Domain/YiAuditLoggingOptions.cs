namespace Yi.Module.AuditLogging.Domain
{
    /// <summary>
    /// Yi 审计记录配置选项
    /// Yi 开关负责"存不存表"
    /// </summary>
    public class YiAuditLoggingOptions
    {
        /// <summary>
        /// 是否保存详细审计记录
        /// 默认 false：不保存详细审计记录，减少数据量
        /// </summary>
        public bool SaveAuditLog { get; set; } = false;

        /// <summary>
        /// 是否保存业务操作记录
        /// 默认 true：保存写操作的业务日志
        /// </summary>
        public bool SaveOperationLog { get; set; } = true;

        /// <summary>
        /// 请求参数最大长度
        /// </summary>
        public int MaxRequestParamLength { get; set; } = 2000;

        /// <summary>
        /// 响应数据最大长度
        /// </summary>
        public int MaxResponseDataLength { get; set; } = 2000;

        /// <summary>
        /// 是否保存请求参数
        /// </summary>
        public bool SaveRequestData { get; set; } = true;

        /// <summary>
        /// 是否保存响应数据
        /// </summary>
        public bool SaveResponseData { get; set; } = false;

        /// <summary>
        /// 是否自动写入写操作记录
        /// </summary>
        public bool AutoLogWriteOperations { get; set; } = true;

        /// <summary>
        /// 是否写入查询操作记录
        /// </summary>
        public bool LogReadOperations { get; set; } = false;

        /// <summary>
        /// 操作记录忽略的 URL 前缀列表
        /// </summary>
        public List<string> IgnoredUrlPrefixes { get; set; } = new();
    }
}
