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

        public async Task<long> GetCountAsync(string? filter = null)
        {
            return await _DbQueryable.WhereIF(!string.IsNullOrEmpty(filter),x=>x.Name.Contains(filter)) .CountAsync();
        }

        public async Task<List<TenantAggregateRoot>> GetListAsync(string? sorting = null, int maxResultCount = int.MaxValue, int skipCount = 0, string? filter = null, bool includeDetails = false)
        {


            return await _DbQueryable.WhereIF(!string.IsNullOrEmpty(filter), x => x.Name.Contains(filter))
                .OrderByIF(!string.IsNullOrEmpty(sorting), sorting)
                .ToPageListAsync(skipCount, maxResultCount);
        }

        public Task<bool> DatabaseExistsAsync(TenantAggregateRoot tenant)
        {
            try
            {
                if (tenant.DbType == SqlSugar.DbType.Sqlite)
                {
                    var dataSource = GetSqliteDataSource(tenant.TenantConnectionString);
                    return Task.FromResult(!string.IsNullOrWhiteSpace(dataSource) && File.Exists(dataSource));
                }

                var dbs = _Db.DbMaintenance.GetDataBaseList();
                return Task.FromResult(dbs.Any(x => x?.ToString() == tenant.Name));
            }
            catch
            {
                return Task.FromResult(false);
            }
        }

        private static string? GetSqliteDataSource(string connectionString)
        {
            var dataSource = connectionString
                .Split(';', StringSplitOptions.RemoveEmptyEntries)
                .Select(part => part.Split('=', 2))
                .Where(parts => parts.Length == 2)
                .FirstOrDefault(parts =>
                    parts[0].Trim().Equals("Data Source", StringComparison.OrdinalIgnoreCase) ||
                    parts[0].Trim().Equals("DataSource", StringComparison.OrdinalIgnoreCase) ||
                    parts[0].Trim().Equals("Filename", StringComparison.OrdinalIgnoreCase))?[1]
                .Trim();

            return string.IsNullOrWhiteSpace(dataSource)
                ? null
                : Path.GetFullPath(dataSource);
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
