using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.Security.Abstractions.Enums;

namespace Yi.Framework.Rbac.Application.Contracts.Dtos.OperLog
{
    public class OperationLogGetListInputVo : PagedAllResultRequestDto
    {
        public OperEnum? OperType { get; set; }
        public string? OperUser { get; set; }
        public string? Title { get; set; }
    }
}
