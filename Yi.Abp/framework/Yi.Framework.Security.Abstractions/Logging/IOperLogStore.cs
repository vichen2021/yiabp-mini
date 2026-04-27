using Microsoft.AspNetCore.Http;
using Yi.Framework.Security.Abstractions.Metadata;

namespace Yi.Framework.Security.Abstractions.Logging
{
    /// <summary>
    /// 操作日志存储接口
    /// </summary>
    public interface IOperLogStore
    {
        /// <summary>
        /// 保存操作日志
        /// </summary>
        Task SaveAsync(ActionMetadata metadata, HttpContext context, object? result, Exception? exception);
    }
}