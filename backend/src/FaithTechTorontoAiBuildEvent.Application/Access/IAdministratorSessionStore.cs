namespace FaithTechTorontoAiBuildEvent.Application.Access;

public interface IAdministratorSessionStore
{
    Task<AdministratorAuthenticationResult> AuthenticateAsync(string passcode, string sessionSecret, DateTimeOffset nowUtc, CancellationToken cancellationToken);
}
