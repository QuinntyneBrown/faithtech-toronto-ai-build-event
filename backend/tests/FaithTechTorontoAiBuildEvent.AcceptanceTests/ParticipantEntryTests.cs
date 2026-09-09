// Acceptance Test
// Traces to: L2-003, L2-004, L2-040
// Description: A valid new email creates one raffle identity and a private participant session.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ParticipantEntryTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ParticipantEntryTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Unused_normalized_email_creates_one_raffle_identity_and_private_session()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receiptResponse = await client.PostAsync("/api/participant/entry-receipt", null);
        var receipt = await receiptResponse.Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);

        var response = await client.PostAsJsonAsync("/api/participant/entries", new
        {
            operationId = receipt.OperationId,
            expectedVersion = "0",
            input = new { email = "  participant+toronto@Example.com  " }
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var entry = await response.Content.ReadFromJsonAsync<EntryResponse>();
        Assert.NotNull(entry);
        Assert.Equal("Participant 001", entry.PublicLabel);
        Assert.Contains("faithtech-participant=", response.Headers.GetValues("Set-Cookie").Single(), StringComparison.Ordinal);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record EntryResponse(string PublicLabel);
}
