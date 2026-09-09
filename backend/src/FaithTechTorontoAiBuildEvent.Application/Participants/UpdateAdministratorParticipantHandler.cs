using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Application.Validation;
using MediatR;
using System.Globalization;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class UpdateAdministratorParticipantHandler(IAdministratorAuthorizationStore authorizationStore, IEntryReceiptSecretService secretService, IAdministratorParticipantStore participantStore) : IRequestHandler<UpdateAdministratorParticipantCommand, AdministratorParticipant>
{
    public async Task<AdministratorParticipant> Handle(UpdateAdministratorParticipantCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.AdministratorSecret) || !long.TryParse(request.ExpectedVersion, out var version) || !await authorizationStore.IsAuthorizedAsync(secretService.Digest(request.AdministratorSecret), DateTimeOffset.UtcNow, cancellationToken)) throw new UnauthorizedAccessException();
        var email = request.Input.Email.Trim();
        var normalizedEmail = EmailNormalizer.Normalize(email);
        if (!UnicodeText.IsWithinScalarLimit(request.Input.Name, 200)
            || !UnicodeText.IsWithinScalarLimit(request.Input.WhatYouMake, 2000)
            || !UnicodeText.IsWithinScalarLimit(request.Input.OnYourHeart, 2000))
        {
            throw new EntryValidationException("Participant fields are too long.");
        }
        var inputDigest = OperationInputDigest.Create(
            request.ParticipantId.ToString("D"),
            version.ToString(CultureInfo.InvariantCulture),
            normalizedEmail,
            request.Input.Name,
            request.Input.WhatYouMake,
            request.Input.OnYourHeart);
        return await participantStore.UpdateAsync(request.OperationId, inputDigest, request.ParticipantId, email, normalizedEmail, request.Input, version, cancellationToken);
    }
}
