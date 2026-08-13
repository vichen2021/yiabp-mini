namespace Yi.Module.Rbac.Domain.Shared.Authorization;

/// <summary>
/// 当前用户尚未展开数据库部门关系的数据权限范围。
/// </summary>
public sealed class DataPermissionScopeResult
{
    /// <summary>
    /// 是否拥有全部数据权限；为 <see langword="true"/> 时持久化层不注册数据权限过滤器。
    /// </summary>
    public bool IsAll { get; init; }

    /// <summary>
    /// 是否包含当前用户本人创建的数据。
    /// </summary>
    public bool IncludesSelf { get; init; }

    /// <summary>
    /// 当前用户标识，用于展开“仅本人”范围。
    /// </summary>
    public Guid? CurrentUserId { get; init; }

    /// <summary>
    /// 当前用户所属部门标识；这里只保留声明值，不在领域层查询部门关系。
    /// </summary>
    public Guid? CurrentDeptId { get; init; }

    /// <summary>
    /// 是否包含当前部门的数据。
    /// </summary>
    public bool IncludesCurrentDept { get; init; }

    /// <summary>
    /// 是否需要由持久化层展开当前部门的全部下级部门。
    /// </summary>
    public bool IncludesDescendantDepts { get; init; }

    /// <summary>
    /// 自定义数据范围对应的角色标识；由 SqlSugarCore 层查询角色部门关系后展开为部门集合。
    /// </summary>
    public IReadOnlyCollection<Guid> CustomRoleIds { get; init; } = Array.Empty<Guid>();
}
