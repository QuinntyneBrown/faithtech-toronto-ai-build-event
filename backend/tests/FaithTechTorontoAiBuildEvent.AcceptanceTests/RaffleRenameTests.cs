// Acceptance Test
// Traces to: L2-014, L2-020, L2-039
// Description: Renaming a participant rewrites retained public raffle labels while preserving stable identity.

using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class RaffleRenameTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public RaffleRenameTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Administrator_rename_updates_retained_winner_and_candidate_labels()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        var added = await client.PostAsJsonAsync("/api/admin/participants", new { operationId = Guid.NewGuid(), expectedVersion = "0", email = "winner@example.com" });
        var participant = await added.Content.ReadFromJsonAsync<ParticipantResponse>();
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "1", fromScreen = "countdown", toScreen = "projects" });
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "2", fromScreen = "projects", toScreen = "teams" });
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "3", fromScreen = "teams", toScreen = "raffle" });
        await client.PostAsJsonAsync("/api/admin/raffle/draws", new { operationId = Guid.NewGuid(), expectedVersion = "4" });

        var renamed = string.Concat(Enumerable.Repeat("😀", 200));
        await client.PutAsJsonAsync($"/api/admin/participants/{participant!.Id}", new
        {
            operationId = Guid.NewGuid(),
            expectedVersion = "5",
            input = new { email = participant.Email, name = renamed, whatYouMake = (string?)null, onYourHeart = (string?)null }
        });
        var raffle = await client.GetFromJsonAsync<RaffleSnapshotResponse>("/api/event/raffle");

        Assert.Equal($"{renamed} (Participant 001)", raffle!.LatestResult!.WinnerLabel);
        Assert.Equal([$"{renamed} (Participant 001)"], raffle.LatestResult.CandidateLabels);
    }

    private sealed record ParticipantResponse(Guid Id, string Email);
    private sealed record RaffleSnapshotResponse(RaffleResultResponse? LatestResult);
    private sealed record RaffleResultResponse(string WinnerLabel, IReadOnlyList<string> CandidateLabels);
}
