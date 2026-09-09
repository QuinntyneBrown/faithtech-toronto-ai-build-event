// Acceptance Test
// Traces to: L2-009
// Description: An administrator moves a formed-team member to Unassigned without changing another member.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class TeamMoveTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public TeamMoveTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Administrator_moves_member_to_unassigned()
    {
        Guid participantId = Guid.Empty;
        for (var index = 0; index < 2; index++)
        {
            using var entrant = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
            var receipt = await (await entrant.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
            Assert.NotNull(receipt);
            var entry = await entrant.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt.OperationId, expectedVersion = index.ToString(), input = new { email = $"move-{index}@example.com" } });
            participantId = index == 0 ? (await entry.Content.ReadFromJsonAsync<EntryResponse>())!.ParticipantId : participantId;
        }

        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "2", fromScreen = "countdown", toScreen = "projects" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "3", fromScreen = "projects", toScreen = "teams" });

        var moved = await administrator.PostAsJsonAsync("/api/admin/teams/moves", new { operationId = Guid.NewGuid(), expectedVersion = "4", participantId, destination = "unassigned", teamId = (Guid?)null });
        var state = await administrator.GetFromJsonAsync<PublicState>("/api/event/state");

        Assert.Equal(HttpStatusCode.NoContent, moved.StatusCode);
        Assert.Contains(state!.UnassignedMembers, member => member.Id == participantId);
        Assert.DoesNotContain(state.Teams.SelectMany(team => team.Members), member => member.Id == participantId);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record EntryResponse(Guid ParticipantId);
    private sealed record PublicState(IReadOnlyList<Team> Teams, IReadOnlyList<Member> UnassignedMembers);
    private sealed record Team(IReadOnlyList<Member> Members);
    private sealed record Member(Guid Id, string Label);
}
