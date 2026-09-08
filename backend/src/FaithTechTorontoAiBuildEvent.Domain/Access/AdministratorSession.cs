namespace FaithTechTorontoAiBuildEvent.Domain.Access;

public sealed class AdministratorSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid AdministratorId { get; set; }
    public DateTimeOffset AuthenticatedAtUtc { get; set; }
    public DateTimeOffset LastInteractionAtUtc { get; set; }
    public bool Revoked { get; set; }
    public bool IsValidAt(DateTimeOffset now) => !Revoked &&
        now < AuthenticatedAtUtc.AddHours(8) && now < LastInteractionAtUtc.AddMinutes(30);
}
