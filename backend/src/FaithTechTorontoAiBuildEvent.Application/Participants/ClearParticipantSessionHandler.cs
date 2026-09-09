using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class ClearParticipantSessionHandler(
    IParticipantSessionStore sessionStore,
    IEntryReceiptSecretService secretService)
    : IRequestHandler<ClearParticipantSessionCommand>
{
    public Task Handle(ClearParticipantSessionCommand request, CancellationToken cancellationToken)
        => string.IsNullOrEmpty(request.Secret)
            ? Task.CompletedTask
            : sessionStore.RevokeAsync(secretService.Digest(request.Secret), cancellationToken);
}
