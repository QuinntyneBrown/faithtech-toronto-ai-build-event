using MediatR;
using FaithTechTorontoAiBuildEvent.Application.Validation;

namespace FaithTechTorontoAiBuildEvent.Application.Participants;

public sealed class SaveParticipantProfileHandler(IProfileStore profileStore, IEntryReceiptSecretService secretService)
    : IRequestHandler<SaveParticipantProfileCommand, ParticipantProfile>
{
    public async Task<ParticipantProfile> Handle(SaveParticipantProfileCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Secret) || !long.TryParse(request.ExpectedVersion, out var expectedVersion))
        {
            throw new EntryValidationException("Participant session or event version is invalid.");
        }

        Validate(request.Input);
        var canonicalInput = System.Text.Json.JsonSerializer.Serialize(request.Input);
        var inputDigest = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(canonicalInput));
        return await profileStore.SaveAsync(new ProfileSaveRequest(request.OperationId, inputDigest, secretService.Digest(request.Secret), expectedVersion, request.Input, DateTimeOffset.UtcNow), cancellationToken)
            ?? throw new EntryValidationException("Participant session is no longer valid.");
    }

    private static void Validate(ProfileInput input)
    {
        if (!UnicodeText.IsWithinScalarLimit(input.Name, 200)
            || !UnicodeText.IsWithinScalarLimit(input.WhatYouMake, 2000)
            || !UnicodeText.IsWithinScalarLimit(input.OnYourHeart, 2000))
        {
            throw new EntryValidationException("Profile fields are too long.");
        }
    }
}
