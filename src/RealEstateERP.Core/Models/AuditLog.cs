namespace RealEstateERP.Core.Models;

/// <summary>
/// Append-only audit trail (F-AUTH-04). Rows are written automatically by the DbContext
/// on SaveChanges — see <c>RealEstateDbContext.CaptureAuditTrail</c> — and must never be
/// updated or deleted from application code. <see cref="Details"/> carries only the
/// changed columns (old → new), never full row snapshots.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>Authenticated actor from the token claim; null for system actions (e.g. seeding).</summary>
    public Guid? ActorUserId { get; set; }

    /// <summary>e.g. "user.created", "user.deactivated", "property.updated".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>CLR type name of the audited entity, e.g. "User".</summary>
    public string EntityType { get; set; } = string.Empty;

    public Guid? EntityId { get; set; }

    /// <summary>JSON (jsonb): changed-column diff with old → new values, or key values on create.</summary>
    public string? Details { get; set; }

    public DateTime OccurredAt { get; set; }
}