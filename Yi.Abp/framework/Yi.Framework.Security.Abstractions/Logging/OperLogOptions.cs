namespace Yi.Framework.Security.Abstractions.Logging
{
    /// <summary>
    /// 操作日志配置选项
    /// </summary>
    public class OperLogOptions
    {
        /// <summary>是否启用操作日志</summary>
        public bool IsEnabled { get; set; } = true;

        /// <summary>是否记录查询操作</summary>
        public bool LogReadOperations { get; set; } = false;

        /// <summary>是否保存请求参数</summary>
        public bool SaveRequestData { get; set; } = true;

        /// <summary>是否保存响应数据</summary>
        public bool SaveResponseData { get; set; } = false;

        /// <summary>请求参数最大长度</summary>
        public int MaxRequestParamLength { get; set; } = 2000;

        /// <summary>响应数据最大长度</summary>
        public int MaxResponseDataLength { get; set; } = 2000;
    }
}