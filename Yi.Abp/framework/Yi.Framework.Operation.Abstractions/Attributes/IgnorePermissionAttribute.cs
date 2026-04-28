namespace Yi.Framework.Operation.Abstractions.Attributes
{
    /// <summary>
    /// 忽略权限检查特性
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnorePermissionAttribute : Attribute
    {
    }
}