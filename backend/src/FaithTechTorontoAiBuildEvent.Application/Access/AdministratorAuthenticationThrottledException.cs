namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class AdministratorAuthenticationThrottledException(TimeSpan retryAfter) : Exception
{
    public TimeSpan RetryAfter { get; } = retryAfter;
}
