using Volo.Abp.Application.Services;
using Yi.Framework.Ddd.Application.Contracts;
using Yi.Framework.Rbac.Application.Contracts.Dtos.Dept;
using Yi.Framework.Rbac.Domain.Shared.Dtos;

namespace Yi.Framework.Rbac.Application.Contracts.IServices
{
    /// <summary>
    /// Dept服务抽象
    /// </summary>
    public interface IDeptService : IYiCrudAppService<DeptGetOutputDto, DeptGetListOutputDto, Guid, DeptGetListInputVo, DeptCreateInputVo, DeptUpdateInputVo>
    {
        Task<List<Guid>> GetChildListAsync(Guid deptId);
        
        /// <summary>
        /// 获取部门树形列表
        /// </summary>
        /// <returns>树形结构的部门列表</returns>
        Task<List<DeptGetTreeListOutputDto>> GetTreeListAsync();
    }
}
