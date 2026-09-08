// Acceptance Test
// Traces to: L2-062
// Description: Reset an administrator through the CLI and verify API credentials and sessions.
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorResetCommandTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-062/AC1")]
    public async Task Given_a_signed_in_administrator_when_reset_then_old_credentials_and_sessions_fail()
    {
        var oldPassword = "previous2026!password";
        var username = await factory.ProvisionAdministrator(oldPassword);
        using var client = factory.Browser();
        Assert.Equal(HttpStatusCode.NoContent, await SignIn(client, username, oldPassword));
        using var unaffected = await factory.AdministratorBrowser();
        Assert.Equal(0, await ProvisioningProcess.Run(factory,
            ["reset-password", username, "--password-stdin"], "faithtech2026!"));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/session")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await unaffected.GetAsync("/api/admin/session")).StatusCode);
        using var fresh = factory.Browser();
        Assert.Equal(HttpStatusCode.Unauthorized, await SignIn(fresh, username, oldPassword));
        Assert.Equal(HttpStatusCode.NoContent, await SignIn(fresh, username, "faithtech2026!"));
    }

    [Theory, Trait("Requirement", "L2-062/AC3;L2-062/AC5")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("weak")]
    public async Task Given_invalid_password_input_when_reset_then_the_existing_password_and_session_remain(string? password)
    {
        var username = await factory.ProvisionAdministrator("previous2026!password");
        using var client = factory.Browser();
        Assert.Equal(HttpStatusCode.NoContent, await SignIn(client, username, "previous2026!password"));
        Assert.NotEqual(0, await ProvisioningProcess.Run(factory, ["reset-password", username, "--password-stdin"], password));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/admin/session")).StatusCode);
        using var fresh = factory.Browser();
        Assert.Equal(HttpStatusCode.NoContent, await SignIn(fresh, username, "previous2026!password"));
    }

    private static async Task<HttpStatusCode> SignIn(HttpClient client, string username, string password)
    {
        var token = await client.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return (await client.PostAsJsonAsync("/api/admin/session", new { username, password })).StatusCode;
    }
}
