using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Options;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Yi.Module.AuditLogging.Domain.Entities;
using Yi.Framework.OperationRecord.Abstractions.Metadata;

namespace Yi.Module.AuditLogging.Domain
{
    /// <summary>
    /// 默认操作记录映射器
    /// 使用 IOperationLogRequirementResolver 解析操作记录要求
    /// </summary>
    public class DefaultOperationLogMapper : IOperationLogMapper, ITransientDependency
    {
        private readonly YiAuditLoggingOptions _yiOptions;
        private readonly IOperationLogRequirementResolver _logResolver;

        public DefaultOperationLogMapper(
            IOptions<YiAuditLoggingOptions> yiOptions,
            IOperationLogRequirementResolver logResolver)
        {
            _yiOptions = yiOptions.Value;
            _logResolver = logResolver;
        }

        public OperationLogEntity? TryMap(AuditLogInfo auditLogInfo)
        {
            // 不保存操作记录时返回 null
            if (!_yiOptions.SaveOperationLog)
            {
                return null;
            }

            // 检查是否在忽略 URL 列表中
            if (IsIgnoredUrl(auditLogInfo.Url))
            {
                return null;
            }

            // 查找业务 Action
            var actionInfo = auditLogInfo.Actions?.FirstOrDefault();
            if (actionInfo == null)
            {
                return null;
            }

            // 解析 ServiceType 和 MethodInfo
            var serviceType = ResolveServiceType(actionInfo.ServiceName);
            var methodInfo = ResolveMethod(serviceType, actionInfo.MethodName);
            if (serviceType == null || methodInfo == null)
            {
                return null;
            }

            // 使用操作记录要求解析器
            var requirement = _logResolver.Resolve(serviceType, methodInfo);
            var hasExplicitLog = requirement.Source == "OperLog";

            // 检查是否忽略操作记录
            if (requirement.Ignore)
            {
                return null;
            }

            // 自动写操作记录开关控制
            // 显式 [OperLog] 不受 AutoLogWriteOperations 控制
            if (!hasExplicitLog && !_yiOptions.AutoLogWriteOperations)
            {
                // 关闭自动写操作记录时，只有显式声明才记录
                return null;
            }

            // 查询操作记录控制
            if (!requirement.IsWriteOperation)
            {
                if (!_yiOptions.LogReadOperations && !hasExplicitLog)
                {
                    return null;
                }
            }

            // 获取操作类型
            var operType = requirement.OperType;
            if (operType == null)
            {
                // 无法推断操作类型，不生成操作记录
                return null;
            }

            // 获取标题
            var title = requirement.Title;
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            var entity = new OperationLogEntity(
                Guid.NewGuid(),
                title,
                operType.Value,
                auditLogInfo.HttpMethod,
                auditLogInfo.UserName,
                auditLogInfo.ClientIpAddress,
                $"{auditLogInfo.HttpMethod} {auditLogInfo.Url}",
                auditLogInfo.Exceptions == null || auditLogInfo.Exceptions.Count == 0,
                auditLogInfo.ExecutionDuration
            );

            // 请求参数
            if ((_yiOptions.SaveRequestData || requirement.SaveRequestData) && actionInfo.Parameters != null)
            {
                entity.RequestParam = Truncate(JsonSerializer.Serialize(actionInfo.Parameters), _yiOptions.MaxRequestParamLength);
            }

            // 错误信息
            if (auditLogInfo.Exceptions != null && auditLogInfo.Exceptions.Count > 0)
            {
                entity.ErrorMessage = Truncate(string.Join(Environment.NewLine, auditLogInfo.Exceptions.Select(e => e.Message)), _yiOptions.MaxResponseDataLength);
            }

            return entity;
        }

        /// <summary>
        /// 检查是否在忽略 URL 列表中
        /// </summary>
        private bool IsIgnoredUrl(string? url)
        {
            if (string.IsNullOrEmpty(url)) return false;

            foreach (var prefix in _yiOptions.IgnoredUrlPrefixes)
            {
                if (url.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        private Type? ResolveServiceType(string? serviceName)
        {
            if (string.IsNullOrWhiteSpace(serviceName))
            {
                return null;
            }

            var matchedType = AppDomain.CurrentDomain
                .GetAssemblies()
                .SelectMany(GetTypesSafely)
                .FirstOrDefault(type =>
                    string.Equals(type.FullName, serviceName, StringComparison.Ordinal) ||
                    string.Equals(type.Name, serviceName, StringComparison.Ordinal));

            if (matchedType is { IsInterface: true })
            {
                return AppDomain.CurrentDomain
                    .GetAssemblies()
                    .SelectMany(GetTypesSafely)
                    .FirstOrDefault(type => type is { IsClass: true, IsAbstract: false } && matchedType.IsAssignableFrom(type));
            }

            return matchedType;
        }

        private MethodInfo? ResolveMethod(Type? serviceType, string? methodName)
        {
            if (serviceType == null || string.IsNullOrWhiteSpace(methodName))
            {
                return null;
            }

            return serviceType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .FirstOrDefault(method => string.Equals(method.Name, methodName, StringComparison.Ordinal));
        }

        private static IEnumerable<Type> GetTypesSafely(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException ex)
            {
                return ex.Types.Where(type => type != null)!;
            }
        }

        /// <summary>
        /// 截断字符串
        /// </summary>
        private string? Truncate(string? text, int maxLength)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= maxLength)
            {
                return text;
            }

            return text.Substring(0, maxLength);
        }
    }
}
