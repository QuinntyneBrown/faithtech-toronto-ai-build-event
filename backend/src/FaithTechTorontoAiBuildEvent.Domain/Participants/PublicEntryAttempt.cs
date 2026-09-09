namespace FaithTechTorontoAiBuildEvent.Domain.Participants;

public sealed class PublicEntryAttempt
{
    public Guid Id { get; set; }
    public required string Source { get; set; }
    public DateTimeOffset AttemptedAtUtc { get; set; }
}
