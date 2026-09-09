using FaithTechTorontoAiBuildEvent.Application.Operations;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace FaithTechTorontoAiBuildEvent.Infrastructure.Operations;

public sealed class SqlOperatorMigrations(EventDbContext db)
{
    public async Task<string[]> Apply(OperatorPreview preview, Action<string> checkpoint, CancellationToken token)
    {
        await using var command = db.Database.GetDbConnection().CreateCommand();
        command.CommandTimeout = db.Database.GetCommandTimeout() ?? 60;
        command.CommandText = "DECLARE @result int; EXEC @result = sp_getapplock @Resource='faithtech:operator:migrations', @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=60000; IF @result < 0 THROW 51000, 'Migration lock unavailable', 1;";
        await command.ExecuteNonQueryAsync(token);
        try {
            var applied = (await db.Database.GetAppliedMigrationsAsync(token)).ToArray();
            var pending = (await db.Database.GetPendingMigrationsAsync(token)).ToArray();
            if (!applied.SequenceEqual(preview.AppliedMigrations) || !pending.SequenceEqual(preview.PendingMigrations))
                throw new OperationConflictException();
            foreach (var migration in pending) {
                await db.GetService<IMigrator>().MigrateAsync(migration, token);
                checkpoint(migration);
            }
            return (await db.Database.GetAppliedMigrationsAsync(token)).ToArray();
        } finally {
            command.CommandText = "EXEC sp_releaseapplock @Resource='faithtech:operator:migrations', @LockOwner='Session';";
            using var cleanup = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await command.ExecuteNonQueryAsync(cleanup.Token);
        }
    }
}
