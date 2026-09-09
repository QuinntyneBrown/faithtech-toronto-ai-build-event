using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;
using System.Globalization;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class DeleteAdministratorParticipantHandler(IAdministratorAuthorizationStore authorizationStore, IEntryReceiptSecretService secretService, IAdministratorParticipantStore participantStore) : IRequestHandler<DeleteAdministratorParticipantCommand>
{
    public async Task Handle(DeleteAdministratorParticipantCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var version) || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }
        var inputDigest = OperationInputDigest.Create(
            request.ParticipantId.ToString("D"),
            version.ToString(CultureInfo.InvariantCulture));
        await participantStore.DeleteAsync(request.OperationId, inputDigest, request.ParticipantId, version, cancellationToken);
    }
}
