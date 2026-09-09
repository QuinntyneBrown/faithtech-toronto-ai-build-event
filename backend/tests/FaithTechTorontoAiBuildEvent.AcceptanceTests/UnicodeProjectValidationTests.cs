// Acceptance Test
// Traces to: L2-012, L2-040
// Description: Project text limits count Unicode scalar values and reject an over-limit mutation without changing state.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class UnicodeProjectValidationTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public UnicodeProjectValidationTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Project_accepts_200_unicode_scalars_and_rejects_201()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        await client.PostAsJsonAsync("/api/admin/event/advance", new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" });
        var acceptedTitle = string.Concat(Enumerable.Repeat("😀", 200));

        var accepted = await client.PostAsJsonAsync("/api/admin/projects", new
        {
            operationId = Guid.NewGuid(), expectedVersion = "1", input = new { title = acceptedTitle, description = "Valid", repositoryUrl = (string?)null, demoUrl = (string?)null }
        });
        var rejected = await client.PostAsJsonAsync("/api/admin/projects", new
        {
            operationId = Guid.NewGuid(), expectedVersion = "2", input = new { title = acceptedTitle + "😀", description = "Invalid", repositoryUrl = (string?)null, demoUrl = (string?)null }
        });
        var state = await client.GetFromJsonAsync<PublicState>("/api/event/state");

        Assert.Equal(HttpStatusCode.OK, accepted.StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, rejected.StatusCode);
        Assert.Contains(state!.Projects, project => project.Title == acceptedTitle);
        Assert.DoesNotContain(state.Projects, project => project.Description == "Invalid");
    }

    private sealed record PublicState(IReadOnlyList<ProjectCard> Projects);
    private sealed record ProjectCard(string Title, string Description);
}
