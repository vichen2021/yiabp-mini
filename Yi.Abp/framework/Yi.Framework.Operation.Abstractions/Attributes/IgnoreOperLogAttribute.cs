namespace Yi.Framework.Operation.Abstractions.Attributes
{
    /// <summary>
    /// 标记方法跳过操作日志记录
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class IgnoreOperLogAttribute : Attribute
    {
    }
}