using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProvisioningMigrationTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact]
    public async Task Given_a_later_migration_failure_when_applied_then_prior_commits_are_reported_as_partial()
    {
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<EventDbContext>().Database;
        await database.EnsureDeletedAsync(); await database.GetService<IRelationalDatabaseCreator>().CreateAsync();
        await database.ExecuteSqlRawAsync("CREATE TRIGGER RejectOperatorTable ON DATABASE FOR CREATE_TABLE AS BEGIN IF EVENTDATA().value('(/EVENT_INSTANCE/ObjectName)[1]', 'nvarchar(128)') = 'OperatorIdentities' THROW 51000, 'private-migration-diagnostic', 1; END");
        using var files = new OperatorCliFixture(factory);
        var preview = await ProvisioningProcess.Execute(factory, ["migrate", "--preview", .. files.Options]);
        Assert.Equal(0, preview.ExitCode);
        var review = JsonDocument.Parse(preview.Output).RootElement;
        var id = review.GetProperty("previewId").GetGuid().ToString();
        try {
            var applied = await ProvisioningProcess.Execute(factory, ["operations", "apply", id, "--approve", id, .. files.Options]);
            Assert.True(applied.ExitCode == 1, applied.Output + applied.Error);
            Assert.Equal("partial", JsonDocument.Parse(applied.Output).RootElement.GetProperty("outcome").GetString());
            Assert.DoesNotContain("private-migration-diagnostic", applied.Output + applied.Error);
            var reconciled = await ProvisioningProcess.Execute(factory, ["operations", "reconcile", review.GetProperty("operationId").GetGuid().ToString(), .. files.Options]);
            Assert.Equal(1, reconciled.ExitCode);
            Assert.Equal("partial", JsonDocument.Parse(reconciled.Output).RootElement.GetProperty("outcome").GetString());
        } finally { await database.ExecuteSqlRawAsync("DROP TRIGGER RejectOperatorTable ON DATABASE"); }
    }

    [Fact, Trait("Requirement", "L2-049/AC3")]
    public async Task Given_an_empty_database_when_the_cli_migrates_twice_then_the_schema_is_current()
    {
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<EventDbContext>().Database;
        await database.EnsureDeletedAsync();
        await database.GetService<IRelationalDatabaseCreator>().CreateAsync();
        using var files = new OperatorCliFixture(factory);
        for (var attempt = 0; attempt < 2; attempt++) {
            var preview = await ProvisioningProcess.Execute(factory, ["migrate", "--preview", .. files.Options]);
            Assert.True(preview.ExitCode == 0, preview.Output + preview.Error);
            var id = JsonDocument.Parse(preview.Output).RootElement.GetProperty("previewId").GetGuid().ToString();
            var applied = await ProvisioningProcess.Execute(factory, ["operations", "apply", id, "--approve", id, .. files.Options]);
            Assert.True(applied.ExitCode == 0, applied.Output + applied.Error);
            Assert.Empty(await database.GetPendingMigrationsAsync());
        }
    }
}
