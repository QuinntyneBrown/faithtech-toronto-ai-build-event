// Acceptance Test
// Traces to: L2-011
// Description: An administrator assigns one saved project to a formed team.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class TeamProjectAssignmentTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;
    public TeamProjectAssignmentTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Administrator_assigns_project_to_a_team()
    {
        using var entrant = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var receipt = await (await entrant.PostAsync("/api/participant/entry-receipt", null)).Content.ReadFromJsonAsync<ReceiptResponse>();
        Assert.NotNull(receipt);
        await entrant.PostAsJsonAsync("/api/participant/entries", new { operationId = receipt.OperationId, expectedVersion = "0", input = new { email = "assignment@example.com" } });
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var administrator = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await administrator.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "1", fromScreen = "countdown", toScreen = "projects" });
        var catalogue = await administrator.GetFromJsonAsync<StateResponse>("/api/event/state");
        Assert.NotNull(catalogue);
        await administrator.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "2", fromScreen = "projects", toScreen = "teams" });
        var state = await administrator.GetFromJsonAsync<StateResponse>("/api/event/state");
        Assert.NotNull(state);

        var assigned = await administrator.PutAsJsonAsync($"/api/admin/teams/{state.Teams[0].Id}/project", new { operationId = Guid.NewGuid(), expectedVersion = "3", projectId = catalogue.Projects[0].Id });

        Assert.Equal(HttpStatusCode.NoContent, assigned.StatusCode);
    }

    private sealed record ReceiptResponse(Guid OperationId);
    private sealed record StateResponse(IReadOnlyList<ProjectResponse> Projects, IReadOnlyList<TeamResponse> Teams);
    private sealed record ProjectResponse(Guid Id);
    private sealed record TeamResponse(Guid Id);
}
