using System.Threading.Tasks;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// 有状态类 Provider 的抽象基类，封装对 <see cref="ISettingManagementStore"/> 的 Get/Set/Delete 调用。
/// 子类只需覆写 <see cref="NormalizeProviderKey"/> 来提供本维度的 Key 解析逻辑。
/// </summary>
public abstract class SettingManagementProvider : ISettingManagementProvider
{
    /// <inheritdoc/>
    public abstract string Name { get; }

    /// <summary>Setting 管理存储，负责底层数据库读写。</summary>
    protected ISettingManagementStore SettingManagementStore { get; }

    /// <summary>注入 <see cref="ISettingManagementStore"/>。</summary>
    protected SettingManagementProvider(ISettingManagementStore settingManagementStore)
    {
        SettingManagementStore = settingManagementStore;
    }

    /// <summary>
    /// 读取当前 Provider 维度下的 Setting 值，无则返回 <c>null</c>。
    /// </summary>
    public virtual async Task<string?> GetOrNullAsync(SettingDefinition setting, string? providerKey)
    {
        return await SettingManagementStore.GetOrNullAsync(setting.Name, Name, NormalizeProviderKey(providerKey));
    }

    /// <summary>
    /// 写入当前 Provider 维度下的 Setting 值。
    /// </summary>
    public virtual async Task SetAsync(SettingDefinition setting, string value, string? providerKey)
    {
        await SettingManagementStore.SetAsync(setting.Name, value, Name, NormalizeProviderKey(providerKey));
    }

    /// <summary>
    /// 删除当前 Provider 维度下的 Setting 值。
    /// </summary>
    public virtual async Task ClearAsync(SettingDefinition setting, string? providerKey)
    {
        await SettingManagementStore.DeleteAsync(setting.Name, Name, NormalizeProviderKey(providerKey));
    }

    /// <summary>
    /// 解析当前 Provider 维度的 Key。
    /// 默认直接返回传入的 <paramref name="providerKey"/>；
    /// 子类可覆写为从当前上下文（如租户/用户）自动提取。
    /// </summary>
    protected virtual string? NormalizeProviderKey(string? providerKey)
    {
        return providerKey;
    }
}
