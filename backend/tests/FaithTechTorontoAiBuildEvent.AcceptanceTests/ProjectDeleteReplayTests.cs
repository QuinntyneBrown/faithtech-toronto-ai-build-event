// Acceptance Test
// Traces to: L2-012, L2-044
// Description: An exact project-delete retry succeeds after deletion, while reuse for another project fails without deleting it.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProjectDeleteReplayTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ProjectDeleteReplayTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Exact_delete_retry_succeeds_without_deleting_another_project()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" });
        var initial = await client.GetFromJsonAsync<PublicState>("/api/event/state");
        var projectId = Assert.Single(initial!.Projects).Id;
        var operationId = Guid.NewGuid();
        var request = new { operationId, expectedVersion = "1" };

        Assert.Equal(HttpStatusCode.NoContent, (await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/projects/{projectId}")
        {
            Content = JsonContent.Create(request)
        })).StatusCode);
        var added = await client.PostAsJsonAsync("/api/admin/projects", new
        {
            operationId = Guid.NewGuid(),
            expectedVersion = "2",
            input = new { title = "Retained", description = "Must remain.", repositoryUrl = (string?)null, demoUrl = (string?)null }
        });
        var retainedId = await added.Content.ReadFromJsonAsync<Guid>();

        var replay = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/projects/{projectId}") { Content = JsonContent.Create(request) });
        var changed = await client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/admin/projects/{retainedId}")
        {
            Content = JsonContent.Create(new { operationId, expectedVersion = "3" })
        });
        var final = await client.GetFromJsonAsync<PublicState>("/api/event/state");

        Assert.Equal(HttpStatusCode.NoContent, replay.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, changed.StatusCode);
        Assert.Equal(retainedId, Assert.Single(final!.Projects).Id);
        Assert.Equal("3", final.Version);
    }

    private sealed record PublicState(string Version, IReadOnlyList<ProjectCard> Projects);
    private sealed record ProjectCard(Guid Id);
}
