using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// ABP <see cref="ISettingStore"/> 的适配器，将 ABP 框架的只读读取请求转发到 <see cref="ISettingManagementStore"/>。
/// ABP 内部的 <c>ISettingProvider</c> 使用此接口读取运行时 Setting 值。
/// </summary>
public class SettingStore : ISettingStore, ITransientDependency
{
    /// <summary>Setting 管理存储，提供实际数据库读写能力。</summary>
    protected ISettingManagementStore ManagementStore { get; }

    /// <summary>注入 <see cref="ISettingManagementStore"/>。</summary>
    public SettingStore(ISettingManagementStore managementStore)
    {
        ManagementStore = managementStore;
    }

    /// <summary>读取单个 Setting 值，委托给 <see cref="ISettingManagementStore.GetOrNullAsync"/>。</summary>
    public virtual Task<string?> GetOrNullAsync(string name, string? providerName, string? providerKey)
    {
        return ManagementStore.GetOrNullAsync(name, providerName, providerKey);
    }

    /// <summary>批量读取 Setting 值列表，委托给 <see cref="ISettingManagementStore.GetListAsync(string[], string, string)"/>。</summary>
    public virtual Task<List<SettingValue>> GetAllAsync(string[] names, string? providerName, string? providerKey)
    {
        return ManagementStore.GetListAsync(names, providerName, providerKey);
    }
}
