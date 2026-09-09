// Acceptance Test
// Traces to: L2-038, L2-063
// Description: Privileged access checks session expiry and credential revision on every request.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorAuthorizationTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AdministratorAuthorizationTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Credential_rotation_invalidates_established_administrator_session()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });

        var active = await client.GetAsync("/api/admin/session");
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        var invalidated = await client.GetAsync("/api/admin/session");

        Assert.Equal(HttpStatusCode.OK, active.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, invalidated.StatusCode);
    }
}
