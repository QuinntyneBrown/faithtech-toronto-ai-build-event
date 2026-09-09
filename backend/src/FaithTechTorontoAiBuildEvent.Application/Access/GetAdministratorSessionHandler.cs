using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Access;

public sealed class GetAdministratorSessionHandler(
    IAdministratorAuthorizationStore authorizationStore,
    IEntryReceiptSecretService secretService)
    : IRequestHandler<GetAdministratorSessionQuery, bool>
{
    public Task<bool> Handle(GetAdministratorSessionQuery request, CancellationToken cancellationToken)
        => string.IsNullOrEmpty(request.Secret)
            ? Task.FromResult(false)
            : authorizationStore.IsAuthorizedAsync(secretService.Digest(request.Secret), DateTimeOffset.UtcNow, cancellationToken);
}
