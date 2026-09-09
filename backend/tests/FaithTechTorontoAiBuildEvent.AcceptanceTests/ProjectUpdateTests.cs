// Acceptance Test
// Traces to: L2-012
// Description: An administrator can edit and remove a project without leaving stale cards behind.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProjectUpdateTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ProjectUpdateTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Administrator_updates_then_removes_a_project_card()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" });
        var added = await client.PostAsJsonAsync("/api/admin/projects", new { operationId = Guid.NewGuid(), expectedVersion = "1", input = new { title = "Original", description = "Original description", repositoryUrl = (string?)null, demoUrl = (string?)null } });
        var projectId = await added.Content.ReadFromJsonAsync<Guid>();

        var update = await client.PutAsJsonAsync($"/api/admin/projects/{projectId}", new { operationId = Guid.NewGuid(), expectedVersion = "2", input = new { title = "Updated", description = "Updated description", repositoryUrl = "https://example.com/repository", demoUrl = (string?)null } });
        var removal = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/projects/{projectId}") { Content = JsonContent.Create(new { operationId = Guid.NewGuid(), expectedVersion = "3" }) });
        var state = await client.GetFromJsonAsync<PublicState>("/api/event/state");

        Assert.Equal(HttpStatusCode.OK, update.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, removal.StatusCode);
        Assert.NotNull(state);
        Assert.DoesNotContain(state.Projects, project => project.Title is "Original" or "Updated");
    }

    private sealed record PublicState(IReadOnlyList<ProjectCard> Projects);
    private sealed record ProjectCard(string Title);
}
