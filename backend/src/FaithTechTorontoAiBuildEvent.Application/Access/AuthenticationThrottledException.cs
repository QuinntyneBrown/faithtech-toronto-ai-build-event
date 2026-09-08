namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class AuthenticationThrottledException(int retryAfterSeconds) : Exception("Authentication temporarily unavailable.")
{
    public int RetryAfterSeconds { get; } = retryAfterSeconds;
}
