// Acceptance Test
// Traces to: L2-004
// Description: Clearing a participant session revokes private access without deleting the raffle identity.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ClearParticipantSessionTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ClearParticipantSessionTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Clearing_session_removes_private_access()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receiptResponse = await client.PostAsync("/api/participant/entry-receipt", null);
        var receipt = await receiptResponse.Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        await client.PostAsJsonAsync("/api/participant/entries", new
        {
            operationId = receipt.OperationId,
            expectedVersion = "0",
            input = new { email = "clear-session@example.com" }
        });

        var clear = await client.DeleteAsync("/api/participant/session");
        var restored = await client.GetAsync("/api/participant/session");

        Assert.Equal(HttpStatusCode.NoContent, clear.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, restored.StatusCode);
    }

    private sealed record ReceiptResponse(Guid OperationId);
}
