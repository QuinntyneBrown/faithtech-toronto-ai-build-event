namespace FaithTechTorontoAiBuildEvent.Domain.Operations;

public sealed class OperationReceipt
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ActorId { get; set; }
    public ActorKind ActorKind { get; set; }
    public Guid? EventId { get; set; }
    public Guid OperationId { get; set; }
    public required string Target { get; set; }
    public required string PayloadHash { get; set; }
    public required string Result { get; set; }
    public DateTimeOffset CommittedAtUtc { get; set; }
}
