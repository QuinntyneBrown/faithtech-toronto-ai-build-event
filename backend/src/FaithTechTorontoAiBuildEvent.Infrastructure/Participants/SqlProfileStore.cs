using FaithTechTorontoAiBuildEvent.Application.Participants;
using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Domain.Participants;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using FaithTechTorontoAiBuildEvent.Application.EventState;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Participants;

public sealed class SqlProfileStore(CompanionDbContext database, IEventUpdatePublisher updatePublisher) : IProfileStore
{
    public async Task<ParticipantProfile?> GetAsync(byte[] secretDigest, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var participant = await FindParticipantAsync(secretDigest, nowUtc, cancellationToken);
        return participant is null ? null : Map(participant);
    }

    public async Task<ParticipantProfile?> SaveAsync(ProfileSaveRequest request, CancellationToken cancellationToken)
    {
        var participant = await FindParticipantAsync(request.SecretDigest, request.NowUtc, cancellationToken);
        if (participant is null)
        {
            return null;
        }

        var receipt = await database.ProfileSaveReceipts.SingleOrDefaultAsync(candidate => candidate.ParticipantId == participant.Id && candidate.OperationId == request.OperationId, cancellationToken);
        if (receipt is not null)
        {
            if (!System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(receipt.InputDigest, request.InputDigest))
            {
                throw new EntryValidationException("This profile operation identity was already used with different input.");
            }
            return new ParticipantProfile(receipt.Name, receipt.WhatYouMake, receipt.OnYourHeart);
        }

        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.CurrentScreen != EventScreen.Countdown || state.Version != request.ExpectedVersion)
        {
            throw new EntryValidationException("Countdown is no longer available; ask an administrator for help.");
        }

        participant.Name = BlankToNull(request.Input.Name);
        participant.WhatYouMake = BlankToNull(request.Input.WhatYouMake);
        participant.OnYourHeart = BlankToNull(request.Input.OnYourHeart);
        var publicDisplay = ParticipantPublicDisplay.Format(participant.Name, participant.PublicLabel);
        var retainedCandidates = await database.RaffleCandidates.Where(candidate => candidate.ParticipantId == participant.Id).ToListAsync(cancellationToken);
        foreach (var candidate in retainedCandidates) { candidate.Label = publicDisplay; }
        var retainedWins = await database.RaffleDraws.Where(draw => draw.WinnerParticipantId == participant.Id).ToListAsync(cancellationToken);
        foreach (var draw in retainedWins) { draw.WinnerLabel = publicDisplay; }
        database.ProfileSaveReceipts.Add(new FaithTechTorontoAiBuildEvent.Domain.Participants.ProfileSaveReceipt
        {
            OperationId = request.OperationId,
            ParticipantId = participant.Id,
            InputDigest = request.InputDigest,
            Name = participant.Name,
            WhatYouMake = participant.WhatYouMake,
            OnYourHeart = participant.OnYourHeart
        });
        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
        await updatePublisher.PublishAsync(state.Version, cancellationToken);
        return Map(participant);
    }

    private async Task<FaithTechTorontoAiBuildEvent.Domain.Participants.Participant?> FindParticipantAsync(byte[] secretDigest, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var sessions = await database.ParticipantSessions.Where(session => !session.Revoked && session.ExpiresAtUtc > nowUtc).ToListAsync(cancellationToken);
        var session = sessions.SingleOrDefault(candidate => System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(candidate.SecretDigest, secretDigest));
        return session is null ? null : await database.Participants.SingleOrDefaultAsync(participant => participant.Id == session.ParticipantId, cancellationToken);
    }

    private static ParticipantProfile Map(FaithTechTorontoAiBuildEvent.Domain.Participants.Participant participant)
        => new(participant.Name, participant.WhatYouMake, participant.OnYourHeart);

    private static string? BlankToNull(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim().Replace("\r\n", "\n", StringComparison.Ordinal);
}
