using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class ResetAdministratorPasswordsHandler(IAdministratorProvisioner provisioner)
    : IRequestHandler<ResetAdministratorPasswordsCommand, string[]>
{
    public Task<string[]> Handle(ResetAdministratorPasswordsCommand request, CancellationToken cancellationToken) =>
        provisioner.ResetPasswords(request.Username?.Trim(), request.Password, cancellationToken);
}
