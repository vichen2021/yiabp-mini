using Yi.Module.Rbac.Domain.Entities;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.Rbac.Domain.Repositories
{
    public interface IDeptRepository : ISqlSugarRepository<DeptAggregateRoot, Guid>
    {
        Task<List<Guid>> GetChildListAsync(Guid deptId);
        Task<List<DeptAggregateRoot>> GetListRoleIdAsync(Guid roleId);
    }
}
