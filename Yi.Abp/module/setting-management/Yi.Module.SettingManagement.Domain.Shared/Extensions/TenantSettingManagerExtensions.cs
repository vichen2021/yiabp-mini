using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Volo.Abp.Settings;

namespace Yi.Module.SettingManagement.Domain.Shared;

/// <summary>
/// <see cref="ISettingManager"/> 租户维度的扩展方法，
/// 提供按指定租户 ID 或当前租户上下文读写 Setting 的快捷操作。
/// </summary>
public static class TenantSettingManagerExtensions
{
    /// <summary>读取指定租户下某个 Setting 的值。</summary>
    public static Task<string> GetOrNullForTenantAsync(this ISettingManager settingManager, [NotNull] string name, Guid tenantId, bool fallback = true)
    {
        return settingManager.GetOrNullAsync(name, TenantSettingValueProvider.ProviderName, tenantId.ToString(), fallback);
    }

    /// <summary>读取当前租户上下文下某个 Setting 的值（由 TenantSettingManagementProvider 自动注入租户 ID）。</summary>
    public static Task<string> GetOrNullForCurrentTenantAsync(this ISettingManager settingManager, [NotNull] string name, bool fallback = true)
    {
        return settingManager.GetOrNullAsync(name, TenantSettingValueProvider.ProviderName, null, fallback);
    }

    /// <summary>读取指定租户下所有 Setting 的值列表。</summary>
    public static Task<List<SettingValue>> GetAllForTenantAsync(this ISettingManager settingManager, Guid tenantId, bool fallback = true)
    {
        return settingManager.GetAllAsync(TenantSettingValueProvider.ProviderName, tenantId.ToString(), fallback);
    }

    /// <summary>读取当前租户上下文下所有 Setting 的值列表。</summary>
    public static Task<List<SettingValue>> GetAllForCurrentTenantAsync(this ISettingManager settingManager, bool fallback = true)
    {
        return settingManager.GetAllAsync(TenantSettingValueProvider.ProviderName, null, fallback);
    }

    /// <summary>写入指定租户下某个 Setting 的值。</summary>
    public static Task SetForTenantAsync(this ISettingManager settingManager, Guid tenantId, [NotNull] string name, [CanBeNull] string value, bool forceToSet = false)
    {
        return settingManager.SetAsync(name, value, TenantSettingValueProvider.ProviderName, tenantId.ToString(), forceToSet);
    }

    /// <summary>写入当前租户上下文下某个 Setting 的值。</summary>
    public static Task SetForCurrentTenantAsync(this ISettingManager settingManager, [NotNull] string name, [CanBeNull] string value, bool forceToSet = false)
    {
        return settingManager.SetAsync(name, value, TenantSettingValueProvider.ProviderName, null, forceToSet);
    }

    /// <summary>
    /// 若 <paramref name="tenantId"/> 有值则写入租户维度，否则写入全局维度。
    /// 适用于宿主管理员为任意租户或全局批量配置 Setting 的场景。
    /// </summary>
    public static Task SetForTenantOrGlobalAsync(this ISettingManager settingManager, Guid? tenantId, [NotNull] string name, [CanBeNull] string value, bool forceToSet = false)
    {
        if (tenantId.HasValue)
        {
            return settingManager.SetForTenantAsync(tenantId.Value, name, value, forceToSet);
        }

        return settingManager.SetGlobalAsync(name, value);
    }
}
