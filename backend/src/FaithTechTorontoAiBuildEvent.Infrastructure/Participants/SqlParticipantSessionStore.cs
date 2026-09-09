using FaithTechTorontoAiBuildEvent.Application.Participants;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Participants;

public sealed class SqlParticipantSessionStore(CompanionDbContext database) : IParticipantSessionStore
{
    public async Task<ParticipantSessionState?> FindActiveAsync(byte[] secretDigest, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var sessions = await database.ParticipantSessions
            .Where(session => !session.Revoked && session.ExpiresAtUtc > nowUtc)
            .ToListAsync(cancellationToken);
        var session = sessions.SingleOrDefault(candidate => System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(candidate.SecretDigest, secretDigest));
        if (session is null)
        {
            return null;
        }

        var participant = await database.Participants.SingleOrDefaultAsync(candidate => candidate.Id == session.ParticipantId, cancellationToken);
        return participant is null ? null : new ParticipantSessionState(participant.Id, participant.PublicLabel);
    }
}
