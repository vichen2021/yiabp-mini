namespace Yi.Framework.Operation.Abstractions.Permissions
{
    /// <summary>
    /// 权限检查接口
    /// </summary>
    public interface IPermissionHandler
    {
        /// <summary>
        /// 检查当前用户是否拥有指定权限
        /// </summary>
        Task<bool> IsGrantedAsync(string permissionCode);
    }
}