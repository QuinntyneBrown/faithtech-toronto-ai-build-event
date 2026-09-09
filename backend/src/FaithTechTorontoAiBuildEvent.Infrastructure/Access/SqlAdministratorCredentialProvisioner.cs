using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Domain.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class SqlAdministratorCredentialProvisioner(CompanionDbContext database, PasscodeVerifier verifier) : IAdministratorCredentialProvisioner
{
    public async Task ProvisionAsync(string passcode, CancellationToken cancellationToken)
    {
        var credential = await database.CompanionCredentials.SingleOrDefaultAsync(cancellationToken);
        var (salt, hash) = verifier.Create(passcode);
        if (credential is null)
        {
            database.CompanionCredentials.Add(new CompanionCredential
            {
                Salt = salt,
                Verifier = hash,
                Revision = 1,
                ChangedAtUtc = DateTimeOffset.UtcNow
            });
        }
        else
        {
            credential.Salt = salt;
            credential.Verifier = hash;
            credential.Revision++;
            credential.ChangedAtUtc = DateTimeOffset.UtcNow;
            var sessions = await database.AdministratorSessions.Where(session => !session.Revoked).ToListAsync(cancellationToken);
            foreach (var session in sessions)
            {
                session.Revoked = true;
            }
        }
        await database.SaveChangesAsync(cancellationToken);
    }
}
