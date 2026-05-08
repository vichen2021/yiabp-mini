using Yi.Framework.OperationRecord.Abstractions.Enums;

namespace Yi.Framework.OperationRecord.Abstractions.Attributes
{
    /// <summary>
    /// 操作记录标记特性。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class OperLogAttribute : Attribute
    {
        /// <summary>
        /// 操作记录标题。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 操作类型。
        /// </summary>
        public OperEnum OperType { get; set; }

        /// <summary>
        /// 是否保存请求参数。
        /// </summary>
        public bool IsSaveRequestData { get; set; } = true;

        /// <summary>
        /// 是否保存响应数据。
        /// </summary>
        public bool IsSaveResponseData { get; set; } = false;

        public OperLogAttribute(string title, OperEnum operType = OperEnum.Other)
        {
            Title = title;
            OperType = operType;
        }

        public OperLogAttribute(OperEnum operType)
        {
            Title = string.Empty;
            OperType = operType;
        }
    }
}
