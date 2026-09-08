using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Access;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorProvisioningTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-038/AC2;L2-038/AC3")]
    public async Task Given_operator_provisioning_when_the_account_is_disabled_then_existing_sessions_and_new_signins_fail()
    {
        using var scope = factory.Services.CreateScope();
        var sender = scope.ServiceProvider.GetRequiredService<ISender>();
        var username = $"operator-{Guid.NewGuid():N}";
        var password = $"Valid9!{Guid.NewGuid():N}";
        await sender.Send(new ProvisionAdministratorCommand(username, password));
        using var client = factory.Browser();
        var token = await client.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/admin/session", new { username, password })).StatusCode);
        await sender.Send(new DisableAdministratorCommand(username));
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/session")).StatusCode);
        using var another = factory.Browser();
        token = await another.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
        another.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        Assert.Equal(HttpStatusCode.Unauthorized, (await another.PostAsJsonAsync("/api/admin/session", new { username, password })).StatusCode);
    }
}
