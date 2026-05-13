namespace Yi.Framework.Authorization.Abstractions.Attributes
{
    /// <summary>
    /// 权限资源声明特性。用于服务类或接口，声明权限码前两段。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = true)]
    public sealed class PermissionResourceAttribute : Attribute
    {
        /// <summary>
        /// 模块名：system, monitor 等。
        /// </summary>
        public string Module { get; }

        /// <summary>
        /// 资源名：user, role, dict 等。
        /// </summary>
        public string Resource { get; }

        public PermissionResourceAttribute(string module, string resource)
        {
            Module = module;
            Resource = resource;
        }
    }
}
