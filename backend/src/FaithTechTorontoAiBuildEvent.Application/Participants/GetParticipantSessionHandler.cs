using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class GetParticipantSessionHandler(
    IParticipantSessionStore sessionStore,
    IEntryReceiptSecretService secretService)
    : IRequestHandler<GetParticipantSessionQuery, ParticipantSessionState?>
{
    public Task<ParticipantSessionState?> Handle(GetParticipantSessionQuery request, CancellationToken cancellationToken)
        => string.IsNullOrEmpty(request.Secret)
            ? Task.FromResult<ParticipantSessionState?>(null)
            : sessionStore.FindActiveAsync(secretService.Digest(request.Secret), DateTimeOffset.UtcNow, cancellationToken);
}
