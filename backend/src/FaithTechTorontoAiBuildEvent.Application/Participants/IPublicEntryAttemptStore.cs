namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public interface IPublicEntryAttemptStore
{
    Task<TimeSpan?> GetRetryAfterAsync(string source, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    Task RecordAsync(string source, DateTimeOffset nowUtc, CancellationToken cancellationToken);
}
