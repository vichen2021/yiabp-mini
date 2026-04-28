using System.Text.Json;
using Volo.Abp.Auditing;
using Volo.Abp.DependencyInjection;
using Yi.Framework.AuditLogging.Domain.Entities;
using Yi.Framework.Operation.Abstractions.Enums;

namespace Yi.Framework.AuditLogging.Domain
{
    /// <summary>
    /// 默认操作日志映射器
    /// 从 ABP AuditLogInfo 映射到业务 OperationLogEntity
    /// </summary>
    public class DefaultOperationLogMapper : IOperationLogMapper, ITransientDependency
    {
        private readonly YiAuditLoggingOptions _options;

        public DefaultOperationLogMapper(YiAuditLoggingOptions options)
        {
            _options = options;
        }

        public OperationLogEntity? TryMap(AuditLogInfo auditLogInfo)
        {
            // 不保存操作日志时返回 null
            if (!_options.SaveOperationLog)
            {
                return null;
            }

            // 查找业务 Action
            var actionInfo = auditLogInfo.Actions?.FirstOrDefault();
            if (actionInfo == null)
            {
                return null;
            }

            // 从 Action 信息推断操作类型
            var operType = InferOperType(actionInfo.MethodName);
            if (operType == null)
            {
                // 无法推断操作类型，不生成操作日志
                return null;
            }

            // 从 ServiceName 推断实体名
            var entityName = InferEntityName(actionInfo.ServiceName);

            // 生成标题
            var title = GenerateTitle(entityName, operType);

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
            if (_options.SaveRequestData && actionInfo.Parameters != null)
            {
                entity.RequestParam = Truncate(JsonSerializer.Serialize(actionInfo.Parameters), _options.MaxRequestParamLength);
            }

            // 错误信息
            if (auditLogInfo.Exceptions != null && auditLogInfo.Exceptions.Count > 0)
            {
                entity.ErrorMessage = Truncate(string.Join(Environment.NewLine, auditLogInfo.Exceptions.Select(e => e.Message)), _options.MaxResponseDataLength);
            }

            return entity;
        }

        /// <summary>
        /// 从方法名推断操作类型
        /// </summary>
        private OperEnum? InferOperType(string? methodName)
        {
            if (string.IsNullOrEmpty(methodName))
            {
                return null;
            }

            // 方法名映射
            if (methodName.StartsWith("Create") || methodName.StartsWith("Add") || methodName.StartsWith("Insert"))
            {
                return OperEnum.Insert;
            }
            if (methodName.StartsWith("Update") || methodName.StartsWith("Edit") || methodName.StartsWith("Modify"))
            {
                return OperEnum.Update;
            }
            if (methodName.StartsWith("Delete") || methodName.StartsWith("Remove"))
            {
                return OperEnum.Delete;
            }
            if (methodName.StartsWith("Export"))
            {
                return OperEnum.Export;
            }
            if (methodName.StartsWith("Import"))
            {
                return OperEnum.Import;
            }

            // POST/PUT/DELETE 方法
            // GET 方法不记录操作日志

            return null;
        }

        /// <summary>
        /// 从 ServiceName 推断实体名
        /// </summary>
        private string? InferEntityName(string? serviceName)
        {
            if (string.IsNullOrEmpty(serviceName))
            {
                return null;
            }

            // 移除 Service 后缀
            if (serviceName.EndsWith("Service"))
            {
                serviceName = serviceName.Substring(0, serviceName.Length - 7);
            }

            // 首字母小写
            if (serviceName.Length > 0)
            {
                serviceName = serviceName[0].ToString().ToLower() + serviceName.Substring(1);
            }

            return serviceName;
        }

        /// <summary>
        /// 生成操作标题
        /// </summary>
        private string? GenerateTitle(string? entityName, OperEnum? operType)
        {
            if (string.IsNullOrEmpty(entityName) || operType == null)
            {
                return null;
            }

            var actionDesc = operType switch
            {
                OperEnum.Insert => "添加",
                OperEnum.Update => "更新",
                OperEnum.Delete => "删除",
                OperEnum.Export => "导出",
                OperEnum.Import => "导入",
                OperEnum.Auth => "授权",
                OperEnum.ForceLogout => "强制退出",
                OperEnum.GenerateCode => "生成代码",
                OperEnum.Clear => "清空",
                _ => "操作"
            };

            return $"{actionDesc}{entityName}";
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