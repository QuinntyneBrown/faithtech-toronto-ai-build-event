namespace FaithTechTorontoAiBuildEvent.Application.Access;

public interface IAdministratorSessionStore
{
    Task<AdministratorAuthenticationResult> AuthenticateAsync(string passcode, string sessionSecret, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    Task<bool> RecordInteractionAsync(byte[] secretDigest, DateTimeOffset nowUtc, CancellationToken cancellationToken);
    Task RevokeAsync(byte[] secretDigest, CancellationToken cancellationToken);
}
