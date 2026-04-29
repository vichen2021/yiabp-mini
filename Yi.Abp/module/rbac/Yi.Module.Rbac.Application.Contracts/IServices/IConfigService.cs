using Volo.Abp.Application.Services;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Module.Rbac.Application.Contracts.Dtos.Config;

namespace Yi.Module.Rbac.Application.Contracts.IServices
{
    /// <summary>
    /// Config服务抽象
    /// </summary>
    public interface IConfigService : IYiCrudAppService<ConfigGetOutputDto, ConfigGetListOutputDto, Guid, ConfigGetListInputVo, ConfigCreateInputVo, ConfigUpdateInputVo>
    {

    }
}
