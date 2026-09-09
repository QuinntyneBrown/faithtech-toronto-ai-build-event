// Acceptance Test
// Traces to: L2-007
// Description: An authorized presenter closes Countdown and opens Projects in one persisted transition.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdvanceScreenTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AdvanceScreenTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Administrator_closes_countdown_and_opens_projects()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });

        var transition = await client.PostAsJsonAsync("/api/admin/event/advance", new
        {
            operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects"
        });
        var state = await client.GetFromJsonAsync<PublicState>("/api/event/state");
        using var scope = factory.Services.CreateScope();
        var changes = scope.ServiceProvider.GetRequiredService<CompanionDbContext>().EventChanges.Select(change => change.Version).ToList();

        Assert.Equal(HttpStatusCode.OK, transition.StatusCode);
        Assert.NotNull(state);
        Assert.Equal("projects", state.CurrentScreen);
        Assert.Contains(1, changes);
    }

    private sealed record PublicState(string CurrentScreen);
}
