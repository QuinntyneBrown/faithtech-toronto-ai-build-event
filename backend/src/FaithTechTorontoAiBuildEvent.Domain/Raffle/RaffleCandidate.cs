namespace FaithTechTorontoAiBuildEvent.Domain.Raffle;

public sealed class RaffleCandidate
{
    public Guid DrawId { get; set; }
    public Guid ParticipantId { get; set; }
    public required string Label { get; set; }
}
