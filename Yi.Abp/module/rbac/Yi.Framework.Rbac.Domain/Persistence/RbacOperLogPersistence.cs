using Volo.Abp.DependencyInjection;
using Yi.Framework.SqlSugarCore.Abstractions;
using Yi.Framework.Security.Abstractions.Logging;
using Yi.Framework.Rbac.Domain.Operlog;

namespace Yi.Framework.Rbac.Domain.Persistence
{
    /// <summary>
    /// Rbac模块操作日志持久化实现
    /// </summary>
    public class RbacOperLogPersistence : IOperLogPersistence, ITransientDependency
    {
        private readonly ISqlSugarRepository<OperationLogEntity> _repository;

        public RbacOperLogPersistence(ISqlSugarRepository<OperationLogEntity> repository)
        {
            _repository = repository;
        }

        public async Task PersistAsync(OperLogEntry entry)
        {
            var entity = new OperationLogEntity
            {
                Title = entry.Title,
                OperType = entry.OperType,
                RequestMethod = entry.RequestMethod,
                OperUser = entry.OperUser,
                OperIp = entry.OperIp,
                OperLocation = entry.OperLocation,
                Method = entry.Method,
                RequestParam = entry.RequestParam,
                RequestResult = entry.RequestResult,
                CreationTime = entry.ExecutionTime
            };

            await _repository.InsertAsync(entity);
        }
    }
}