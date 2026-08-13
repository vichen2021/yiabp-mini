using Volo.Abp.DependencyInjection;
using Volo.Abp.Users;
using Yi.Module.Rbac.Domain.Extensions;
using Yi.Module.Rbac.Domain.Shared.Authorization;
using Yi.Module.Rbac.Domain.Shared.Consts;

namespace Yi.Module.Rbac.Domain.Authorization;

/// <summary>
/// 从当前用户及令牌声明中解析数据权限范围，不在解析阶段访问数据库。
/// </summary>
public class CurrentUserDataPermissionScopeProvider : IDataPermissionScopeProvider, ITransientDependency
{
    private readonly ICurrentUser _currentUser;

    /// <summary>
    /// 初始化当前用户数据权限范围提供器
    /// </summary>
    /// <param name="currentUser">ABP 当前用户访问器</param>
    public CurrentUserDataPermissionScopeProvider(ICurrentUser currentUser)
    {
        _currentUser = currentUser;
    }

    /// <summary>
    /// 根据当前用户身份及角色声明计算尚未展开部门关系的数据权限范围
    /// </summary>
    /// <returns>当前用户的数据权限范围</returns>
    public DataPermissionScopeResult GetCurrent()
    {
        // 登录、刷新令牌和无用户上下文的系统调用链不能被业务查询过滤器截断；
        // 返回全量仅表示本过滤器不介入，调用入口仍须通过原有认证和功能权限校验。
        if (_currentUser.Id is null || _currentUser.IsRefreshToken())
        {
            return new DataPermissionScopeResult { IsAll = true };
        }

        // 超级管理员沿用系统既有的用户名和角色码双重识别规则，避免历史令牌形态差异造成误过滤。
        if (string.Equals(_currentUser.UserName, UserConst.SuperAdminUserName, StringComparison.OrdinalIgnoreCase)
            || _currentUser.Roles.Any(role => string.Equals(role, UserConst.SuperAdminRoleCode, StringComparison.OrdinalIgnoreCase)))
        {
            return new DataPermissionScopeResult { IsAll = true };
        }

        // 普通用户的多个角色按并集计算；数据库部门树和自定义角色部门留给 SqlSugarCore 层展开。
        return DataPermissionScopeCalculator.Calculate(
            _currentUser.Id.Value,
            _currentUser.GetDeptId(),
            _currentUser.GetRoleInfo());
    }
}
