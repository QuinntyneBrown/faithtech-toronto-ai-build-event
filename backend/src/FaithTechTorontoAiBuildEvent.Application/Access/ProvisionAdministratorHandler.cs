using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class ProvisionAdministratorHandler(IAdministratorProvisioner provisioner)
    : IRequestHandler<ProvisionAdministratorCommand, Guid>
{
    public Task<Guid> Handle(ProvisionAdministratorCommand request, CancellationToken cancellationToken) =>
        provisioner.Provision(request.Username.Trim(), request.Password, cancellationToken);
}
