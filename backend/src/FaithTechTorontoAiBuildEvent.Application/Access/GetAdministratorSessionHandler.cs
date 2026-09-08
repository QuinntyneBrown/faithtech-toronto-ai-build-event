using FaithTechTorontoAiBuildEvent.Domain.Access;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class GetAdministratorSessionHandler(IAdministratorStore store)
    : IRequestHandler<GetAdministratorSessionQuery, AdministratorSession?>
{
    public async Task<AdministratorSession?> Handle(GetAdministratorSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await store.FindSession(request.SessionId, cancellationToken);
        return session is not null && session.IsValidAt(await store.GetUtcNow(cancellationToken)) &&
            await store.IsEnabledAdministrator(session.AdministratorId, cancellationToken) ? session : null;
    }
}
