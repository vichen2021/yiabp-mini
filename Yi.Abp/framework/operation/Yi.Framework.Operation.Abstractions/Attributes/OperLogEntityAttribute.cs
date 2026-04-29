namespace Yi.Framework.Operation.Abstractions.Attributes
{
    /// <summary>
    /// 标记服务类的实体显示名，用于操作日志标题生成
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class OperLogEntityAttribute : Attribute
    {
        public string DisplayName { get; }

        public OperLogEntityAttribute(string displayName)
        {
            DisplayName = displayName;
        }
    }
}