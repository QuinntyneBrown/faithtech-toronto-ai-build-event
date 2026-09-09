namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class PublicEntryThrottledException(TimeSpan retryAfter) : Exception
{
    public TimeSpan RetryAfter { get; } = retryAfter;
}
