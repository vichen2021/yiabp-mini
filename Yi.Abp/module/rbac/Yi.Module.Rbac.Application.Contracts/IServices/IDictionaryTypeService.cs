using Yi.Framework.Ddd.Application.Contracts;
using Yi.Module.Rbac.Application.Contracts.Dtos.DictionaryType;

namespace Yi.Module.Rbac.Application.Contracts.IServices
{
    /// <summary>
    /// DictionaryType服务抽象
    /// </summary>
    public interface IDictionaryTypeService : IYiCrudAppService<DictionaryTypeGetOutputDto, DictionaryTypeGetListOutputDto, Guid, DictionaryTypeGetListInputVo, DictionaryTypeCreateInputVo, DictionaryTypeUpdateInputVo>
    {

    }
}
