using Volo.Abp.DependencyInjection;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.SqlSugarCore.Repositories;
using Yi.Module.TenantManagement.Domain;
using Yi.Module.TenantManagement.Domain.Entities;

namespace Yi.Module.TenantManagement.SqlSugarCore
{
    public class SqlSugarTenantRepository : SqlSugarRepository<TenantAggregateRoot, Guid>, ISqlSugarTenantRepository,ITransientDependency
    {
        public SqlSugarTenantRepository(ISugarDbContextProvider<ISqlSugarDbContext> sugarDbContextProvider) : base(sugarDbContextProvider)
        {
        }

        public async Task<TenantAggregateRoot> FindByNameAsync(string name, bool includeDetails = true)
        {
            return await _DbQueryable.FirstAsync(x => x.Name == name);
        }

        public async Task<long> GetCountAsync(string filter = null)
        {
            return await _DbQueryable.WhereIF(!string.IsNullOrEmpty(filter),x=>x.Name.Contains(filter)) .CountAsync();
        }

        public async Task<List<TenantAggregateRoot>> GetListAsync(string sorting = null, int maxResultCount = int.MaxValue, int skipCount = 0, string filter = null, bool includeDetails = false)
        {


            return await _DbQueryable.WhereIF(!string.IsNullOrEmpty(filter), x => x.Name.Contains(filter))
                .OrderByIF(!string.IsNullOrEmpty(sorting), sorting)
                .ToPageListAsync(skipCount, maxResultCount);
        }

        public Task<bool> DatabaseExistsAsync(string dbName)
        {
            try
            {
                var dbs = _Db.DbMaintenance.GetDataBaseList();
                return Task.FromResult(dbs.Any(x => x?.ToString() == dbName));
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        public Task<int> GetTableCountAsync()
        {
            var tables = _Db.DbMaintenance.GetTableInfoList();
            return Task.FromResult(tables.Count);
        }

        public Task CreateDatabaseAsync(string dbName)
        {
            _Db.DbMaintenance.CreateDatabase(dbName);
            return Task.CompletedTask;
        }

        public Task InitTablesAsync(Type[] entityTypes)
        {
            _Db.CodeFirst.InitTables(entityTypes);
            return Task.CompletedTask;
        }
    }
}
