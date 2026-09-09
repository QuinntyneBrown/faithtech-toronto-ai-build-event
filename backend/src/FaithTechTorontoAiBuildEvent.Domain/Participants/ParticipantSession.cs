namespace FaithTechTorontoAiBuildEvent.Domain.Participants;

public sealed class ParticipantSession
{
    public Guid Id { get; set; }
    public Guid ParticipantId { get; set; }
    public required byte[] SecretDigest { get; set; }
    public DateTimeOffset ExpiresAtUtc { get; set; }
    public bool Revoked { get; set; }
}
