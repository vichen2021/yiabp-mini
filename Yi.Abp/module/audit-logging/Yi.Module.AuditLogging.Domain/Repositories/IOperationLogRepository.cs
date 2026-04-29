using Volo.Abp.Domain.Repositories;
using Yi.Module.AuditLogging.Domain.Entities;

namespace Yi.Module.AuditLogging.Domain.Repositories
{
    public interface IOperationLogRepository : IRepository<OperationLogEntity, Guid>
    {
    }
}