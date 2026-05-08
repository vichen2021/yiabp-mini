namespace Yi.Framework.OperationRecord.Abstractions.Attributes
{
    /// <summary>
    /// 标记方法或类跳过操作记录。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class IgnoreOperLogAttribute : Attribute
    {
    }
}
