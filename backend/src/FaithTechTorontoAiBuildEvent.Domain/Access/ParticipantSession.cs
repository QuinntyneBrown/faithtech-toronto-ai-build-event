namespace FaithTechTorontoAiBuildEvent.Domain.Access;

public sealed class ParticipantSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid RegistrationId { get; set; }
    public Guid EventId { get; set; }
    public DateTimeOffset AuthenticatedAtUtc { get; set; }
    public bool Revoked { get; set; }
    public bool IsValidAt(DateTimeOffset now) => !Revoked && now < AuthenticatedAtUtc.AddHours(24);
}
