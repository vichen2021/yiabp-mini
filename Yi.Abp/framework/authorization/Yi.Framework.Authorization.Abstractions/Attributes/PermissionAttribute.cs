namespace Yi.Framework.Authorization.Abstractions.Attributes
{
    /// <summary>
    /// 完整权限码标记特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class PermissionAttribute : Attribute
    {
        /// <summary>
        /// 权限码。
        /// </summary>
        public string Code { get; }

        public PermissionAttribute(string code)
        {
            Code = code;
        }
    }
}
