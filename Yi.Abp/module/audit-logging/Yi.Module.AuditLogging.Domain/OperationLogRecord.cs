using Yi.Framework.Auditing.Abstractions.Enums;

namespace Yi.Module.AuditLogging.Domain;

/// <summary>
/// 统一审计模型投影出的业务操作记录
/// </summary>
public class OperationLogRecord
{
    public Guid Id { get; set; }

    public Guid AuditLogId { get; set; }

    public string? Title { get; set; }

    public OperEnum OperType { get; set; }

    public string? RequestMethod { get; set; }

    public string? OperUser { get; set; }

    public string? OperIp { get; set; }

    public string? Method { get; set; }

    public string? RequestParam { get; set; }

    public bool IsSuccess { get; set; }

    public string? ErrorMessage { get; set; }

    public int ExecutionDuration { get; set; }

    public DateTime CreationTime { get; set; }
}
