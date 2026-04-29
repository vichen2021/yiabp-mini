using Volo.Abp.Domain.Repositories;
using Yi.Module.Rbac.Domain.Entities;
using Yi.Module.Rbac.Domain.Shared.Dtos;
using Yi.Framework.SqlSugarCore.Abstractions;

namespace Yi.Module.Rbac.Domain.Repositories
{
    public interface IUserRepository : ISqlSugarRepository<UserAggregateRoot>
    {
        /// <summary>
        /// 获取用户的所有信息
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        Task<UserAggregateRoot> GetUserAllInfoAsync(Guid userId);
        /// <summary>
        /// 批量获取用户的所有信息
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        Task<List<UserAggregateRoot>> GetListUserAllInfoAsync(List<Guid> userIds);

    }
}
