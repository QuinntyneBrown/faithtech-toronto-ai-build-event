// Acceptance Test
// Traces to: L2-012, L2-044
// Description: An exact project-add retry returns its original identity after later edits, while changed input under the operation identity fails.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProjectReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ProjectReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Exact_add_retry_returns_original_project_without_duplicate()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" });
        var operationId = Guid.NewGuid();
        var request = new
        {
            operationId,
            expectedVersion = "1",
            input = new { title = "Community map", description = "Map relationships.", repositoryUrl = (string?)null, demoUrl = (string?)null }
        };
        var first = await client.PostAsJsonAsync("/api/admin/projects", request);
        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var projectId = await first.Content.ReadFromJsonAsync<Guid>();
        await client.PutAsJsonAsync($"/api/admin/projects/{projectId}", new
        {
            operationId = Guid.NewGuid(),
            expectedVersion = "2",
            input = new { title = "Updated map", description = "Map relationships.", repositoryUrl = (string?)null, demoUrl = (string?)null }
        });

        var replay = await client.PostAsJsonAsync("/api/admin/projects", request);
        var replayedId = await replay.Content.ReadFromJsonAsync<Guid>();
        var changed = await client.PostAsJsonAsync("/api/admin/projects", new
        {
            operationId,
            expectedVersion = "1",
            input = new { title = "Different", description = "Map relationships.", repositoryUrl = (string?)null, demoUrl = (string?)null }
        });
        var state = await client.GetFromJsonAsync<PublicState>("/api/event/state");

        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(projectId, replayedId);
        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
        Assert.NotNull(state);
        Assert.Single(state.Projects, project => project.Id == projectId);
        Assert.Equal("3", state.Version);
    }

    private sealed record PublicState(string Version, IReadOnlyList<ProjectCard> Projects);
    private sealed record ProjectCard(Guid Id);
}
