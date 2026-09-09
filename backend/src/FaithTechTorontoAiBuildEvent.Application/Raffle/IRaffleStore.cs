namespace FaithTechTorontoAiBuildEvent.Application.Raffle;

public interface IRaffleStore
{
    Task<DrawWinnerResult> DrawAsync(Guid operationId, long expectedVersion, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    Task<RaffleSnapshot> GetSnapshotAsync(CancellationToken cancellationToken);
}
