using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class GetAdministratorSessionHandler(IAdministratorStore store)
    : IRequestHandler<GetAdministratorSessionQuery, AdministratorSessionState?>
{
    public async Task<AdministratorSessionState?> Handle(GetAdministratorSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await store.FindSession(request.SessionId, cancellationToken);
        var now = await store.GetUtcNow(cancellationToken);
        return session is not null && session.IsValidAt(now) &&
            await store.IsEnabledAdministrator(session.AdministratorId, cancellationToken)
            ? new AdministratorSessionState(session.AdministratorId, now,
                session.AuthenticatedAtUtc.AddHours(8), session.LastInteractionAtUtc.AddMinutes(30)) : null;
    }
}
