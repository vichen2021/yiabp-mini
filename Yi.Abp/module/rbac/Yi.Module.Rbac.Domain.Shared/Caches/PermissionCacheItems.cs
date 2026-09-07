namespace Yi.Module.Rbac.Domain.Shared.Caches;

/// <summary>
/// 租户权限缓存版本
/// </summary>
public sealed class PermissionCacheVersionItem
{
    public string Version { get; set; } = string.Empty;
}

/// <summary>
/// 租户权限缓存版本键
/// </summary>
public sealed class PermissionCacheVersionKey
{
    /// <summary>
    /// 创建宿主或租户隔离的权限缓存版本键
    /// </summary>
    /// <param name="tenantId">租户标识，空值表示宿主</param>
    public PermissionCacheVersionKey(Guid? tenantId)
    {
        TenantId = tenantId;
    }

    public Guid? TenantId { get; set; }

    /// <summary>
    /// 生成宿主或租户隔离的缓存键
    /// </summary>
    public override string ToString()
    {
        return $"PermissionVersion:{TenantId?.ToString("N") ?? "host"}";
    }
}

/// <summary>
/// 用户权限授权快照
/// </summary>
public sealed class PermissionGrantCacheItem
{
    public bool GrantAll { get; set; }

    public string[] PermissionCodes { get; set; } = [];
}

/// <summary>
/// 用户权限授权快照键
/// </summary>
public sealed class PermissionGrantCacheKey
{
    /// <summary>
    /// 创建租户、用户和授权版本共同隔离的权限快照键
    /// </summary>
    /// <param name="tenantId">租户标识，空值表示宿主</param>
    /// <param name="userId">用户标识</param>
    /// <param name="version">权限缓存版本</param>
    public PermissionGrantCacheKey(Guid? tenantId, Guid userId, string version)
    {
        TenantId = tenantId;
        UserId = userId;
        Version = version;
    }

    public Guid? TenantId { get; set; }

    public Guid UserId { get; set; }

    public string Version { get; set; }

    /// <summary>
    /// 生成租户、用户和授权版本共同隔离的缓存键
    /// </summary>
    public override string ToString()
    {
        return $"PermissionGrant:{TenantId?.ToString("N") ?? "host"}:{UserId:N}:{Version}";
    }
}
