namespace FaithTechTorontoAiBuildEvent.Application.Raffle;

public interface IRaffleStore
{
    Task<DrawWinnerResult> DrawAsync(long expectedVersion, DateTimeOffset nowUtc, CancellationToken cancellationToken);
}
