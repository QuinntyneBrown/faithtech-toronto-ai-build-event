// Acceptance Test
// Traces to: L2-002
// Description: An administrator deletes a participant and revokes their private session.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorParticipantDeleteTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;
    public AdministratorParticipantDeleteTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Administrator_deletes_participant_and_revokes_private_session()
    {
        using var entrant = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await entrant.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        var entered = await entrant.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt.OperationId, expectedVersion = "0", input = new { email = "remove@example.com" } });
        var participant = await entered.Content.ReadFromJsonAsync<ParticipantResponse>();
        Assert.NotNull(participant);
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });

        var removed = await administrator.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/participants/{participant.ParticipantId}") { Content = JsonContent.Create(new { operationId = Guid.NewGuid(), expectedVersion = "1" }) });

        Assert.Equal(HttpStatusCode.NoContent, removed.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await entrant.GetAsync("/api/participant/profile")).StatusCode);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record ParticipantResponse(Guid ParticipantId);
}
