namespace FaithTechTorontoAiBuildEvent.Application.Raffle;

public sealed record RaffleSnapshot(int EligibleCount, RaffleResult? LatestResult, IReadOnlyList<RaffleResult> PreviousWinners);
