// Acceptance Test
// Traces to: L2-005
// Description: Countdown clients obtain an authoritative UTC sample rather than relying on their device clock.

using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ServerTimeTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ServerTimeTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Public_time_endpoint_returns_current_utc_time()
    {
        using var client = factory.CreateClient();
        var before = DateTimeOffset.UtcNow;

        var response = await client.GetAsync("/api/event/time");

        var after = DateTimeOffset.UtcNow;
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var result = await response.Content.ReadFromJsonAsync<ServerTime>();
        Assert.NotNull(result);
        Assert.InRange(result.ServerTimeUtc, before, after);
    }

    private sealed record ServerTime(DateTimeOffset ServerTimeUtc);
}
