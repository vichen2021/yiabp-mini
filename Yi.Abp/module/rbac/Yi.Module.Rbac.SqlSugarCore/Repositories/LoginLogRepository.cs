using SqlSugar;
using Volo.Abp.DependencyInjection;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.SqlSugarCore.Repositories;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Repositories;

namespace Yi.Module.Rbac.SqlSugarCore.Repositories
{
    public class LoginLogRepository : SqlSugarRepository<LoginLogAggregateRoot, Guid>, ILoginLogRepository, ITransientDependency
    {
        public LoginLogRepository(ISugarDbContextProvider<ISqlSugarDbContext> sugarDbContextProvider) : base(sugarDbContextProvider)
        {
        }

        public async Task DeleteAllAsync()
        {
            await _Db.Deleteable<LoginLogAggregateRoot>().ExecuteCommandAsync();
        }

        public async Task<(List<LoginLogAggregateRoot> items, int total)> GetFilteredPagedAsync(
            string? loginIp, string? loginUser,
            DateTime? startTime, DateTime? endTime,
            int skipCount, int maxResultCount)
        {
            RefAsync<int> total = 0;
            var items = await _DbQueryable
                .WhereIF(!string.IsNullOrEmpty(loginIp), x => x.LoginIp.Contains(loginIp!))
                .WhereIF(!string.IsNullOrEmpty(loginUser), x => x.LoginUser!.Contains(loginUser!))
                .WhereIF(startTime is not null && endTime is not null,
                    x => x.CreationTime >= startTime && x.CreationTime <= endTime)
                .OrderByDescending(it => it.CreationTime)
                .ToPageListAsync(skipCount, maxResultCount, total);
            return (items, total);
        }

        public async Task<List<LoginLogAggregateRoot>> GetFilteredListAsync(
            string? loginIp, string? loginUser,
            DateTime? startTime, DateTime? endTime)
        {
            return await _DbQueryable
                .WhereIF(!string.IsNullOrEmpty(loginIp), x => x.LoginIp.Contains(loginIp!))
                .WhereIF(!string.IsNullOrEmpty(loginUser), x => x.LoginUser!.Contains(loginUser!))
                .WhereIF(startTime is not null && endTime is not null,
                    x => x.CreationTime >= startTime && x.CreationTime <= endTime)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
        }
    }
}
