// Acceptance Test
// Traces to: L2-012, L2-044
// Description: An exact project-update retry is acknowledged without overwriting a later edit, while changed input under the operation identity fails.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProjectUpdateReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ProjectUpdateReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Exact_update_retry_does_not_overwrite_later_project_edit()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" });
        var initial = await client.GetFromJsonAsync<PublicState>("/api/event/state");
        var project = Assert.Single(initial!.Projects);
        var operationId = Guid.NewGuid();
        var firstRequest = new
        {
            operationId,
            expectedVersion = "1",
            input = new { title = "First edit", description = project.Description, repositoryUrl = (string?)null, demoUrl = (string?)null }
        };
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/admin/projects/{project.Id}", firstRequest)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PutAsJsonAsync($"/api/admin/projects/{project.Id}", new
        {
            operationId = Guid.NewGuid(),
            expectedVersion = "2",
            input = new { title = "Later edit", description = project.Description, repositoryUrl = (string?)null, demoUrl = (string?)null }
        })).StatusCode);

        var replay = await client.PutAsJsonAsync($"/api/admin/projects/{project.Id}", firstRequest);
        var changed = await client.PutAsJsonAsync($"/api/admin/projects/{project.Id}", new
        {
            operationId,
            expectedVersion = "1",
            input = new { title = "Changed retry", description = project.Description, repositoryUrl = (string?)null, demoUrl = (string?)null }
        });
        var final = await client.GetFromJsonAsync<PublicState>("/api/event/state");

        Assert.Equal(HttpStatusCode.OK, replay.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
        Assert.Equal("Later edit", Assert.Single(final!.Projects).Title);
        Assert.Equal("3", final.Version);
    }

    private sealed record PublicState(string Version, IReadOnlyList<ProjectCard> Projects);
    private sealed record ProjectCard(Guid Id, string Title, string Description);
}
