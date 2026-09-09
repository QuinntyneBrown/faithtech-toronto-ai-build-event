namespace FaithTechTorontoAiBuildEvent.Application.Access;

public interface IAdministratorAuthorizationStore
{
    Task<bool> IsAuthorizedAsync(byte[] secretDigest, DateTimeOffset nowUtc, CancellationToken cancellationToken);
}
