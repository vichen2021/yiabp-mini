using Volo.Abp.Auditing;
using Yi.Module.AuditLogging.Domain.Entities;

namespace Yi.Module.AuditLogging.Domain
{
    /// <summary>
    /// 操作日志映射器接口
    /// 从 ABP AuditLogInfo 映射到业务 OperationLogEntity
    /// </summary>
    public interface IOperationLogMapper
    {
        /// <summary>
        /// 尝试从审计日志映射操作日志
        /// 返回 null 表示该请求不需要生成操作日志
        /// </summary>
        OperationLogEntity? TryMap(AuditLogInfo auditLogInfo);
    }
}