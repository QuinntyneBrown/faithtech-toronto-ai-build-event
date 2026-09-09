// Acceptance Test
// Traces to: L2-042
// Description: Invalid public entry submissions consume the same durable per-source budget as valid entries.

using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class PublicEntryThrottleTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public PublicEntryThrottleTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Sixty_first_public_entry_attempt_is_throttled()
    {
        using var client = factory.CreateClient();
        for (var attempt = 0; attempt < 60; attempt++)
        {
            var rejected = await client.PostAsJsonAsync("/api/participant/entries", new { operationId = Guid.NewGuid(), expectedVersion = "0", input = new { email = "invalid@example.com" } });
            Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        }

        var throttled = await client.PostAsJsonAsync("/api/participant/entries", new { operationId = Guid.NewGuid(), expectedVersion = "0", input = new { email = "invalid@example.com" } });

        Assert.Equal((HttpStatusCode)429, throttled.StatusCode);
        Assert.True(int.Parse(throttled.Headers.GetValues("Retry-After").Single()) > 0);
    }
}
