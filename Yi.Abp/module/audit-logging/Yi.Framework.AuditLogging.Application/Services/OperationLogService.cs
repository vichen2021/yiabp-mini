using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using SqlSugar;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Yi.Framework.AuditLogging.Application.Contracts.Dtos.OperationLog;
using Yi.Framework.AuditLogging.Application.Contracts.IServices;
using Yi.Framework.AuditLogging.Domain.Entities;
using Yi.Framework.Operation.Abstractions.Attributes;
using Yi.Framework.Operation.Abstractions.Enums;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Framework.AuditLogging.Application.Services
{
    /// <summary>
    /// 操作日志服务
    /// </summary>
    [OperLogEntity("操作日志")]
    public class OperationLogService : ApplicationService, IOperationLogService
    {
        private readonly ISqlSugarRepository<OperationLogEntity, Guid> _repository;

        public OperationLogService(ISqlSugarRepository<OperationLogEntity, Guid> repository)
        {
            _repository = repository;
        }

        [Permission("monitor:operlog:list")]
        public virtual async Task<PagedResultDto<OperationLogGetListOutputDto>> GetListAsync(OperationLogGetListInputVo input)
        {
            RefAsync<int> total = 0;

            var entities = await BuildQuery(input)
                .OrderByDescending(x => x.CreationTime)
                .ToPageListAsync(input.SkipCount, input.MaxResultCount, total);

            return new PagedResultDto<OperationLogGetListOutputDto>(total, ObjectMapper.Map<List<OperationLogEntity>, List<OperationLogGetListOutputDto>>(entities));
        }

        [Permission("monitor:operlog:query")]
        public virtual async Task<OperationLogGetOutputDto> GetAsync(Guid id)
        {
            var entity = await _repository.GetAsync(id);
            return ObjectMapper.Map<OperationLogEntity, OperationLogGetOutputDto>(entity);
        }

        [Permission("monitor:operlog:remove")]
        [OperLog("删除操作日志", OperEnum.Delete)]
        public virtual async Task DeleteAsync(IEnumerable<Guid> ids)
        {
            await _repository.DeleteManyAsync(ids);
        }

        [Permission("monitor:operlog:remove")]
        [OperLog("清空操作日志", OperEnum.Clear)]
        public virtual async Task DeleteCleanAsync()
        {
            await _repository._Db.Deleteable<OperationLogEntity>().ExecuteCommandAsync();
        }

        [Permission("monitor:operlog:export")]
        [OperLog("导出操作日志", OperEnum.Export)]
        public virtual async Task<IActionResult> PostExportAsync(OperationLogGetListInputVo input)
        {
            var entities = await BuildQuery(input)
                .OrderByDescending(x => x.CreationTime)
                .ToListAsync();
            var output = ObjectMapper.Map<List<OperationLogEntity>, List<OperationLogGetListOutputDto>>(entities);

            var tempPath = Path.Combine(AppContext.BaseDirectory, "temp", "exports");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var filePath = Path.Combine(tempPath, $"OperationLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Guid.NewGuid()}.xlsx");
            await MiniExcel.SaveAsAsync(filePath, output);
            return new PhysicalFileResult(filePath, "application/vnd.ms-excel");
        }

        private ISugarQueryable<OperationLogEntity> BuildQuery(OperationLogGetListInputVo input)
        {
            return _repository._DbQueryable
                .WhereIF(input.OperType is not null, x => x.OperType == input.OperType)
                .WhereIF(!string.IsNullOrEmpty(input.OperUser), x => x.OperUser.Contains(input.OperUser!))
                .WhereIF(!string.IsNullOrEmpty(input.Title), x => x.Title.Contains(input.Title!))
                .WhereIF(input.IsSuccess is not null, x => x.IsSuccess == input.IsSuccess)
                .WhereIF(input.StartTime is not null && input.EndTime is not null,
                    x => x.CreationTime >= input.StartTime && x.CreationTime <= input.EndTime);
        }
    }
}