using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProvisioningMigrationTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
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
