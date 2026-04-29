using Yi.Framework.Ddd.Application.Contracts;
using Yi.Module.Rbac.Application.Contracts.Dtos.Notice;

namespace Yi.Module.Rbac.Application.Contracts.IServices
{
    /// <summary>
    /// Notice服务抽象
    /// </summary>
    public interface INoticeService : IYiCrudAppService<NoticeGetOutputDto, NoticeGetListOutputDto, Guid, NoticeGetListInput, NoticeCreateInput, NoticeUpdateInput>
    {

    }
}
