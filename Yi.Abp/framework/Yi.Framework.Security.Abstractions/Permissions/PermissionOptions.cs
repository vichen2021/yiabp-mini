namespace Yi.Framework.Security.Abstractions.Permissions
{
    /// <summary>
    /// 权限配置选项
    /// </summary>
    public class PermissionOptions
    {
        /// <summary>未推断权限码的方法默认拒绝（安全优先）</summary>
        public bool DefaultDenyUnresolved { get; set; } = true;

        /// <summary>允许未推断的方法通过（兼容模式）</summary>
        public bool AllowUnresolvedActions { get; set; } = false;

        /// <summary>白名单方法（无需权限检查）</summary>
        public List<string> WhitelistActions { get; set; } = new();
    }
}