using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class UpdateAdministratorParticipantHandler(IAdministratorAuthorizationStore authorizationStore, IEntryReceiptSecretService secretService, IAdministratorParticipantStore participantStore) : IRequestHandler<UpdateAdministratorParticipantCommand, AdministratorParticipant>
{
    public async Task<AdministratorParticipant> Handle(UpdateAdministratorParticipantCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var version) || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken)) throw new UnauthorizedAccessException();
        var email = request.Input.Email.Trim();
        return await participantStore.UpdateAsync(request.ParticipantId, email, EmailNormalizer.Normalize(email), request.Input, version, cancellationToken);
    }
}
