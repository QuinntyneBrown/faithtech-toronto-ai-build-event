using System.Net;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ApplicationHostingTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-038;L2-041")]
    [InlineData("/")]
    [InlineData("/events/00000000-0000-0000-0000-000000000001/schedule")]
    public async Task Given_a_participant_link_when_opened_directly_then_the_client_shell_is_served(string path)
    {
        using var client = factory.Browser();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("<base href=\"/\">", await response.Content.ReadAsStringAsync());
    }

    [Theory, Trait("Requirement", "L2-038;L2-041")]
    [InlineData("/api/missing")]
    [InlineData("/api/admin/missing")]
    [InlineData("/missing.js")]
    [InlineData("/admin/missing.js")]
    public async Task Given_a_missing_api_or_asset_when_requested_then_no_spa_html_is_returned(string path)
    {
        using var client = factory.Browser();
        var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.NotEqual("text/html", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact, Trait("Requirement", "L2-038;L2-041")]
    public async Task Given_a_built_application_when_opening_an_admin_deep_link_then_its_shell_is_served_on_the_api_origin()
    {
        using var client = factory.Browser();
        var response = await client.GetAsync("/admin/sign-in");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/html", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("<base href=\"/admin/\">", await response.Content.ReadAsStringAsync());
    }
}
