namespace Yi.Framework.Operation.Abstractions.Attributes
{
    /// <summary>
    /// 标记方法跳过操作日志记录
    /// 支持类级和方法级
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IgnoreOperLogAttribute : Attribute
    {
    }
}