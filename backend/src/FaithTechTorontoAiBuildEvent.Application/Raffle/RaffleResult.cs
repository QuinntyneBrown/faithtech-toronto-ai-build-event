namespace FaithTechTorontoAiBuildEvent.Application.Raffle;

public sealed record RaffleResult(Guid DrawId, string WinnerLabel, IReadOnlyList<string> CandidateLabels, DateTimeOffset StartedAtUtc, DateTimeOffset RevealAtUtc, DateTimeOffset EffectsEndAtUtc);
