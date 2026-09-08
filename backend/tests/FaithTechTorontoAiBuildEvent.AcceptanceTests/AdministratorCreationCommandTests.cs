// Acceptance Test
// Traces to: L2-061
// Description: Create administrators through the CLI and authenticate through the API.
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorCreationCommandTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-061/AC1;L2-061/AC2")]
    [InlineData(null)]
    [InlineData("custom2026!password")]
    public async Task Given_a_unique_username_when_added_then_the_selected_password_authenticates(string? password)
    {
        var username = $"added-{Guid.NewGuid():N}";
        string[] arguments = password is null ? ["add-user", username] : ["add-user", username, "--password-stdin"];
        Assert.Equal(0, await ProvisioningProcess.Run(factory, arguments, password));
        Assert.Equal(HttpStatusCode.NoContent, await SignIn(username, password ?? "faithtech2026!"));
        if (password is not null) Assert.Equal(HttpStatusCode.Unauthorized, await SignIn(username, "faithtech2026!"));
        Assert.NotEqual(0, await ProvisioningProcess.Run(factory, ["add-user", username.ToUpperInvariant()]));
    }

    [Theory, Trait("Requirement", "L2-061/AC3")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("weak")]
    public async Task Given_invalid_explicit_input_when_added_then_no_default_account_is_created(string? password)
    {
        var username = $"rejected-{Guid.NewGuid():N}";
        Assert.NotEqual(0, await ProvisioningProcess.Run(factory, ["add-user", username, "--password-stdin"], password));
        Assert.Equal(HttpStatusCode.Unauthorized, await SignIn(username, "faithtech2026!"));
    }

    private async Task<HttpStatusCode> SignIn(string username, string password)
    {
        using var client = factory.Browser();
        var token = await client.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return (await client.PostAsJsonAsync("/api/admin/session", new { username, password })).StatusCode;
    }
}
