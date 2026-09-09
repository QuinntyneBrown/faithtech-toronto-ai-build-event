// Acceptance Test
// Traces to: L2-012, L2-040
// Description: Project text is trimmed and persisted with canonical LF line endings.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ProjectTextNormalizationTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public ProjectTextNormalizationTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Saved_project_text_is_trimmed_and_uses_lf_line_endings()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" });

        var saved = await client.PostAsJsonAsync("/api/admin/projects", new
        {
            operationId = Guid.NewGuid(), expectedVersion = "1",
            input = new { title = "  Canonical project  ", description = "  First\r\nSecond\rThird  ", repositoryUrl = "  https://example.com/repository  ", demoUrl = (string?)null }
        });
        var state = await client.GetFromJsonAsync<PublicState>("/api/event/state");
        var project = Assert.Single(state!.Projects, project => project.Title == "Canonical project");

        Assert.Equal(HttpStatusCode.OK, saved.StatusCode);
        Assert.Equal("First\nSecond\nThird", project.Description);
        Assert.Equal("https://example.com/repository", project.RepositoryUrl);
    }

    private sealed record PublicState(IReadOnlyList<ProjectCard> Projects);
    private sealed record ProjectCard(string Title, string Description, string? RepositoryUrl);
}
