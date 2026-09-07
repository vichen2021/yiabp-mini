using System;
using SqlSugar;
using Volo.Abp.Auditing;
using Volo.Abp.Data;
using Volo.Abp.Domain.Entities;
using Volo.Abp.MultiTenancy;
using Yi.Module.AuditLogging.Domain.Shared.Consts;
using Yi.Framework.Auditing.Abstractions;
using Yi.Framework.Auditing.Abstractions.Enums;

namespace Yi.Module.AuditLogging.Domain.Entities;

[DisableAuditing]
[SugarTable("AuditLogAction")]
[SugarIndex($"index_{nameof(AuditLogId)}", nameof(AuditLogId), OrderByType.Asc)]
[SugarIndex($"index_{nameof(TenantId)}_{nameof(ExecutionTime)}", nameof(TenantId), OrderByType.Asc, nameof(ServiceName), OrderByType.Asc, nameof(MethodName), OrderByType.Asc, nameof(ExecutionTime), OrderByType.Asc)]
public class AuditLogActionEntity : Entity<Guid>, IMultiTenant
{
    public virtual Guid? TenantId { get; protected set; }

    public virtual Guid AuditLogId { get; protected set; }

    public virtual string? ServiceName { get; protected set; }

    public virtual string? MethodName { get; protected set; }

    [SugarColumn(ColumnDataType = StaticConfig.CodeFirst_BigString)]
    public virtual string? Parameters { get; protected set; }

    public virtual DateTime? ExecutionTime { get; protected set; }

    public virtual int? ExecutionDuration { get; protected set; }

    /// <summary>
    /// 业务操作标题
    /// </summary>
    [SugarColumn(Length = 200)]
    public virtual string? OperationTitle { get; protected set; }

    /// <summary>
    /// 业务操作类型
    /// </summary>
    public virtual OperEnum? OperationType { get; protected set; }

    /// <summary>
    /// 是否为需要展示的业务操作
    /// </summary>
    public virtual bool IsBusinessOperation { get; protected set; }


    [SugarColumn(ColumnName = "Id", IsPrimaryKey = true)]
    public override Guid Id { get; protected set; }
    public AuditLogActionEntity()
    {
    }

    public AuditLogActionEntity(Guid id, Guid auditLogId, AuditLogActionInfo actionInfo, Guid? tenantId = null)
    {

        Id = id;
        TenantId = tenantId;
        AuditLogId = auditLogId;
        ExecutionTime = actionInfo.ExecutionTime;
        ExecutionDuration = actionInfo.ExecutionDuration;

        ServiceName = actionInfo.ServiceName.TruncateFromBeginning(AuditLogActionConsts.MaxServiceNameLength);
        MethodName = actionInfo.MethodName.TruncateFromBeginning(AuditLogActionConsts.MaxMethodNameLength);
        Parameters = actionInfo.Parameters.Length > AuditLogActionConsts.MaxParametersLength ? "" : actionInfo.Parameters;

        OperationTitle = GetExtraProperty<string>(actionInfo, OperationMetadataPropertyNames.Title);
        var operationType = GetExtraProperty<int?>(actionInfo, OperationMetadataPropertyNames.Type);
        OperationType = operationType.HasValue ? (OperEnum)operationType.Value : null;
        IsBusinessOperation = GetExtraProperty<bool>(actionInfo, OperationMetadataPropertyNames.IsBusinessOperation);
    }

    /// <summary>
    /// 从 ABP Action 扩展属性读取并转换业务审计字段
    /// </summary>
    private static T? GetExtraProperty<T>(AuditLogActionInfo actionInfo, string name)
    {
        if (!actionInfo.ExtraProperties.TryGetValue(name, out var value) || value == null)
        {
            return default;
        }

        if (value is T typedValue)
        {
            return typedValue;
        }

        var targetType = Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T);
        return (T?)Convert.ChangeType(value, targetType);
    }
}
