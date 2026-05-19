using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Module.Rbac.Domain.Entities;

namespace Yi.Module.Rbac.Domain.Repositories
{
    public interface ILoginLogRepository : ISqlSugarRepository<LoginLogAggregateRoot, Guid>
    {
        Task DeleteAllAsync();

        Task<(List<LoginLogAggregateRoot> items, int total)> GetFilteredPagedAsync(
            string? loginIp, string? loginUser,
            DateTime? startTime, DateTime? endTime,
            int skipCount, int maxResultCount);

        Task<List<LoginLogAggregateRoot>> GetFilteredListAsync(
            string? loginIp, string? loginUser,
            DateTime? startTime, DateTime? endTime);
    }
}
