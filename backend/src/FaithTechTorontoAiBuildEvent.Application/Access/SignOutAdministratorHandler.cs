using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class SignOutAdministratorHandler(IAdministratorStore store) : IRequestHandler<SignOutAdministratorCommand>
{
    public Task Handle(SignOutAdministratorCommand request, CancellationToken cancellationToken) =>
        store.RevokeSession(request.SessionId, cancellationToken);
}
