using Volo.Abp.Application.Dtos;
using Yi.Framework.Operation.Abstractions.Enums;

namespace Yi.Module.AuditLogging.Application.Contracts.Dtos.OperationLog
{
    public class OperationLogGetListOutputDto : EntityDto<Guid>
    {
        public string? Title { get; set; }
        public OperEnum OperType { get; set; }
        public string? RequestMethod { get; set; }
        public string? OperUser { get; set; }
        public string? OperIp { get; set; }
        public string? Method { get; set; }
        public bool IsSuccess { get; set; }
        public DateTime CreationTime { get; set; }
    }
}