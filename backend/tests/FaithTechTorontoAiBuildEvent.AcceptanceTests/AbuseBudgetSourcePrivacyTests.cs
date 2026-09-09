// Acceptance Test
// Traces to: L2-041, L2-042
// Description: Durable abuse budgets persist a stable keyed digest instead of the raw request source.

using System.Net.Http.Json;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AbuseBudgetSourcePrivacyTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AbuseBudgetSourcePrivacyTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Public_entry_budget_does_not_store_raw_source()
    {
        using var client = factory.CreateClient();
        await client.PostAsJsonAsync("/api/participant/entries", new { operationId = Guid.NewGuid(), expectedVersion = "0", input = new { email = "invalid@example.com" } });

        var persistedSource = await factory.GetLastPublicEntrySourceAsync();

        Assert.NotEqual("unknown", persistedSource);
        Assert.Matches("^[A-F0-9]{64}$", persistedSource);
    }
}
