using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Yi.Module.SettingManagement.Domain;

public interface ISettingRepository : IBasicRepository<SettingAggregateRoot, Guid>
{
    /// <summary>
    /// 按名称 + Provider 维度精确查找单条 Setting 记录，不存在时返回 null。
    /// </summary>
    Task<SettingAggregateRoot> FindAsync(
        string name,
        string providerName,
        string providerKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 读取指定 Provider 维度下的所有 Setting 记录。
    /// </summary>
    Task<List<SettingAggregateRoot>> GetListAsync(
        string providerName,
        string providerKey,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 按名称数组批量读取指定 Provider 维度下的 Setting 记录。
    /// </summary>
    Task<List<SettingAggregateRoot>> GetListAsync(
        string[] names,
        string providerName,
        string providerKey,
        CancellationToken cancellationToken = default);
}
