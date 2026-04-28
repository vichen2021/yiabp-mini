using SqlSugar;
using Yi.Framework.AuditLogging.Domain.Entities;
using Yi.Framework.AuditLogging.Domain.Repositories;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.SqlSugarCore.Repositories;

namespace Yi.Framework.AuditLogging.SqlSugarCore.Repositories
{
    public class SqlSugarCoreOperationLogRepository : SqlSugarRepository<OperationLogEntity, Guid>, IOperationLogRepository
    {
        public SqlSugarCoreOperationLogRepository(ISugarDbContextProvider<ISqlSugarDbContext> sugarDbContextProvider) : base(sugarDbContextProvider)
        {
        }
    }
}