using Volo.Abp.Domain.Repositories;
using Yi.Framework.AuditLogging.Domain.Entities;

namespace Yi.Framework.AuditLogging.Domain.Repositories
{
    public interface IOperationLogRepository : IRepository<OperationLogEntity, Guid>
    {
    }
}