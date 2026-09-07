using Yi.Framework.Auditing.Abstractions.Enums;

namespace Yi.Framework.Auditing.Abstractions.Attributes;

/// <summary>
/// 声明应用服务方法的业务操作审计元数据
/// </summary>
[AttributeUsage(AttributeTargets.Method, Inherited = true)]
public class OperLogAttribute : Attribute
{
    public string Title { get; }

    public OperEnum OperType { get; set; }

    public bool IsSaveRequestData { get; set; } = true;

    public bool IsSaveResponseData { get; set; }

    /// <summary>
    /// 使用业务标题和操作类型创建操作审计特性
    /// </summary>
    /// <param name="title">业务操作标题</param>
    /// <param name="operType">业务操作类型</param>
    public OperLogAttribute(string title, OperEnum operType = OperEnum.Other)
    {
        Title = title;
        OperType = operType;
    }

    /// <summary>
    /// 使用操作类型创建由实体名称推导标题的操作审计特性
    /// </summary>
    /// <param name="operType">业务操作类型</param>
    public OperLogAttribute(OperEnum operType)
    {
        Title = string.Empty;
        OperType = operType;
    }
}
