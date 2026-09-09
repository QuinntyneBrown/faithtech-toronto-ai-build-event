namespace FaithTechTorontoAiBuildEvent.Domain.Operations;

public sealed class AuditRecord
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActorId { get; set; }
    public ActorKind ActorKind { get; set; }
    public Guid? EventId { get; set; }
    public Guid? SubjectId { get; set; }
    public required string Action { get; set; }
    public required string Outcome { get; set; }
    public DateTimeOffset AtUtc { get; set; }
}
