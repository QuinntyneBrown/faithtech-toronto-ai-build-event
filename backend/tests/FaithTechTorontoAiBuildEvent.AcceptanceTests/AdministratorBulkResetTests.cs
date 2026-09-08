// Acceptance Test
// Traces to: L2-062
// Description: Bulk reset is atomic and preserves account status and identity.
using FaithTechTorontoAiBuildEvent.Infrastructure.Access;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorBulkResetTests
{
    [Fact, Trait("Requirement", "L2-062/AC2;L2-062/AC3")]
    public async Task Given_multiple_accounts_when_a_later_update_fails_then_all_changes_roll_back()
    {
        var factory = new EventApiFactory();
        await factory.InitializeAsync();
        try
        {
            await factory.ProvisionAdministrator("previous2026!password");
            var second = await factory.ProvisionAdministrator("previous2026!password");
            var removedRole = await factory.ProvisionAdministrator("previous2026!password");
            using var signedIn = await factory.AdministratorBrowser();
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
            var removedId = await db.Users.Where(x => x.UserName == removedRole).Select(x => x.Id).SingleAsync();
            await db.UserRoles.Where(x => x.UserId == removedId).ExecuteDeleteAsync();
            var roleCount = await db.UserRoles.CountAsync();
            var buildEvent = new Domain.Events.BuildEvent { Title = "Synthetic password reset acceptance" };
            var registration = new Domain.Roster.Registration { EventId = buildEvent.Id, DisplayName = "Participant", CodeDigest = new string('a', 64) };
            db.Events.Add(buildEvent);
            db.Registrations.Add(registration);
            await db.SaveChangesAsync();
            var before = await db.Users.AsNoTracking().OrderBy(x => x.UserName).ToArrayAsync();
            var constraint = $"ALTER TABLE dbo.AspNetUsers ADD CONSTRAINT RejectPasswordUpdate CHECK (Id <> '{before[1].Id:D}' OR PasswordHash = '{before[1].PasswordHash}')";
            await db.Database.ExecuteSqlRawAsync(constraint);
            Assert.NotEqual(0, await ProvisioningProcess.Run(factory, ["reset-password", "--all", "--password-stdin"], "faithtech2026!"));
            var afterFailure = await db.Users.AsNoTracking().OrderBy(x => x.UserName).ToArrayAsync();
            Assert.Equal(before.Select(x => x.PasswordHash), afterFailure.Select(x => x.PasswordHash));
            Assert.Equal(before.Select(x => x.SecurityStamp), afterFailure.Select(x => x.SecurityStamp));
            Assert.Equal(System.Net.HttpStatusCode.OK, (await signedIn.GetAsync("/api/admin/session")).StatusCode);
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE dbo.AspNetUsers DROP CONSTRAINT RejectPasswordUpdate");
            await db.Users.Where(x => x.UserName == second).ExecuteUpdateAsync(set => set.SetProperty(x => x.Enabled, false));
            Assert.Equal(0, await ProvisioningProcess.Run(factory, ["reset-password", "--all", "--password-stdin"], "faithtech2026!"));
            var after = await db.Users.AsNoTracking().OrderBy(x => x.UserName).ToArrayAsync();
            Assert.Equal(before.Select(x => x.Id), after.Select(x => x.Id));
            Assert.False(after.Single(x => x.UserName == second).Enabled);
            Assert.Equal(roleCount, await db.UserRoles.CountAsync());
            Assert.False(await db.UserRoles.AnyAsync(x => x.UserId == removedId));
            Assert.False(await db.AdministratorSessions.AnyAsync(x => !x.Revoked));
            Assert.Equal(System.Net.HttpStatusCode.Unauthorized, (await signedIn.GetAsync("/api/admin/session")).StatusCode);
            var participant = await db.Registrations.AsNoTracking().SingleAsync();
            Assert.Equal(registration.CodeDigest, participant.CodeDigest);
            Assert.Equal(registration.CredentialVersion, participant.CredentialVersion);
            var hasher = new Microsoft.AspNetCore.Identity.PasswordHasher<AdministratorAccount>();
            Assert.All(after, account => Assert.NotEqual(Microsoft.AspNetCore.Identity.PasswordVerificationResult.Failed,
                hasher.VerifyHashedPassword(account, account.PasswordHash!, "faithtech2026!")));
        }
        finally { await ((Xunit.IAsyncLifetime)factory).DisposeAsync(); }
    }

    [Fact, Trait("Requirement", "L2-062/AC4")]
    public async Task Given_an_empty_database_when_bulk_reset_then_it_succeeds_and_named_reset_fails()
    {
        var factory = new EventApiFactory();
        await factory.InitializeAsync();
        try
        {
            Assert.Equal(0, await ProvisioningProcess.Run(factory, ["reset-password", "--all", "--password-stdin"], "faithtech2026!"));
            Assert.NotEqual(0, await ProvisioningProcess.Run(factory, ["reset-password", "missing", "--password-stdin"], "faithtech2026!"));
        }
        finally { await ((Xunit.IAsyncLifetime)factory).DisposeAsync(); }
    }
}
