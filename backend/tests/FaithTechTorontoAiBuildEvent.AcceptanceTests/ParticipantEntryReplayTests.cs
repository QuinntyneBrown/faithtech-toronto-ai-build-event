// Acceptance Test
// Traces to: L2-044
// Description: Retrying an entry with its browser-scoped receipt returns the saved entry and establishes a valid private session.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ParticipantEntryReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ParticipantEntryReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Retrying_the_same_entry_receipt_returns_the_saved_identity_and_a_valid_session()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await client.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        var request = new { operationId = receipt.OperationId, expectedVersion = "0", input = new { email = "retry@example.com" } };

        var original = await client.PostAsJsonAsync("/api/participant/entries", request);
        var replay = await client.PostAsJsonAsync("/api/participant/entries", request);
        var session = await client.GetAsync("/api/participant/session");

        Assert.Equal(HttpStatusCode.OK, original.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal((await original.Content.ReadFromJsonAsync<EntryResponse>())?.PublicLabel, (await replay.Content.ReadFromJsonAsync<EntryResponse>())?.PublicLabel);
        Assert.Equal(HttpStatusCode.OK, session.StatusCode);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record EntryResponse(string PublicLabel);
}
