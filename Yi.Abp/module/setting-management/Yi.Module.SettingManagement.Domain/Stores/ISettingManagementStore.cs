using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// 设置管理存储接口（管理侧），提供 Setting 的完整 CRUD 能力。
/// 供 <see cref="SettingManagementProvider"/> 及 <see cref="SettingManagementStore"/> 使用。
/// </summary>
public interface ISettingManagementStore
{
    /// <summary>
    /// 读取单个 Setting 值，无记录时返回 null。
    /// </summary>
    Task<string?> GetOrNullAsync(string name, string providerName, string providerKey);

    /// <summary>
    /// 读取指定 Provider 维度下的所有 Setting 值列表。
    /// </summary>
    Task<List<SettingValue>> GetListAsync(string providerName, string providerKey);

    /// <summary>
    /// 按名称数组批量读取指定 Provider 维度下的 Setting 值列表。
    /// </summary>
    Task<List<SettingValue>> GetListAsync(string[] names, string providerName, string providerKey);

    /// <summary>
    /// 写入（新增或更新）一个 Setting 值。
    /// </summary>
    Task SetAsync(string name, string value, string providerName, string providerKey);

    /// <summary>
    /// 删除指定 Provider 维度下的一个 Setting 记录。
    /// </summary>
    Task DeleteAsync(string name, string providerName, string providerKey);
}
