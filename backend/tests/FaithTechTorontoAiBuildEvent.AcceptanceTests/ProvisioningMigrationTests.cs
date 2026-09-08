using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProvisioningMigrationTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-049/AC3")]
    public async Task Given_an_empty_database_when_the_cli_migrates_twice_then_the_schema_is_current()
    {
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<EventDbContext>().Database;
        await database.EnsureDeletedAsync();
        Assert.Equal(0, await ProvisioningProcess.Run(factory, ["migrate"]));
        Assert.Empty(await database.GetPendingMigrationsAsync());
        Assert.Equal(0, await ProvisioningProcess.Run(factory, ["migrate"]));
    }
}
