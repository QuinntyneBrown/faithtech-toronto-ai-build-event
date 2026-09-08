using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ReadinessTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-045")]
    public async Task Given_a_database_missing_a_migration_when_readiness_is_requested_then_it_reports_not_ready()
    {
        using var client = await factory.AdministratorBrowser();
        using var scope = factory.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<EventDbContext>().Database;
        var migration = (await database.GetAppliedMigrationsAsync()).Last();
        await database.ExecuteSqlRawAsync("UPDATE [__EFMigrationsHistory] SET MigrationId = 'pending-test' WHERE MigrationId = {0}", migration);
        try
        {
            var state = await client.GetFromJsonAsync<JsonElement>("/api/admin/readiness");
            Assert.False(state.GetProperty("ready").GetBoolean());
        }
        finally
        {
            await database.ExecuteSqlRawAsync("UPDATE [__EFMigrationsHistory] SET MigrationId = {0} WHERE MigrationId = 'pending-test'", migration);
        }
    }

    [Fact, Trait("Requirement", "L2-045")]
    public async Task Given_no_session_when_readiness_is_requested_then_access_is_denied()
    {
        using var client = factory.Browser();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/readiness")).StatusCode);
    }

    [Fact, Trait("Requirement", "L2-045")]
    public async Task Given_an_administrator_and_current_database_when_readiness_is_requested_then_the_release_is_ready()
    {
        using var client = await factory.AdministratorBrowser();
        var response = await client.GetAsync("/api/admin/readiness");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("ready").GetBoolean());
        Assert.NotEmpty(body.GetProperty("revision").GetString()!);
        Assert.True(Guid.TryParse(body.GetProperty("instance").GetString(), out _));
    }
}
