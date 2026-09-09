// Acceptance Test
// Traces to: L2-014, L2-044
// Description: An exact administrator-add retry returns its original private outcome after a later edit without creating another participant.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorParticipantAddReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AdministratorParticipantAddReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Exact_add_retry_returns_original_outcome_without_duplicate()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        var operationId = Guid.NewGuid();
        var request = new { operationId, expectedVersion = "0", email = "original@example.com" };
        var first = await client.PostAsJsonAsync("/api/admin/participants", request);
        var original = await first.Content.ReadFromJsonAsync<ParticipantResponse>();
        Assert.NotNull(original);
        await client.PutAsJsonAsync($"/api/admin/participants/{original.Id}", new
        {
            operationId = Guid.NewGuid(),
            expectedVersion = "1",
            input = new { email = "updated@example.com", name = "Updated", whatYouMake = (string?)null, onYourHeart = (string?)null }
        });

        var replay = await client.PostAsJsonAsync("/api/admin/participants", request);
        var replayed = await replay.Content.ReadFromJsonAsync<ParticipantResponse>();
        var changed = await client.PostAsJsonAsync("/api/admin/participants", new { operationId, expectedVersion = "0", email = "changed@example.com" });
        var roster = await client.GetFromJsonAsync<IReadOnlyList<ParticipantResponse>>("/api/admin/participants");

        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(original, replayed);
        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
        Assert.Equal("updated@example.com", Assert.Single(roster!).Email);
    }

    private sealed record ParticipantResponse(Guid Id, string Email, string PublicLabel, string? Name, string? WhatYouMake, string? OnYourHeart, string? TeamLabel, bool HasWonRaffle);
}
