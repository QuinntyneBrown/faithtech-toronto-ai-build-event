using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class GetAdministratorParticipantsHandler(IAdministratorAuthorizationStore authorizationStore, IEntryReceiptSecretService secretService, IAdministratorParticipantStore participantStore) : IRequestHandler<GetAdministratorParticipantsQuery, IReadOnlyList<AdministratorParticipant>>
{
    public async Task<IReadOnlyList<AdministratorParticipant>> Handle(GetAdministratorParticipantsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }
        return await participantStore.ListAsync(cancellationToken);
    }
}
