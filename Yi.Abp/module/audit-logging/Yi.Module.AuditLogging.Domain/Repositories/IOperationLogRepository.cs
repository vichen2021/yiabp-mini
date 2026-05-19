using Yi.Framework.OperationRecord.Abstractions.Enums;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Module.AuditLogging.Domain.Entities;

namespace Yi.Module.AuditLogging.Domain.Repositories
{
    public interface IOperationLogRepository : ISqlSugarRepository<OperationLogEntity, Guid>
    {
        Task DeleteAllAsync();

        Task<(List<OperationLogEntity> items, int total)> GetFilteredPagedAsync(
            OperEnum? operType, string? operUser, string? title, bool? isSuccess,
            DateTime? startTime, DateTime? endTime, int skipCount, int maxResultCount);

        Task<List<OperationLogEntity>> GetFilteredListAsync(
            OperEnum? operType, string? operUser, string? title, bool? isSuccess,
            DateTime? startTime, DateTime? endTime);
    }
}