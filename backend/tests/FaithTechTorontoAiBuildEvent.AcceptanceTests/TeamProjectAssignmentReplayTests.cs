// Acceptance Test
// Traces to: L2-009, L2-011, L2-044
// Description: Replaying an earlier team-project assignment does not overwrite a later clear, while changed input under the operation identity fails.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class TeamProjectAssignmentReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public TeamProjectAssignmentReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Exact_assignment_retry_does_not_overwrite_later_clear()
    {
        using var entrant = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await entrant.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        await entrant.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt!.OperationId, expectedVersion = "0", input = new { email = "assignment-replay@example.com" } });
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "1", fromScreen = "countdown", toScreen = "projects" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "2", fromScreen = "projects", toScreen = "teams" });
        var state = await administrator.GetFromJsonAsync<PublicState>("/api/event/state");
        var teamId = Assert.Single(state!.Teams).Id;
        var projectId = Assert.Single(state.Projects).Id;
        var operationId = Guid.NewGuid();
        var request = new { operationId, expectedVersion = "3", projectId = (Guid?)projectId };

        Assert.Equal(HttpStatusCode.NoContent, (await administrator.PutAsJsonAsync($"/api/admin/teams/{teamId}/project", request)).StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, (await administrator.PutAsJsonAsync($"/api/admin/teams/{teamId}/project", new { operationId = Guid.NewGuid(), expectedVersion = "4", projectId = (Guid?)null })).StatusCode);
        var replay = await administrator.PutAsJsonAsync($"/api/admin/teams/{teamId}/project", request);
        var changed = await administrator.PutAsJsonAsync($"/api/admin/teams/{teamId}/project", new { operationId, expectedVersion = "4", projectId = (Guid?)null });
        var final = await administrator.GetFromJsonAsync<PublicState>("/api/event/state");

        Assert.Equal(HttpStatusCode.NoContent, replay.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
        Assert.Null(Assert.Single(final!.Teams).ProjectId);
        Assert.Equal("5", final.Version);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record PublicState(string Version, IReadOnlyList<ProjectCard> Projects, IReadOnlyList<PublicTeam> Teams);
    private sealed record ProjectCard(Guid Id);
    private sealed record PublicTeam(Guid Id, Guid? ProjectId);
}
