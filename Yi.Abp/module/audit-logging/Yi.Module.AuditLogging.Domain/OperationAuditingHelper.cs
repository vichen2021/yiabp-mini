using System.Reflection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Volo.Abp.Auditing;
using Volo.Abp.Clients;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Threading;
using Volo.Abp.Timing;
using Volo.Abp.Tracing;
using Volo.Abp.Users;
using Yi.Framework.Auditing.Abstractions;

namespace Yi.Module.AuditLogging.Domain;

/// <summary>
/// 在 ABP 创建 Action 审计信息时附加 Yi 业务操作元数据
/// </summary>
[Dependency(ReplaceServices = true)]
public class OperationAuditingHelper : AuditingHelper
{
    private readonly YiAuditLoggingOptions _yiOptions;

    /// <summary>
    /// 创建负责附加业务操作元数据的审计辅助器
    /// </summary>
    public OperationAuditingHelper(
        IAuditSerializer auditSerializer,
        IOptions<AbpAuditingOptions> options,
        ICurrentUser currentUser,
        ICurrentTenant currentTenant,
        ICurrentClient currentClient,
        IClock clock,
        IAuditingStore auditingStore,
        ILogger<AuditingHelper> logger,
        IServiceProvider serviceProvider,
        ICorrelationIdProvider correlationIdProvider,
        IAmbientScopeProvider<AuditingDisabledState> auditingDisabledState,
        IOptions<YiAuditLoggingOptions> yiOptions)
        : base(
            auditSerializer,
            options,
            currentUser,
            currentTenant,
            currentClient,
            clock,
            auditingStore,
            logger,
            serviceProvider,
            correlationIdProvider,
            auditingDisabledState)
    {
        _yiOptions = yiOptions.Value;
    }

    /// <summary>
    /// 创建 Action 审计信息并直接写入业务操作扩展属性
    /// </summary>
    public override AuditLogActionInfo CreateAuditLogAction(
        AuditLogInfo auditLog,
        Type? type,
        MethodInfo method,
        IDictionary<string, object?> arguments)
    {
        var actionInfo = base.CreateAuditLogAction(auditLog, type, method, arguments);
        var metadata = OperationMetadataResolver.Resolve(type, method);
        if (metadata == null)
        {
            return actionInfo;
        }

        actionInfo.ExtraProperties[OperationMetadataPropertyNames.Title] = metadata.Title;
        actionInfo.ExtraProperties[OperationMetadataPropertyNames.Type] = (int)metadata.OperationType;
        actionInfo.ExtraProperties[OperationMetadataPropertyNames.IsBusinessOperation] = metadata.IsBusinessOperation;

        if (!_yiOptions.SaveRequestData && !metadata.SaveRequestData)
        {
            actionInfo.Parameters = "{}";
        }

        return actionInfo;
    }
}
