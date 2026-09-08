using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class SignOutParticipantHandler(IParticipantStore store) : IRequestHandler<SignOutParticipantCommand>
{
    public Task Handle(SignOutParticipantCommand request, CancellationToken cancellationToken) =>
        store.RevokeSession(request.SessionId, cancellationToken);
}
