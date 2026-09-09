using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class DeleteAdministratorParticipantHandler(IAdministratorAuthorizationStore authorizationStore, IEntryReceiptSecretService secretService, IAdministratorParticipantStore participantStore) : IRequestHandler<DeleteAdministratorParticipantCommand>
{
    public async Task Handle(DeleteAdministratorParticipantCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var version) || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }
        await participantStore.DeleteAsync(request.ParticipantId, version, cancellationToken);
    }
}
