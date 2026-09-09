// Acceptance Test
// Traces to: L2-007, L2-044
// Description: An exact screen-advance retry returns its committed outcome, while changed input under the operation identity fails.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdvanceScreenReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AdvanceScreenReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Exact_retry_returns_original_transition_without_advancing_again()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        var operationId = Guid.NewGuid();
        var request = new { operationId, expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" };

        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/admin/event/advance", request)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/admin/event/advance", request)).StatusCode);
        var changed = await client.PostAsJsonAsync("/api/admin/event/advance", new
        {
            operationId,
            expectedVersion = "1",
            fromScreen = "projects",
            toScreen = "teams"
        });

        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
        var state = await client.GetFromJsonAsync<PublicState>("/api/event/state");
        Assert.NotNull(state);
        Assert.Equal("projects", state.CurrentScreen);
        Assert.Equal("1", state.Version);
    }

    private sealed record PublicState(string CurrentScreen, string Version);
}
