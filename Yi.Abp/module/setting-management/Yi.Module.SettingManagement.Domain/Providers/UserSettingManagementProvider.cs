using Volo.Abp.DependencyInjection;
using Volo.Abp.Settings;
using Volo.Abp.Users;

namespace Yi.Module.SettingManagement.Domain;

/// <summary>
/// 用户级 Setting Provider，优先级最高。
/// <see cref="NormalizeProviderKey"/> 使用当前登录用户 ID 作为 Key。
/// </summary>
public class UserSettingManagementProvider : SettingManagementProvider, ITransientDependency
{
    /// <inheritdoc/>
    public override string Name => UserSettingValueProvider.ProviderName;

    /// <summary>当前登录用户上下文。</summary>
    protected ICurrentUser CurrentUser { get; }

    /// <summary>注入 Store 和用户上下文。</summary>
    public UserSettingManagementProvider(
        ISettingManagementStore settingManagementStore,
        ICurrentUser currentUser)
        : base(settingManagementStore)
    {
        CurrentUser = currentUser;
    }

    /// <summary>
    /// 若传入了显式 Key 则优先使用；否则取当前登录用户 ID。
    /// </summary>
    protected override string? NormalizeProviderKey(string? providerKey)
    {
        if (providerKey != null)
        {
            return providerKey;
        }

        return CurrentUser.Id?.ToString();
    }
}
