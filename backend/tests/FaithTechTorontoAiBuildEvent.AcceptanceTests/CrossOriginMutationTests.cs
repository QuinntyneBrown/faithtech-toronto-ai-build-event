// Acceptance Test
// Traces to: L2-041
// Description: Cookie-bearing mutations reject an untrusted browser origin before dispatching a command.

using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class CrossOriginMutationTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public CrossOriginMutationTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Cross_origin_mutation_is_forbidden_before_command_processing()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/event/advance")
        {
            Content = JsonContent.Create(new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" })
        };
        request.Headers.Add("Origin", "https://untrusted.example");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Same_origin_mutation_reaches_its_authorization_boundary()
    {
        using var client = factory.CreateClient();
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/event/advance")
        {
            Content = JsonContent.Create(new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" })
        };
        request.Headers.Add("Origin", client.BaseAddress!.GetLeftPart(UriPartial.Authority));

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Different_scheme_on_same_host_is_not_a_trusted_origin()
    {
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/admin/event/advance")
        {
            Content = JsonContent.Create(new { operationId = Guid.NewGuid(), expectedVersion = "0", fromScreen = "countdown", toScreen = "projects" })
        };
        request.Headers.Add("Origin", "http://localhost");

        var response = await client.SendAsync(request);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }
}
