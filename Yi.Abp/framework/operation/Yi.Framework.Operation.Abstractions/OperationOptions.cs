namespace Yi.Framework.Operation.Abstractions
{
    /// <summary>
    /// 操作相关配置
    /// </summary>
    public class OperationOptions
    {
        /// <summary>
        /// 是否自动记录写操作日志
        /// </summary>
        public bool AutoLogWriteOperations { get; set; } = true;

        /// <summary>
        /// 是否记录查询操作日志
        /// </summary>
        public bool LogReadOperations { get; set; } = false;

        /// <summary>
        /// 忽略的 URL 前缀列表
        /// </summary>
        public List<string> IgnoredUrlPrefixes { get; set; } = new();
    }
}