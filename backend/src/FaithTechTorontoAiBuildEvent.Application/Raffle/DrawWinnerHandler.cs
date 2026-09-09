using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Participants;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Raffle;

public sealed class DrawWinnerHandler(
    IAdministratorAuthorizationStore authorizationStore,
    IEntryReceiptSecretService secretService,
    IRaffleStore raffleStore) : IRequestHandler<DrawWinnerCommand, DrawWinnerResult>
{
    public async Task<DrawWinnerResult> Handle(DrawWinnerCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var expectedVersion)
            || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }

        return await raffleStore.DrawAsync(request.OperationId, expectedVersion, DateTimeOffset.UtcNow, cancellationToken);
    }
}
