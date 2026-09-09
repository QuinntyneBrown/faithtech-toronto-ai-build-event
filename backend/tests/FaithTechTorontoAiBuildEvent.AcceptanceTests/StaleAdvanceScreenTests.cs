// Acceptance Test
// Traces to: L2-044
// Description: An authorized transition using an obsolete event version is a conflict, not an authentication failure.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class StaleAdvanceScreenTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public StaleAdvanceScreenTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Stale_screen_transition_reports_a_conflict()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" });

        var stale = await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" });

        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);
    }
}
