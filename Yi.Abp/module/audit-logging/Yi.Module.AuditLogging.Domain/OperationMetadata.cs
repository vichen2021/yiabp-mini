using Yi.Framework.Auditing.Abstractions.Enums;

namespace Yi.Module.AuditLogging.Domain;

/// <summary>
/// 应用服务动作的业务操作审计元数据
/// </summary>
internal sealed record OperationMetadata(
    string Title,
    OperEnum OperationType,
    bool SaveRequestData,
    bool IsBusinessOperation);
