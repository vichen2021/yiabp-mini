using Yi.Framework.Authorization.Abstractions.Enums;
using Yi.Framework.Authorization.Abstractions.Extensions;

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

        /// <summary>
        /// 使用字符串声明权限动作（兼容历史代码和自定义动作）
        /// </summary>
        public PermissionActionAttribute(string action)
        {
            Action = action;
        }

        /// <summary>
        /// 使用枚举声明标准权限动作
        /// </summary>
        public PermissionActionAttribute(PermissionActionEnum action)
        {
            Action = action.ToPermissionCode();
        }
    }
}
