using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class SqlAdministratorAuthorizationStore(CompanionDbContext database) : IAdministratorAuthorizationStore
{
    public async Task<bool> IsAuthorizedAsync(byte[] secretDigest, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var credential = await database.CompanionCredentials.SingleOrDefaultAsync(cancellationToken);
        if (credential is null)
        {
            return false;
        }

        var sessions = await database.AdministratorSessions.Where(session => !session.Revoked).ToListAsync(cancellationToken);
        var session = sessions.SingleOrDefault(candidate => System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(candidate.SecretDigest, secretDigest));
        return session is not null
            && session.CredentialRevision == credential.Revision
            && session.CreatedAtUtc.AddHours(8) > nowUtc
            && session.LastInteractionAtUtc.AddMinutes(30) > nowUtc;
    }
}
