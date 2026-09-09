namespace FaithTechTorontoAiBuildEvent.Domain.Access;

public sealed class AdministratorLoginAttempt
{
    public Guid Id { get; set; }
    public required string Source { get; set; }
    public DateTimeOffset AttemptedAtUtc { get; set; }
}
