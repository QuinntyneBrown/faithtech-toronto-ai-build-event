namespace FaithTechTorontoAiBuildEvent.Application.Access;

public interface IAdministratorProvisioner
{
    Task<Guid> Provision(string username, string password, CancellationToken cancellationToken);
    Task Disable(string username, CancellationToken cancellationToken);
}
