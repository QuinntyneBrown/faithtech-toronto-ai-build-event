using FaithTechTorontoAiBuildEvent.Domain.Access;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class AuthenticateAdministratorHandler(IAdministratorStore store, IRequestSource source)
    : IRequestHandler<AuthenticateAdministratorCommand, AdministratorSession?>
{
    public async Task<AdministratorSession?> Handle(AuthenticateAdministratorCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrEmpty(request.Password) ||
            request.Username.Length > 256 || request.Password.Length > 1024) return null;
        var administratorId = await store.VerifyCredentials(request.Username, request.Password, source.Address, cancellationToken);
        if (administratorId is null) return null;
        var now = await store.GetUtcNow(cancellationToken);
        var session = new AdministratorSession
        {
            AdministratorId = administratorId.Value, AuthenticatedAtUtc = now, LastInteractionAtUtc = now
        };
        await store.SaveSession(session, cancellationToken);
        return session;
    }
}
