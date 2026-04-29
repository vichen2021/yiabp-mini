using Yi.Module.AuditLogging.Domain.Entities;

namespace Yi.Module.AuditLogging.Domain;

public class EntityChangeWithUsername
{
    public EntityChangeEntity EntityChange { get; set; }

    public string UserName { get; set; }
}
