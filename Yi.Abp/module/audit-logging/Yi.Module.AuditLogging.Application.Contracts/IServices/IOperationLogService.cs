using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Yi.Module.AuditLogging.Application.Contracts.Dtos.OperationLog;
using Microsoft.AspNetCore.Mvc;

namespace Yi.Module.AuditLogging.Application.Contracts.IServices
{
    public interface IOperationLogService : IApplicationService
    {
        Task<PagedResultDto<OperationLogGetListOutputDto>> GetListAsync(OperationLogGetListInputVo input);
        Task<OperationLogGetOutputDto> GetAsync(Guid id);
        Task DeleteAsync(IEnumerable<Guid> ids);
        Task DeleteCleanAsync();
        Task<IActionResult> PostExportAsync(OperationLogGetListInputVo input);
    }
}
