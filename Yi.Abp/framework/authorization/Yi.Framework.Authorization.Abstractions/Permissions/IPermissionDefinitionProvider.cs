namespace Yi.Framework.Authorization.Abstractions.Permissions
{
    /// <summary>
    /// 后端权限定义提供器。
    /// </summary>
    public interface IPermissionDefinitionProvider
    {
        /// <summary>
        /// 获取按资源聚合的权限定义。
        /// </summary>
        IReadOnlyList<PermissionResourceDefinition> GetResources();

        /// <summary>
        /// 获取完整权限码集合。
        /// </summary>
        IReadOnlySet<string> GetPermissionCodes();
    }
}
