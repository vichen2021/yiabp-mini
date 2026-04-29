using Yi.Framework.Ddd.Application.Contracts;
using Yi.Module.Rbac.Application.Contracts.Dtos.Dictionary;

namespace Yi.Module.Rbac.Application.Contracts.IServices
{
    /// <summary>
    /// Dictionary服务抽象
    /// </summary>
    public interface IDictionaryService : IYiCrudAppService<DictionaryGetOutputDto, DictionaryGetListOutputDto, Guid, DictionaryGetListInputVo, DictionaryCreateInputVo, DictionaryUpdateInputVo>
    {

    }
}
