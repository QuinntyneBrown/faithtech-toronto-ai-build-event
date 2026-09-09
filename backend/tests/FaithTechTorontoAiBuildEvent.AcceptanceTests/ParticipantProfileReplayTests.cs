// Acceptance Test
// Traces to: L2-044
// Description: An unchanged participant-profile retry returns its saved result without applying the mutation twice.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ParticipantProfileReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ParticipantProfileReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Saved_profile_can_be_retried_with_its_original_operation_identity()
    {
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await client.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        await client.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt.OperationId, expectedVersion = "0", input = new { email = "profile-retry@example.com" } });
        var operationId = Guid.NewGuid();
        var request = new { operationId, expectedVersion = "1", input = new { name = "Taylor", whatYouMake = "Software", onYourHeart = "Belonging" } };

        var original = await client.PutAsJsonAsync("/api/participant/profile", request);
        var replay = await client.PutAsJsonAsync("/api/participant/profile", request);
        var changed = await client.PutAsJsonAsync("/api/participant/profile", new { operationId, expectedVersion = "1", input = new { name = "Changed", whatYouMake = "Software", onYourHeart = "Belonging" } });

        Assert.Equal(HttpStatusCode.OK, original.StatusCode);
        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
    }

    private sealed record ReceiptResponse(Guid OperationId);
}
