using MediatR;

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
        return await profileStore.SaveAsync(new ProfileSaveRequest(secretService.Digest(request.Secret), expectedVersion, request.Input, DateTimeOffset.UtcNow), cancellationToken)
            ?? throw new EntryValidationException("Participant session is no longer valid.");
    }

    private static void Validate(ProfileInput input)
    {
        if ((input.Name?.Trim().Length ?? 0) > 200 || (input.WhatYouMake?.Trim().Length ?? 0) > 2000 || (input.OnYourHeart?.Trim().Length ?? 0) > 2000)
        {
            throw new EntryValidationException("Profile fields are too long.");
        }
    }
}
