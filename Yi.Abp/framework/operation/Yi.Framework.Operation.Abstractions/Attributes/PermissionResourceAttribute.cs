namespace Yi.Framework.Operation.Abstractions.Attributes
{
    /// <summary>
    /// 权限资源声明特性
    /// 用于服务类或接口，声明模块和资源名
    /// 示例: [PermissionResource("system", "user")] -> 生成权限码前缀 system:user
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class PermissionResourceAttribute : Attribute
    {
        /// <summary>
        /// 模块名（权限码第一段）：system, monitor, log 等
        /// </summary>
        public string Module { get; }

        /// <summary>
        /// 资源名（权限码第二段）：user, role, dict 等
        /// </summary>
        public string Resource { get; }

        public PermissionResourceAttribute(string module, string resource)
        {
            Module = module;
            Resource = resource;
        }
    }
}