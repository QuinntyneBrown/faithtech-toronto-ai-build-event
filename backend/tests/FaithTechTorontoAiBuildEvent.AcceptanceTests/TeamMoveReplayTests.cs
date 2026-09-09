// Acceptance Test
// Traces to: L2-009, L2-044
// Description: Replaying a move to a new team does not create another team or membership, while changed input under the operation identity fails.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class TeamMoveReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public TeamMoveReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Exact_new_team_move_retry_does_not_duplicate_team_or_membership()
    {
        using var entrant = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await entrant.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        var entry = await entrant.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt!.OperationId, expectedVersion = "0", input = new { email = "move-replay@example.com" } });
        var participant = await entry.Content.ReadFromJsonAsync<EntryResponse>();
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "1", fromScreen = "countdown", toScreen = "projects" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "2", fromScreen = "projects", toScreen = "teams" });
        var operationId = Guid.NewGuid();
        var request = new { operationId, expectedVersion = "3", participantId = participant!.ParticipantId, destination = "new", teamId = (Guid?)null };

        Assert.Equal(HttpStatusCode.NoContent, (await administrator.PostAsJsonAsync("/api/admin/teams/moves", request)).StatusCode);
        var replay = await administrator.PostAsJsonAsync("/api/admin/teams/moves", request);
        var changed = await administrator.PostAsJsonAsync("/api/admin/teams/moves", new
        {
            operationId,
            expectedVersion = "4",
            participantId = participant.ParticipantId,
            destination = "unassigned",
            teamId = (Guid?)null
        });
        var state = await administrator.GetFromJsonAsync<PublicState>("/api/event/state");

        Assert.Equal(HttpStatusCode.NoContent, replay.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
        Assert.Equal(2, state!.Teams.Count);
        Assert.Equal(1, state.Teams.Sum(team => team.Members.Count(member => member.Label == participant.PublicLabel)));
        Assert.Equal("4", state.Version);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record EntryResponse(Guid ParticipantId, string PublicLabel);
    private sealed record PublicState(string Version, IReadOnlyList<PublicTeam> Teams);
    private sealed record PublicTeam(IReadOnlyList<Member> Members);
    private sealed record Member(Guid Id, string Label);
}
