// Acceptance Test
// Traces to: L2-002
// Description: An administrator updates a participant's email and optional profile without changing their identity.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorParticipantUpdateTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;
    public AdministratorParticipantUpdateTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Administrator_updates_participant_details()
    {
        using var entrant = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await entrant.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        var entered = await entrant.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt!.OperationId, expectedVersion = "0", input = new { email = "before@example.com" } });
        var participant = await entered.Content.ReadFromJsonAsync<ParticipantResponse>();
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });

        var update = await administrator.PutAsJsonAsync($"/api/admin/participants/{participant!.ParticipantId}", new { operationId = Guid.NewGuid(), expectedVersion = "1", input = new { email = "after@example.com", name = "Updated", whatYouMake = "Tools", onYourHeart = "Hope" } });
        var result = await update.Content.ReadFromJsonAsync<ParticipantResult>();

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(participant.ParticipantId, result!.Id);
        Assert.Equal("after@example.com", result.Email);
        Assert.Equal("Updated", result.Name);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record ParticipantResponse(Guid ParticipantId);
    private sealed record ParticipantResult(Guid Id, string Email, string? Name);
}
