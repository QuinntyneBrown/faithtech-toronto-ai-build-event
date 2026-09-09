using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class RecordAdministratorInteractionHandler(
    IAdministratorSessionStore sessionStore,
    IEntryReceiptSecretService secretService)
    : IRequestHandler<RecordAdministratorInteractionCommand, bool>
{
    public Task<bool> Handle(RecordAdministratorInteractionCommand request, CancellationToken cancellationToken)
        => string.IsNullOrEmpty(request.SessionSecret)
            ? Task.FromResult(false)
            : sessionStore.RecordInteractionAsync(
                secretService.Digest(request.SessionSecret),
                DateTimeOffset.UtcNow,
                cancellationToken);
}
