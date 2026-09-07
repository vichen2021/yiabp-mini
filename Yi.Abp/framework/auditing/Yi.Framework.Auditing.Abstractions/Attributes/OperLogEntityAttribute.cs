namespace Yi.Framework.Auditing.Abstractions.Attributes;

/// <summary>
/// 声明服务对应的业务实体显示名
/// </summary>
[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public class OperLogEntityAttribute : Attribute
{
    public string DisplayName { get; }

    /// <summary>
    /// 创建业务实体显示名特性
    /// </summary>
    /// <param name="displayName">业务实体显示名</param>
    public OperLogEntityAttribute(string displayName)
    {
        DisplayName = displayName;
    }
}
