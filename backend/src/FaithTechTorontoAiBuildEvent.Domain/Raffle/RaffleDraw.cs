namespace FaithTechTorontoAiBuildEvent.Domain.Raffle;

public sealed class RaffleDraw
{
    public Guid Id { get; set; }
    public Guid? WinnerParticipantId { get; set; }
    public required string WinnerLabel { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public DateTimeOffset RevealAtUtc { get; set; }
    public DateTimeOffset EffectsEndAtUtc { get; set; }
}
