using Volo.Abp.Application.Dtos;
using Yi.Framework.OperationRecord.Abstractions.Enums;

namespace Yi.Module.AuditLogging.Application.Contracts.Dtos.OperationLog
{
    public class OperationLogGetOutputDto : EntityDto<Guid>
    {
        public string? Title { get; set; }
        public OperEnum OperType { get; set; }
        public string? RequestMethod { get; set; }
        public string? OperUser { get; set; }
        public string? OperIp { get; set; }
        public string? OperLocation { get; set; }
        public string? Method { get; set; }
        public string? RequestParam { get; set; }
        public string? RequestResult { get; set; }
        public bool IsSuccess { get; set; }
        public string? ErrorMessage { get; set; }
        public DateTime CreationTime { get; set; }
    }
}