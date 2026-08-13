namespace Yi.Module.Rbac.Domain.Shared.Authorization;

/// <summary>
/// 标记实体选择接入部门和创建人维度的数据权限过滤。
/// </summary>
/// <remarks>
/// 该接口是显式接入协议；未实现此接口的部门、角色等权限配置实体不会被业务数据过滤器裁剪。
/// </remarks>
public interface IHasDataPermission
{
    /// <summary>
    /// 数据归属部门。
    /// </summary>
    Guid? DeptId { get; }

    /// <summary>
    /// 数据创建用户，用于判定“仅本人”数据范围。
    /// </summary>
    Guid? CreatorId { get; }
}
