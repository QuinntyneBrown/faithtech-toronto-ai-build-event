// Acceptance Test
// Traces to: L2-045
// Description: Process liveness remains a lightweight endpoint independent of event-state reads.

using System.Net;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class LivenessTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public LivenessTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Liveness_endpoint_returns_success()
    {
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
