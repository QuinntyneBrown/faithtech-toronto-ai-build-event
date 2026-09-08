using System.Net;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ApplicationHostingTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
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
