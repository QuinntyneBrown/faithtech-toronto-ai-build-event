using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorSignInTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-038/AC2;L2-041/AC2")]
    public async Task Given_a_provisioned_account_when_signing_in_then_a_secure_session_survives_refresh()
    {
        var password = $"Valid9!{Guid.NewGuid():N}";
        var username = await factory.ProvisionAdministrator(password);
        using var client = factory.Browser();
        var token = await client.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        var response = await client.PostAsJsonAsync("/api/admin/session", new { username, password });
        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), x => x.StartsWith("FaithTech.Admin="));
        Assert.Contains("secure", cookie);
        Assert.Contains("httponly", cookie);
        Assert.Contains("path=/api/admin", cookie);
        var session = await client.GetFromJsonAsync<JsonElement>("/api/admin/session");
        Assert.True(session.GetProperty("actorId").TryGetGuid(out _));
        Assert.False(session.TryGetProperty("password", out _));
        Assert.False(session.TryGetProperty("sessionId", out _));
        Assert.True(session.GetProperty("serverNow").GetDateTimeOffset() < session.GetProperty("idleExpiresAtUtc").GetDateTimeOffset());
    }

    [Fact, Trait("Requirement", "L2-038/AC4;L2-042/AC4")]
    public async Task Given_an_unknown_account_when_signing_in_then_no_session_is_issued()
    {
        using var client = factory.Browser();
        var token = await client.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        var response = await client.PostAsJsonAsync("/api/admin/session", new { username = "unknown", password = "Invalid9!" });
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }
}
