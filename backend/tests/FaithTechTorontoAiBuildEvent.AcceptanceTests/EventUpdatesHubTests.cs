// Acceptance Test
// Traces to: L2-044
// Description: Clients can negotiate the event-update SignalR transport.

using System.Net;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class EventUpdatesHubTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;
    public EventUpdatesHubTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Viewer_can_negotiate_event_update_transport()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var response = await client.PostAsync("/hubs/event-updates/negotiate?negotiateVersion=1", null);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
