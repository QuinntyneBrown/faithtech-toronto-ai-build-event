namespace FaithTechTorontoAiBuildEvent.Application.Raffle;

public sealed record DrawWinnerResult(Guid DrawId, string WinnerLabel, DateTimeOffset RevealAtUtc, DateTimeOffset EffectsEndAtUtc);
