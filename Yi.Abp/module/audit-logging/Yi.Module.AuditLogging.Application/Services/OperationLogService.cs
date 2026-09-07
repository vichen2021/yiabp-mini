using Microsoft.AspNetCore.Mvc;
using MiniExcelLibs;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Yi.Framework.Authorization.Abstractions.Attributes;
using Yi.Framework.Auditing.Abstractions.Attributes;
using Yi.Framework.Auditing.Abstractions.Enums;
using Yi.Module.AuditLogging.Application.Contracts.Dtos.OperationLog;
using Yi.Module.AuditLogging.Application.Contracts.IServices;
using Yi.Module.AuditLogging.Domain;
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
        private readonly IAuditLogRepository _repository;

        /// <summary>
        /// 创建基于统一审计仓储的操作记录服务
        /// </summary>
        /// <param name="repository">统一审计仓储</param>
        public OperationLogService(IAuditLogRepository repository)
        {
            _repository = repository;
        }

        /// <summary>
        /// 从 AuditLogAction 与 AuditLog 联表分页查询业务操作记录
        /// </summary>
        [Permission("monitor:operlog:query")]
        public virtual async Task<PagedResultDto<OperationLogGetListOutputDto>> GetListAsync(OperationLogGetListInputVo input)
        {
            var (entities, total) = await _repository.GetOperationLogPageAsync(
                input.OperType, input.OperUser, input.Title, input.IsSuccess,
                input.StartTime, input.EndTime, input.SkipCount, input.MaxResultCount);

            return new PagedResultDto<OperationLogGetListOutputDto>(total, entities.Select(MapToListOutput).ToList());
        }

        /// <summary>
        /// 按 Action 标识查询业务操作详情
        /// </summary>
        [Permission("monitor:operlog:query")]
        public virtual async Task<OperationLogGetOutputDto> GetAsync(Guid id)
        {
            var entity = await _repository.GetOperationLogAsync(id);
            if (entity == null)
            {
                throw new EntityNotFoundException(typeof(OperationLogRecord), id);
            }

            return MapToOutput(entity);
        }

        /// <summary>
        /// 根据 Action 标识删除其所属的完整审计聚合
        /// </summary>
        [Permission("monitor:operlog:remove")]
        [OperLog("删除操作记录", OperEnum.Delete)]
        public virtual async Task DeleteAsync(IEnumerable<Guid> ids)
        {
            await _repository.DeleteOperationLogsAsync(ids);
        }

        /// <summary>
        /// 清空当前租户包含业务操作的完整审计聚合
        /// </summary>
        [Permission("monitor:operlog:remove")]
        [OperLog("清空操作记录", OperEnum.Clear)]
        public virtual async Task DeleteCleanAsync()
        {
            await _repository.DeleteAllOperationLogsAsync();
        }

        /// <summary>
        /// 导出统一审计模型中的业务操作记录
        /// </summary>
        [Permission("monitor:operlog:export")]
        [OperLog("导出操作记录", OperEnum.Export)]
        public virtual async Task<IActionResult> PostExportAsync(OperationLogGetListInputVo input)
        {
            var entities = await _repository.GetOperationLogListAsync(
                input.OperType, input.OperUser, input.Title, input.IsSuccess,
                input.StartTime, input.EndTime);
            var output = entities.Select(MapToListOutput).ToList();

            var tempPath = Path.Combine(AppContext.BaseDirectory, "temp", "exports");
            if (!Directory.Exists(tempPath))
            {
                Directory.CreateDirectory(tempPath);
            }

            var filePath = Path.Combine(tempPath, $"OperationLog_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{Guid.NewGuid()}.xlsx");
            await MiniExcel.SaveAsAsync(filePath, output);
            return new PhysicalFileResult(filePath, "application/vnd.ms-excel");
        }

        /// <summary>
        /// 将统一审计投影映射为列表 DTO
        /// </summary>
        private static OperationLogGetListOutputDto MapToListOutput(OperationLogRecord record)
        {
            return new OperationLogGetListOutputDto
            {
                Id = record.Id,
                Title = record.Title,
                OperType = record.OperType,
                RequestMethod = record.RequestMethod,
                OperUser = record.OperUser,
                OperIp = record.OperIp,
                Method = record.Method,
                RequestParam = record.RequestParam,
                IsSuccess = record.IsSuccess,
                ErrorMessage = record.ErrorMessage,
                ExecutionDuration = record.ExecutionDuration,
                CreationTime = record.CreationTime
            };
        }

        /// <summary>
        /// 将统一审计投影映射为详情 DTO
        /// </summary>
        private static OperationLogGetOutputDto MapToOutput(OperationLogRecord record)
        {
            return new OperationLogGetOutputDto
            {
                Id = record.Id,
                Title = record.Title,
                OperType = record.OperType,
                RequestMethod = record.RequestMethod,
                OperUser = record.OperUser,
                OperIp = record.OperIp,
                Method = record.Method,
                RequestParam = record.RequestParam,
                IsSuccess = record.IsSuccess,
                ErrorMessage = record.ErrorMessage,
                ExecutionDuration = record.ExecutionDuration,
                CreationTime = record.CreationTime
            };
        }

    }
}
