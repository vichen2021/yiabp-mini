using SqlSugar;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Yi.Framework.AuditLogging.Application.Contracts.Dtos.OperationLog;
using Yi.Framework.AuditLogging.Application.Contracts.IServices;
using Yi.Framework.AuditLogging.Domain.Entities;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.AuditLogging.Application.Services
{
    /// <summary>
    /// 操作日志服务 - 只读服务，不支持创建和更新
    /// </summary>
    public class OperationLogService : ApplicationService, IOperationLogService
    {
        private readonly ISqlSugarRepository<OperationLogEntity, Guid> _repository;

        public OperationLogService(ISqlSugarRepository<OperationLogEntity, Guid> repository)
        {
            _repository = repository;
        }

        public virtual async Task<PagedResultDto<OperationLogGetListOutputDto>> GetListAsync(OperationLogGetListInputVo input)
        {
            RefAsync<int> total = 0;

            var entities = await _repository._DbQueryable
                .WhereIF(input.OperType is not null, x => x.OperType == input.OperType)
                .WhereIF(!string.IsNullOrEmpty(input.OperUser), x => x.OperUser.Contains(input.OperUser!))
                .WhereIF(!string.IsNullOrEmpty(input.Title), x => x.Title.Contains(input.Title!))
                .WhereIF(input.IsSuccess is not null, x => x.IsSuccess == input.IsSuccess)
                .WhereIF(input.StartTime is not null && input.EndTime is not null,
                    x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime)
                .OrderByDescending(x => x.CreationTime)
                .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);

            return new PagedResultDto<OperationLogGetListOutputDto>(total, ObjectMapper.Map<List<OperationLogEntity>, List<OperationLogGetListOutputDto>>(entities));
        }

        public virtual async Task<OperationLogGetOutputDto> GetAsync(Guid id)
        {
            var entity = await _repository.GetAsync(id);
            return ObjectMapper.Map<OperationLogEntity, OperationLogGetOutputDto>(entity);
        }

        public virtual async Task DeleteAsync(IEnumerable<Guid> ids)
        {
            await _repository.DeleteManyAsync(ids);
        }
    }
}