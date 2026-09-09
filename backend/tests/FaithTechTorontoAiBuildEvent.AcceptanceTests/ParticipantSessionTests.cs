// Acceptance Test
// Traces to: L2-004, L2-039, L2-041
// Description: Only the browser that entered an email can restore its private participant identity.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ParticipantSessionTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ParticipantSessionTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Only_entry_browser_can_restore_its_participant_label()
    {
        using var owner = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receiptResponse = await owner.PostAsync("/api/participant/entry-receipt", null);
        var receipt = await receiptResponse.Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        await owner.PostAsJsonAsync("/api/participant/entries", new
        {
            operationId = receipt.OperationId,
            expectedVersion = "0",
            input = new { email = "session-owner@example.com" }
        });

        var ownerResponse = await owner.GetAsync("/api/participant/session");
        using var otherBrowser = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var otherResponse = await otherBrowser.GetAsync("/api/participant/session");

        Assert.Equal(HttpStatusCode.OK, ownerResponse.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, otherResponse.StatusCode);
    }

    private sealed record ReceiptResponse(Guid OperationId);
}
