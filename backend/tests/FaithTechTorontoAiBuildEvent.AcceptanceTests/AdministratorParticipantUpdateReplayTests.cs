// Acceptance Test
// Traces to: L2-014, L2-044
// Description: An exact administrator-update retry returns its original private outcome without overwriting a later participant edit.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorParticipantUpdateReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AdministratorParticipantUpdateReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Exact_update_retry_returns_original_outcome_without_overwrite()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        var added = await client.PostAsJsonAsync("/api/admin/participants", new { operationId = Guid.NewGuid(), expectedVersion = "0", email = "update-replay@example.com" });
        var participant = await added.Content.ReadFromJsonAsync<ParticipantResponse>();
        var operationId = Guid.NewGuid();
        var firstRequest = new
        {
            operationId,
            expectedVersion = "1",
            input = new { email = participant!.Email, name = "First name", whatYouMake = "First answer", onYourHeart = (string?)null }
        };
        var first = await client.PutAsJsonAsync($"/api/admin/participants/{participant.Id}", firstRequest);
        var original = await first.Content.ReadFromJsonAsync<ParticipantResponse>();
        await client.PutAsJsonAsync($"/api/admin/participants/{participant.Id}", new
        {
            operationId = Guid.NewGuid(),
            expectedVersion = "2",
            input = new { email = participant.Email, name = "Later name", whatYouMake = "Later answer", onYourHeart = (string?)null }
        });

        var replay = await client.PutAsJsonAsync($"/api/admin/participants/{participant.Id}", firstRequest);
        var replayed = await replay.Content.ReadFromJsonAsync<ParticipantResponse>();
        var changed = await client.PutAsJsonAsync($"/api/admin/participants/{participant.Id}", new
        {
            operationId,
            expectedVersion = "1",
            input = new { email = participant.Email, name = "Changed retry", whatYouMake = "First answer", onYourHeart = (string?)null }
        });
        var roster = await client.GetFromJsonAsync<IReadOnlyList<ParticipantResponse>>("/api/admin/participants");

        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(original, replayed);
        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
        Assert.Equal("Later name", Assert.Single(roster!).Name);
    }

    private sealed record ParticipantResponse(Guid Id, string Email, string PublicLabel, string? Name, string? WhatYouMake, string? OnYourHeart, string? TeamLabel, bool HasWonRaffle);
}
