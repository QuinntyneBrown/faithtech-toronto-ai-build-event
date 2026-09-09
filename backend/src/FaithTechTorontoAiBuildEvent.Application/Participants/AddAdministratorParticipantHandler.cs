using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class AddAdministratorParticipantHandler(IAdministratorAuthorizationStore authorizationStore, IEntryReceiptSecretService secretService, IAdministratorParticipantStore participantStore) : IRequestHandler<AddAdministratorParticipantCommand, AdministratorParticipant>
{
    public async Task<AdministratorParticipant> Handle(AddAdministratorParticipantCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var version) || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken))
        {
            throw new UnauthorizedAccessException();
        }
        var email = request.Email.Trim();
        return await participantStore.AddAsync(email, EmailNormalizer.Normalize(email), version, cancellationToken);
    }
}
