using MediatR;
using FaithTechTorontoAiBuildEvent.Application.Participants;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class RevokeAdministratorSessionHandler(IAdministratorSessionStore sessionStore, IEntryReceiptSecretService secretService)
    : IRequestHandler<RevokeAdministratorSessionCommand>
{
    public Task Handle(RevokeAdministratorSessionCommand request, CancellationToken cancellationToken)
        => string.IsNullOrEmpty(request.SessionSecret)
            ? Task.CompletedTask
            : sessionStore.RevokeAsync(secretService.Digest(request.SessionSecret), cancellationToken);
}
