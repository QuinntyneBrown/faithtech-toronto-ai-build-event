// Acceptance Test
// Traces to: L2-002, L2-040
// Description: Administrator roster edits reject over-limit optional fields atomically.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorParticipantValidationTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AdministratorParticipantValidationTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Over_limit_administrator_edit_is_rejected_without_partial_changes()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        var added = await client.PostAsJsonAsync("/api/admin/participants", new { operationId = Guid.NewGuid(), expectedVersion = "0", email = "unchanged@example.com" });
        var participant = await added.Content.ReadFromJsonAsync<ParticipantResponse>();

        var response = await client.PutAsJsonAsync($"/api/admin/participants/{participant!.Id}", new
        {
            operationId = Guid.NewGuid(), expectedVersion = "1",
            input = new { email = "changed@example.com", name = new string('x', 201), whatYouMake = (string?)null, onYourHeart = (string?)null }
        });
        var roster = await client.GetFromJsonAsync<IReadOnlyList<ParticipantResponse>>("/api/admin/participants");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("unchanged@example.com", Assert.Single(roster!).Email);
    }

    private sealed record ParticipantResponse(Guid Id, string Email);
}
