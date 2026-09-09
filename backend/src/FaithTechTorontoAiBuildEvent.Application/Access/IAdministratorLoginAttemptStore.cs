namespace FaithTechTorontoAiBuildEvent.Application.Access;

public interface IAdministratorLoginAttemptStore
{
    Task<TimeSpan?> GetRetryAfterAsync(string source, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    Task RecordFailureAsync(string source, DateTimeOffset nowUtc, CancellationToken cancellationToken);
}
