using Volo.Abp.Application.Dtos;
using Yi.Framework.AuditLogging.Application.Contracts.Dtos.OperationLog;

namespace Yi.Framework.AuditLogging.Application.Contracts.IServices
{
    public interface IOperationLogService
    {
        Task<PagedResultDto<OperationLogGetListOutputDto>> GetListAsync(OperationLogGetListInputVo input);
        Task<OperationLogGetOutputDto> GetAsync(Guid id);
        Task DeleteAsync(IEnumerable<Guid> ids);
    }
}