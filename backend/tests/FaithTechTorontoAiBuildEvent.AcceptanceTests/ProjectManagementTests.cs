// Acceptance Test
// Traces to: L2-011, L2-012
// Description: An administrator adds a project card that becomes visible in the public catalogue.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProjectManagementTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ProjectManagementTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Administrator_adds_project_card_with_optional_https_links()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" });

        var save = await client.PostAsJsonAsync("/api/admin/projects", new
        {
            operationId = Guid.NewGuid(), expectedVersion = "1", input = new
            {
                title = "Neighbourhood map", description = "Map local relationships.", repositoryUrl = "https://example.com/repository", demoUrl = (string?)null
            }
        });
        var state = await client.GetFromJsonAsync<PublicState>("/api/event/state");

        Assert.Equal(HttpStatusCode.OK, save.StatusCode);
        Assert.NotNull(state);
        Assert.Contains(state.Projects, project => project.Title == "Neighbourhood map");
    }

    private sealed record PublicState(IReadOnlyList<ProjectCard> Projects);
    private sealed record ProjectCard(string Title);
}
