using System.Threading.Tasks;
using Volo.Abp.Auditing;
using Yi.Module.AuditLogging.Domain.Entities;

namespace Yi.Module.AuditLogging.Domain;

public interface IAuditLogInfoToAuditLogConverter
{
    Task<AuditLogAggregateRoot> ConvertAsync(AuditLogInfo auditLogInfo);
}
