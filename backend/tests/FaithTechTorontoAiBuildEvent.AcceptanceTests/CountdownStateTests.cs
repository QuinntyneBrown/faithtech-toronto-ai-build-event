// Acceptance Test
// Traces to: L2-005, L2-011, L2-045
// Description: A new companion exposes its Countdown state and seeded RTR project through the public API.

using System.Net;
using System.Net.Http.Json;
using Xunit;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using FaithTechTorontoAiBuildEvent.Application.Access;
using Microsoft.Data.SqlClient;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class CountdownStateTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public CountdownStateTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task New_companion_exposes_countdown_and_seeded_rtr_project()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/event/state");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var state = await response.Content.ReadFromJsonAsync<PublicState>();
        Assert.NotNull(state);
        Assert.Equal("countdown", state.CurrentScreen);
        Assert.Equal("2026-09-09T21:20:00+00:00", state.CountdownTargetUtc);
        var project = Assert.Single(state.Projects);
        Assert.Equal("RTR — Reconciliation Through Relationships", project.Title);
        Assert.Null(project.RepositoryUrl);
        Assert.Null(project.DemoUrl);
    }

    private sealed record PublicState(string CurrentScreen, string CountdownTargetUtc, IReadOnlyList<Project> Projects);
    private sealed record Project(string Title, string? RepositoryUrl, string? DemoUrl);
}

public sealed class CountdownApiFactory : WebApplicationFactory<Program>
{
    private readonly string databaseName = Guid.NewGuid().ToString();
    private readonly string? sqlServerConnection = Environment.GetEnvironmentVariable("FAITHTECH_TEST_SQL");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Companion"] = "Server=unused;Database=unused;Encrypt=true;TrustServerCertificate=false",
            ["Security:DigestKey"] = "MDEyMzQ1Njc4OUFCQ0RFRjAxMjM0NTY3ODlBQkNERUY="
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CompanionDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<CompanionDbContext>>();
            if (sqlServerConnection is null)
            {
                services.AddDbContext<CompanionDbContext>(options => options.UseInMemoryDatabase(databaseName));
            }
            else
            {
                var connection = new SqlConnectionStringBuilder(sqlServerConnection)
                {
                    InitialCatalog = $"FaithTechAcceptance_{databaseName.Replace("-", string.Empty)}"
                };
                services.AddDbContext<CompanionDbContext>(options => options.UseSqlServer(connection.ConnectionString));
            }
        });
    }

    public async Task ProvisionAdministratorPasscodeAsync(string passcode)
    {
        using var scope = Services.CreateScope();
        var provisioner = scope.ServiceProvider.GetRequiredService<IAdministratorCredentialProvisioner>();
        await provisioner.ProvisionAsync(passcode, CancellationToken.None);
    }

    public async Task ClearAdministratorLoginAttemptsAsync()
    {
        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<CompanionDbContext>();
        database.AdministratorLoginAttempts.RemoveRange(database.AdministratorLoginAttempts);
        await database.SaveChangesAsync();
    }

    public async Task RecordAdministratorLoginFailuresAsync(int count)
    {
        using var scope = Services.CreateScope();
        var attempts = scope.ServiceProvider.GetRequiredService<IAdministratorLoginAttemptStore>();
        for (var index = 0; index < count; index++) await attempts.RecordFailureAsync($"source-{index}", DateTimeOffset.UtcNow, CancellationToken.None);
    }

    public async Task SetAdministratorLastInteractionAsync(DateTimeOffset interactionAtUtc)
    {
        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<CompanionDbContext>();
        var session = await database.AdministratorSessions.SingleAsync(session => !session.Revoked);
        session.LastInteractionAtUtc = interactionAtUtc;
        await database.SaveChangesAsync();
    }

    public async Task<string> GetLastPublicEntrySourceAsync()
    {
        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<CompanionDbContext>();
        return await database.PublicEntryAttempts.OrderByDescending(attempt => attempt.AttemptedAtUtc).Select(attempt => attempt.Source).FirstAsync();
    }

    public async Task<DateTimeOffset> GetAdministratorLastInteractionAsync()
    {
        using var scope = Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<CompanionDbContext>();
        return (await database.AdministratorSessions.SingleAsync(session => !session.Revoked)).LastInteractionAtUtc;
    }
}
