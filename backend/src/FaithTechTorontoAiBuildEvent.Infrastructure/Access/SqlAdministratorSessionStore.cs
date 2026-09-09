using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Domain.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class SqlAdministratorSessionStore(CompanionDbContext database, PasscodeVerifier verifier) : IAdministratorSessionStore
{
    public async Task<AdministratorAuthenticationResult> AuthenticateAsync(string passcode, string sessionSecret, DateTimeOffset nowUtc, CancellationToken cancellationToken)
    {
        var credential = await database.CompanionCredentials.SingleOrDefaultAsync(cancellationToken);
        if (credential is null || !verifier.Verify(credential.Salt, credential.Verifier, passcode))
        {
            return new AdministratorAuthenticationResult(false, null);
        }

        database.AdministratorSessions.Add(new AdministratorSession
        {
            Id = Guid.NewGuid(),
            SecretDigest = System.Security.Cryptography.SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(sessionSecret)),
            CredentialRevision = credential.Revision,
            CreatedAtUtc = nowUtc,
            LastInteractionAtUtc = nowUtc
        });
        await database.SaveChangesAsync(cancellationToken);
        return new AdministratorAuthenticationResult(true, sessionSecret);
    }

    public async Task RevokeAsync(byte[] secretDigest, CancellationToken cancellationToken)
    {
        var sessions = await database.AdministratorSessions.Where(session => !session.Revoked).ToListAsync(cancellationToken);
        var session = sessions.SingleOrDefault(candidate => System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(candidate.SecretDigest, secretDigest));
        if (session is null) return;
        session.Revoked = true;
        await database.SaveChangesAsync(cancellationToken);
    }
}
