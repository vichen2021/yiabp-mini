using Yi.Framework.Ddd.Application.Contracts;
using Yi.Module.Rbac.Domain.Shared.Enums;

namespace Yi.Module.Rbac.Application.Contracts.Dtos.Notice
{
    /// <summary>
    /// 查询参数
    /// </summary>
    public class NoticeGetListInput : PagedAllResultRequestDto
    {
        public string? Title { get; set; }
        public NoticeTypeEnum? Type { get; set; }
    }
}
