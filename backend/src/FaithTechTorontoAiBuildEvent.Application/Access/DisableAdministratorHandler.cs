using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class DisableAdministratorHandler(IAdministratorProvisioner provisioner)
    : IRequestHandler<DisableAdministratorCommand>
{
    public Task Handle(DisableAdministratorCommand request, CancellationToken cancellationToken) =>
        provisioner.Disable(request.Username.Trim(), cancellationToken);
}
