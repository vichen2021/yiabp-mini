namespace Yi.Framework.Authorization.Abstractions.Attributes
{
    /// <summary>
    /// 权限动作声明特性。用于方法，声明权限码第三段。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class PermissionActionAttribute : Attribute
    {
        /// <summary>
        /// 动作名：query, add, edit, remove, export, import 等。
        /// </summary>
        public string Action { get; }

        public PermissionActionAttribute(string action)
        {
            Action = action;
        }
    }
}
