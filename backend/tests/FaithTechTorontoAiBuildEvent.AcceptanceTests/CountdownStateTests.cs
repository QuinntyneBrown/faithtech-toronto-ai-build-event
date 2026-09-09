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

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(configuration => configuration.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["ConnectionStrings:Companion"] = "Server=unused;Database=unused;Encrypt=true;TrustServerCertificate=false"
        }));
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<CompanionDbContext>>();
            services.RemoveAll<DbContextOptions>();
            services.RemoveAll<IDbContextOptionsConfiguration<CompanionDbContext>>();
            services.AddDbContext<CompanionDbContext>(options => options.UseInMemoryDatabase(databaseName));
        });
    }
}
