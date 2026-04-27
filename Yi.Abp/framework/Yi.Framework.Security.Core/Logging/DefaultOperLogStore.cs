using Microsoft.AspNetCore.Http;
using System.Text.Json;
using Yi.Framework.Security.Abstractions.Enums;
using Yi.Framework.Security.Abstractions.Logging;
using Yi.Framework.Security.Abstractions.Metadata;

namespace Yi.Framework.Security.Core.Logging
{
    /// <summary>
    /// 默认操作日志存储
    /// </summary>
    public class DefaultOperLogStore : IOperLogStore
    {
        private readonly IOperLogPersistence _persistence;
        private readonly OperLogOptions _options;

        public DefaultOperLogStore(IOperLogPersistence persistence, OperLogOptions options)
        {
            _persistence = persistence;
            _options = options;
        }

        public async Task SaveAsync(
            ActionMetadata metadata,
            HttpContext context,
            object? result,
            Exception? exception)
        {
            var entry = new OperLogEntry
            {
                Title = metadata.ExplicitLogInfo?.Title ?? metadata.LogTitle,
                OperType = metadata.ExplicitLogInfo?.OperType ?? metadata.OperType ?? OperEnum.Other,
                RequestMethod = context.Request.Method,
                OperUser = context.User?.Identity?.Name,
                OperIp = GetClientIp(context),
                OperLocation = "",
                Method = context.Request.Path.Value,
                IsSuccess = exception == null,
                ErrorMessage = exception?.Message,
                ExecutionTime = DateTime.Now
            };

            // 请求参数
            if (_options.SaveRequestData)
            {
                entry.RequestParam = SerializeRequestData(context.Request);
                if (entry.RequestParam?.Length > _options.MaxRequestParamLength)
                {
                    entry.RequestParam = entry.RequestParam?.Substring(0, _options.MaxRequestParamLength);
                }
            }

            // 响应数据
            if (_options.SaveResponseData && result != null)
            {
                entry.RequestResult = SerializeResponseData(result);
                if (entry.RequestResult?.Length > _options.MaxResponseDataLength)
                {
                    entry.RequestResult = entry.RequestResult?.Substring(0, _options.MaxResponseDataLength);
                }
            }

            await _persistence.PersistAsync(entry);
        }

        private string? GetClientIp(HttpContext context)
        {
            return context.Connection.RemoteIpAddress?.ToString();
        }

        private string? SerializeRequestData(HttpRequest request)
        {
            try
            {
                // 简化实现：返回请求路径和查询参数
                return $"{request.Method} {request.Path}{request.QueryString}";
            }
            catch
            {
                return null;
            }
        }

        private string? SerializeResponseData(object? result)
        {
            try
            {
                if (result == null) return null;
                return JsonSerializer.Serialize(result);
            }
            catch
            {
                return null;
            }
        }
    }
}