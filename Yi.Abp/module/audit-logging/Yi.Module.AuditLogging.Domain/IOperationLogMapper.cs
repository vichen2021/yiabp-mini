using Volo.Abp.Auditing;
using Yi.Module.AuditLogging.Domain.Entities;

namespace Yi.Module.AuditLogging.Domain
{
    /// <summary>
    /// 操作记录映射器接口
    /// 从 ABP AuditLogInfo 映射到业务 OperationLogEntity
    /// </summary>
    public interface IOperationLogMapper
    {
        /// <summary>
        /// 尝试从审计记录映射操作记录
        /// 返回 null 表示该请求不需要生成操作记录
        /// </summary>
        OperationLogEntity? TryMap(AuditLogInfo auditLogInfo);
    }
}
