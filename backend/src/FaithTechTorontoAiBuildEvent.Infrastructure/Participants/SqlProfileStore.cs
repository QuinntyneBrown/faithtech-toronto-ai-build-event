using FaithTechTorontoAiBuildEvent.Application.Participants;
using FaithTechTorontoAiBuildEvent.Domain.EventFlow;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Participants;

public sealed class SqlProfileStore(CompanionDbContext database) : IProfileStore
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

        var state = await database.EventStates.SingleAsync(cancellationToken);
        if (state.CurrentScreen != EventScreen.Countdown || state.Version != request.ExpectedVersion)
        {
            throw new EntryValidationException("Countdown is no longer available; ask an administrator for help.");
        }

        participant.Name = BlankToNull(request.Input.Name);
        participant.WhatYouMake = BlankToNull(request.Input.WhatYouMake);
        participant.OnYourHeart = BlankToNull(request.Input.OnYourHeart);
        state.Version++;
        await database.SaveChangesAsync(cancellationToken);
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
