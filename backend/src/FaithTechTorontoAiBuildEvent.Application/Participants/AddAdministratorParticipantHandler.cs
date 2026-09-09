using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using MediatR;
using System.Globalization;

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
        var normalizedEmail = EmailNormalizer.Normalize(email);
        var inputDigest = OperationInputDigest.Create(
            version.ToString(CultureInfo.InvariantCulture),
            normalizedEmail);
        return await participantStore.AddAsync(request.OperationId, inputDigest, email, normalizedEmail, version, cancellationToken);
    }
}
