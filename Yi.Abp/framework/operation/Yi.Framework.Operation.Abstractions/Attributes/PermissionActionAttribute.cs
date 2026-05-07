namespace Yi.Framework.Operation.Abstractions.Attributes
{
    /// <summary>
    /// 权限动作声明特性
    /// 用于方法，声明动作段（权限码第三段）
    /// 示例: [PermissionAction("update")] -> 结合类级资源生成 system:user:update
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class PermissionActionAttribute : Attribute
    {
        /// <summary>
        /// 动作名（权限码第三段）：query, add, edit, remove, export, import 等
        /// </summary>
        public string Action { get; }

        public PermissionActionAttribute(string action)
        {
            Action = action;
        }
    }
}