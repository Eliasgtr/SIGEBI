using Sigebi.Domain.Enums;

namespace Sigebi.Domain.Entities;

public class AuditLog
{
    public int Id { get; set; }
    public AuditActionType Action { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public string? EntityId { get; set; }
    public int? ActorUserId { get; set; }
    public string Details { get; set; } = string.Empty;
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}
