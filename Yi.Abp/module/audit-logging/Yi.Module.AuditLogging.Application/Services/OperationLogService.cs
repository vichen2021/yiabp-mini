using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.OperationRecord.Abstractions.Attributes;
using Yi.Framework.OperationRecord.Abstractions.Enums;
using Yi.Module.AuditLogging.Application.Contracts.Dtos.OperationLog;
using Yi.Module.AuditLogging.Application.Contracts.IServices;
using Yi.Module.AuditLogging.Domain.Entities;
using Yi.Module.AuditLogging.Domain.Repositories;

namespace Yi.Module.AuditLogging.Application.Services
{
    /// <summary>
    /// 操作记录服务
    /// </summary>
    [PermissionResource("monitor", "operlog")]
    [OperLogEntity("操作记录")]
    public class OperationLogService : ApplicationService, IOperationLogService
    {
        private readonly IOperationLogRepository _repository;

        public OperationLogService(IOperationLogRepository repository)
        {
            _repository = repository;
        }

        [Permission("monitor:operlog:query")]
        public virtual async Task<PagedResultDto<OperationLogGetListOutputDto>> GetListAsync(OperationLogGetListInputVo input)
        {
            var (entities, total) = await _repository.GetFilteredPagedAsync(
                input.OperType, input.OperUser, input.Title, input.IsSuccess,
                input.StartTime, input.EndTime, input.SkipCount, input.MaxResultCount);

            return new PagedResultDto<OperationLogGetListOutputDto>(total, ObjectMapper.Map<List<OperationLogEntity>, List<OperationLogGetListOutputDto>>(entities));
        }

        [Permission("monitor:operlog:query")]
        public virtual async Task<OperationLogGetOutputDto> GetAsync(Guid id)
        {
            var entity = await _repository.GetAsync(id);
            return ObjectMapper.Map<OperationLogEntity, OperationLogGetOutputDto>(entity);
        }

        [Permission("monitor:operlog:remove")]
        [OperLog("删除操作记录", OperEnum.Delete)]
        public virtual async Task DeleteAsync(IEnumerable<Guid> ids)
        {
            await _repository.DeleteManyAsync(ids);
        }

        [Permission("monitor:operlog:remove")]
        [OperLog("清空操作记录", OperEnum.Clear)]
        public virtual async Task DeleteCleanAsync()
        {
            await _repository.DeleteAllAsync();
        }

        [Permission("monitor:operlog:export")]
        [OperLog("导出操作记录", OperEnum.Export)]
        public virtual async Task<IActionResult> PostExportAsync(OperationLogGetListInputVo input)
        {
            var entities = await _repository.GetFilteredListAsync(
                input.OperType, input.OperUser, input.Title, input.IsSuccess,
                input.StartTime, input.EndTime);
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

    }
}
