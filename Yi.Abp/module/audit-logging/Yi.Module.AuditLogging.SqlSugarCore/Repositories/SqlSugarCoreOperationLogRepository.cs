using SqlSugar;
using Yi.Module.AuditLogging.Domain.Entities;
using Yi.Module.AuditLogging.Domain.Repositories;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.SqlSugarCore.Repositories;

namespace Yi.Module.AuditLogging.SqlSugarCore.Repositories
{
    public class SqlSugarCoreOperationLogRepository : SqlSugarRepository<OperationLogEntity, Guid>, IOperationLogRepository
    {
        public SqlSugarCoreOperationLogRepository(ISugarDbContextProvider<ISqlSugarDbContext> sugarDbContextProvider) : base(sugarDbContextProvider)
        {
        }
    }
}