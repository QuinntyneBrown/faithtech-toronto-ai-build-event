namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class AuthenticationFailure
{
    public long Id { get; set; }
    public required string SourceKey { get; set; }
    public required string AccountKey { get; set; }
    public DateTimeOffset FailedAtUtc { get; set; }
}
