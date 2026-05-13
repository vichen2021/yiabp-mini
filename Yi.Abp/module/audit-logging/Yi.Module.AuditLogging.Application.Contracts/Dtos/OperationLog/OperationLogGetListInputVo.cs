using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.OperationRecord.Abstractions.Enums;

namespace Yi.Module.AuditLogging.Application.Contracts.Dtos.OperationLog
{
    public class OperationLogGetListInputVo : PagedAllResultRequestDto
    {
        public OperEnum? OperType { get; set; }
        public string? OperUser { get; set; }
        public string? Title { get; set; }
        public bool? IsSuccess { get; set; }
    }
}