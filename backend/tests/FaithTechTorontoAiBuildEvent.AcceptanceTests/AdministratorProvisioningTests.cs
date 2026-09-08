using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorProvisioningTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-038/AC2;L2-038/AC3;L2-049/AC3")]
    public async Task Given_operator_provisioning_when_the_account_is_disabled_then_existing_sessions_and_new_signins_fail()
    {
        var username = $"operator-{Guid.NewGuid():N}";
        var password = $"Valid9!{Guid.NewGuid():N}";
        Assert.Equal(0, await ProvisioningProcess.Run(factory, ["create-admin", username], password));
        Assert.Equal(1, await ProvisioningProcess.Run(factory, ["create-admin", username], password));
        Assert.Equal(1, await ProvisioningProcess.Run(factory, ["create-admin", $"invalid-{Guid.NewGuid():N}"], "weak"));
        using var client = factory.Browser();
        var token = await client.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/admin/session", new { username, password })).StatusCode);
        Assert.Equal(0, await ProvisioningProcess.Run(factory, ["disable-admin", username]));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/session")).StatusCode);
        using var another = factory.Browser();
        token = await another.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
        another.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        Assert.Equal(HttpStatusCode.Unauthorized, (await another.PostAsJsonAsync("/api/admin/session", new { username, password })).StatusCode);
    }
}
