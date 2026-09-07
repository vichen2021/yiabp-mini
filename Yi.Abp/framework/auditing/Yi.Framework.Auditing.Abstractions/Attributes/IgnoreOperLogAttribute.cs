namespace Yi.Framework.Auditing.Abstractions.Attributes;

/// <summary>
/// 声明服务或方法不生成业务操作记录
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = true)]
public class IgnoreOperLogAttribute : Attribute
{
}
