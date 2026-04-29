namespace Yi.Framework.Operation.Abstractions.Attributes
{
    /// <summary>
    /// 忽略权限检查特性
    /// 支持类级和方法级
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IgnorePermissionAttribute : Attribute
    {
    }
}