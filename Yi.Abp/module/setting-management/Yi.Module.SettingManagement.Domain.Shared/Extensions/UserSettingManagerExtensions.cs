using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain.Shared;

/// <summary>
/// <see cref="ISettingManager"/> 用户维度的扩展方法，
/// 提供按指定用户 ID 或当前登录用户读写 Setting 的快捷操作。
/// </summary>
public static class UserSettingManagerExtensions
{
    /// <summary>读取指定用户下某个 Setting 的值。</summary>
    public static Task<string> GetOrNullForUserAsync(this ISettingManager settingManager, [NotNull] string name, Guid userId, bool fallback = true)
    {
        return settingManager.GetOrNullAsync(name, UserSettingValueProvider.ProviderName, userId.ToString(), fallback);
    }

    /// <summary>读取当前登录用户下某个 Setting 的值。</summary>
    public static Task<string> GetOrNullForCurrentUserAsync(this ISettingManager settingManager, [NotNull] string name, bool fallback = true)
    {
        return settingManager.GetOrNullAsync(name, UserSettingValueProvider.ProviderName, null, fallback);
    }

    /// <summary>读取指定用户下所有 Setting 的值列表。</summary>
    public static Task<List<SettingValue>> GetAllForUserAsync(this ISettingManager settingManager, Guid userId, bool fallback = true)
    {
        return settingManager.GetAllAsync(UserSettingValueProvider.ProviderName, userId.ToString(), fallback);
    }

    /// <summary>读取当前登录用户下所有 Setting 的值列表。</summary>
    public static Task<List<SettingValue>> GetAllForCurrentUserAsync(this ISettingManager settingManager, bool fallback = true)
    {
        return settingManager.GetAllAsync(UserSettingValueProvider.ProviderName, null, fallback);
    }

    /// <summary>写入指定用户下某个 Setting 的值。</summary>
    public static Task SetForUserAsync(this ISettingManager settingManager, Guid userId, [NotNull] string name, [CanBeNull] string value, bool forceToSet = false)
    {
        return settingManager.SetAsync(name, value, UserSettingValueProvider.ProviderName, userId.ToString(), forceToSet);
    }

    /// <summary>写入当前登录用户下某个 Setting 的值。</summary>
    public static Task SetForCurrentUserAsync(this ISettingManager settingManager, [NotNull] string name, [CanBeNull] string value, bool forceToSet = false)
    {
        return settingManager.SetAsync(name, value, UserSettingValueProvider.ProviderName, null, forceToSet);
    }
}
