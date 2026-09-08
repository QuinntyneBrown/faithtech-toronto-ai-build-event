using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ReadinessTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-045")]
    public async Task Given_no_session_when_readiness_is_requested_then_access_is_denied()
    {
        using var client = factory.Browser();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/readiness")).StatusCode);
    }

    [Fact, Trait("Requirement", "L2-045")]
    public async Task Given_an_administrator_and_current_database_when_readiness_is_requested_then_the_release_is_ready()
    {
        using var client = await factory.AdministratorBrowser();
        var response = await client.GetAsync("/api/admin/readiness");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(body.GetProperty("ready").GetBoolean());
        Assert.NotEmpty(body.GetProperty("revision").GetString()!);
    }
}
