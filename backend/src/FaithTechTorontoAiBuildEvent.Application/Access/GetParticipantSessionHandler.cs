using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class GetParticipantSessionHandler(IParticipantStore store) : IRequestHandler<GetParticipantSessionQuery, ParticipantSessionState?>
{
    public async Task<ParticipantSessionState?> Handle(GetParticipantSessionQuery request, CancellationToken cancellationToken)
    {
        var session = await store.FindSession(request.SessionId, cancellationToken);
        var now = await store.GetUtcNow(cancellationToken);
        return session is not null && session.IsValidAt(now) && await store.IsActiveRegistration(session.RegistrationId, cancellationToken)
            ? new ParticipantSessionState(session.RegistrationId, session.EventId, now, session.AuthenticatedAtUtc.AddHours(24)) : null;
    }
}
