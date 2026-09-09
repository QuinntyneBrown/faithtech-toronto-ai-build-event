namespace FaithTechTorontoAiBuildEvent.Domain.Participants;

public sealed class EntryReceipt
{
    public Guid Id { get; set; }
    public required byte[] SecretDigest { get; set; }
    public Guid OperationId { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public Guid? ParticipantId { get; set; }
    public bool Revoked { get; set; }
}
