using FaithTechTorontoAiBuildEvent.Application.Access;
using FaithTechTorontoAiBuildEvent.Domain.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Access;

public sealed class SqlAdministratorStore(EventDbContext db, UserManager<AdministratorAccount> users, AuthenticationBudget budget)
    : IAdministratorStore
{
    private static readonly string UnassignedPasswordHash = new PasswordHasher<AdministratorAccount>()
        .HashPassword(new AdministratorAccount(), Guid.NewGuid().ToString());

    public Task<Guid?> VerifyCredentials(string username, string password, string source, CancellationToken cancellationToken) =>
        budget.Verify("admin:" + username.Trim().ToUpperInvariant(), source, async () => {
        var account = await users.FindByNameAsync(username.Trim());
        if (account is null)
        {
            users.PasswordHasher.VerifyHashedPassword(new AdministratorAccount(), UnassignedPasswordHash, password);
            return null;
        }
        if (!await users.CheckPasswordAsync(account, password)) return null;
        return account.Enabled && await users.IsInRoleAsync(account, "Administrator") ? account.Id : null;
    }, cancellationToken);

    public async Task<bool> IsEnabledAdministrator(Guid id, CancellationToken cancellationToken)
    {
        var account = await users.FindByIdAsync(id.ToString());
        return account is { Enabled: true } && await users.IsInRoleAsync(account, "Administrator");
    }

    public Task<AdministratorSession?> FindSession(Guid id, CancellationToken cancellationToken) =>
        db.AdministratorSessions.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task SaveSession(AdministratorSession session, CancellationToken cancellationToken)
    {
        if (db.Entry(session).State == EntityState.Detached) db.AdministratorSessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);
    }

    public Task<DateTimeOffset> GetUtcNow(CancellationToken cancellationToken) =>
        db.Database.SqlQuery<DateTimeOffset>($"SELECT TODATETIMEOFFSET(SYSUTCDATETIME(), '+00:00') AS Value")
            .SingleAsync(cancellationToken);

    public Task RevokeSession(Guid sessionId, CancellationToken cancellationToken) =>
        db.AdministratorSessions.Where(x => x.Id == sessionId)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.Revoked, true), cancellationToken);

    public async Task<bool> RecordInteraction(Guid sessionId, CancellationToken cancellationToken)
    {
        var now = await GetUtcNow(cancellationToken);
        return await db.AdministratorSessions.Where(x => x.Id == sessionId && !x.Revoked &&
                x.AuthenticatedAtUtc > now.AddHours(-8) && x.LastInteractionAtUtc > now.AddMinutes(-30))
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.LastInteractionAtUtc,
                x => x.LastInteractionAtUtc > now ? x.LastInteractionAtUtc : now), cancellationToken) == 1;
    }
}
