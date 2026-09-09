namespace FaithTechTorontoAiBuildEvent.Application.Access;

public interface IAdministratorCredentialProvisioner
{
    Task ProvisionAsync(string passcode, CancellationToken cancellationToken);
}
