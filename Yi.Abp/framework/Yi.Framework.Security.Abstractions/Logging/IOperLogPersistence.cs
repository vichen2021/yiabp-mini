using Yi.Framework.Security.Abstractions.Enums;

namespace Yi.Framework.Security.Abstractions.Logging
{
    /// <summary>
    /// 操作日志持久化接口（由业务模块实现）
    /// </summary>
    public interface IOperLogPersistence
    {
        /// <summary>
        /// 持久化操作日志
        /// </summary>
        Task PersistAsync(OperLogEntry entry);
    }

    /// <summary>
    /// 操作日志条目（框架层定义，避免依赖具体实体）
    /// </summary>
    public class OperLogEntry
    {
        public string? Title { get; set; }
        public OperEnum OperType { get; set; }
        public string? RequestMethod { get; set; }
        public string? OperUser { get; set; }
        public string? OperIp { get; set; }
        public string? OperLocation { get; set; }
        public string? Method { get; set; }
        public string? RequestParam { get; set; }
        public string? RequestResult { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime ExecutionTime { get; set; }
        public int ExecutionDuration { get; set; }
    }
}