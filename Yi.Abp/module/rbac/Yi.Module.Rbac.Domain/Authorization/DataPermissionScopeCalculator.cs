using Yi.Module.Rbac.Domain.Shared.Authorization;
using Yi.Module.Rbac.Domain.Shared.Enums;
using Yi.Module.Rbac.Domain.Shared.Model;

namespace Yi.Module.Rbac.Domain.Authorization;

/// <summary>
/// 合并令牌中的角色数据范围；该计算器保持纯规则计算，不访问数据库。
/// </summary>
public static class DataPermissionScopeCalculator
{
    /// <summary>
    /// 将当前用户的多个角色数据范围合并为持久化层可展开的声明性范围
    /// </summary>
    /// <param name="currentUserId">当前用户标识</param>
    /// <param name="currentDeptId">当前用户所属部门标识</param>
    /// <param name="roleInfo">令牌中携带的角色及数据范围</param>
    /// <returns>合并后的数据权限范围</returns>
    public static DataPermissionScopeResult Calculate(
        Guid currentUserId,
        Guid? currentDeptId,
        IReadOnlyCollection<RoleTokenInfoModel>? roleInfo)
    {
        // 角色声明缺失时按“仅本人”收紧范围，避免异常或旧令牌意外获得全量数据权限。
        if (roleInfo is null || roleInfo.Count == 0)
        {
            return new DataPermissionScopeResult
            {
                IncludesSelf = true,
                CurrentUserId = currentUserId,
                CurrentDeptId = currentDeptId
            };
        }

        // 多角色采用权限并集，只要任一角色拥有全部范围，就不再生成限制条件。
        if (roleInfo.Any(role => role.DataScope == DataScopeEnum.ALL))
        {
            return new DataPermissionScopeResult { IsAll = true };
        }

        // CUSTOM 这里只保留角色标识，防止范围提供器查询仓储并在 DbContext 初始化期间形成递归依赖。
        return new DataPermissionScopeResult
        {
            CurrentUserId = currentUserId,
            CurrentDeptId = currentDeptId,
            IncludesSelf = roleInfo.Any(role => role.DataScope == DataScopeEnum.USER),
            IncludesCurrentDept = roleInfo.Any(role => role.DataScope is DataScopeEnum.DEPT or DataScopeEnum.DEPT_FOLLOW),
            IncludesDescendantDepts = roleInfo.Any(role => role.DataScope == DataScopeEnum.DEPT_FOLLOW),
            CustomRoleIds = roleInfo
                .Where(role => role.DataScope == DataScopeEnum.CUSTOM)
                .Select(role => role.Id)
                .Distinct()
                .ToArray()
        };
    }
}
