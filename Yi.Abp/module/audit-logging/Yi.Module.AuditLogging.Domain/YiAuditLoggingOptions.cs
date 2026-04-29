namespace Yi.Module.AuditLogging.Domain
{
    /// <summary>
    /// Yi 审计日志配置选项
    /// Yi 开关负责"存不存表"
    /// </summary>
    public class YiAuditLoggingOptions
    {
        /// <summary>
        /// 是否保存详细审计日志
        /// 默认 false：不保存详细审计日志，减少日志量
        /// </summary>
        public bool SaveAuditLog { get; set; } = false;

        /// <summary>
        /// 是否保存业务操作日志
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
    }
}