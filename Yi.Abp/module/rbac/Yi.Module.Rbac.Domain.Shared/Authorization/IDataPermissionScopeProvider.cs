namespace Yi.Module.Rbac.Domain.Shared.Authorization;

/// <summary>
/// 提供当前用户的数据权限范围。
/// </summary>
public interface IDataPermissionScopeProvider
{
    /// <summary>
    /// 获取当前调用用户尚未展开数据库关系的数据权限范围
    /// </summary>
    /// <remarks>
    /// DbContext 的自定义过滤钩子是同步调用，因此实现只能读取当前用户及令牌声明，不能在此查询数据库
    /// </remarks>
    /// <returns>当前用户的数据权限范围</returns>
    DataPermissionScopeResult GetCurrent();
}
