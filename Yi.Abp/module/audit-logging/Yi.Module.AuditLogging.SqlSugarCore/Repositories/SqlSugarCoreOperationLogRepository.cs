using SqlSugar;
using Yi.Framework.OperationRecord.Abstractions.Enums;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.SqlSugarCore.Repositories;
using Yi.Module.AuditLogging.Domain.Entities;
using Yi.Module.AuditLogging.Domain.Repositories;

namespace Yi.Module.AuditLogging.SqlSugarCore.Repositories
{
    public class SqlSugarCoreOperationLogRepository : SqlSugarRepository<OperationLogEntity, Guid>, IOperationLogRepository
    {
        public SqlSugarCoreOperationLogRepository(ISugarDbContextProvider<ISqlSugarDbContext> sugarDbContextProvider) : base(sugarDbContextProvider)
        {
        }

        public async Task DeleteAllAsync()
        {
            await _Db.Deleteable<OperationLogEntity>().ExecuteCommandAsync();
        }

        public async Task<(List<OperationLogEntity> items, int total)> GetFilteredPagedAsync(
            OperEnum? operType, string? operUser, string? title, bool? isSuccess,
            DateTime? startTime, DateTime? endTime, int skipCount, int maxResultCount)
        {
            RefAsync<int> total = 0;
            var items = await _DbQueryable
                .WhereIF(operType is not null, x => x.OperType == operType)
                .WhereIF(!string.IsNullOrEmpty(operUser), x => x.OperUser.Contains(operUser!))
                .WhereIF(!string.IsNullOrEmpty(title), x => x.Title.Contains(title!))
                .WhereIF(isSuccess is not null, x => x.IsSuccess == isSuccess)
                .WhereIF(startTime is not null && endTime is not null,
                    x => x.CreationTime >= startTime && x.CreationTime <= endTime)
                .OrderByDescending(x => x.CreationTime)
                .ToPageListAsync(skipCount, maxResultCount, total);
            return (items, total);
        }

        public async Task<List<OperationLogEntity>> GetFilteredListAsync(
            OperEnum? operType, string? operUser, string? title, bool? isSuccess,
            DateTime? startTime, DateTime? endTime)
        {
            return await _DbQueryable
                .WhereIF(operType is not null, x => x.OperType == operType)
                .WhereIF(!string.IsNullOrEmpty(operUser), x => x.OperUser.Contains(operUser!))
                .WhereIF(!string.IsNullOrEmpty(title), x => x.Title.Contains(title!))
                .WhereIF(isSuccess is not null, x => x.IsSuccess == isSuccess)
                .WhereIF(startTime is not null && endTime is not null,
                    x => x.CreationTime >= startTime && x.CreationTime <= endTime)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }
    }
}