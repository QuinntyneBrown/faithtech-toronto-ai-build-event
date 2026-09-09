// Acceptance Test
// Traces to: L2-063
// Description: Replacing even the same passcode revokes every existing administrator browser session.

using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorPasscodeRotationTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AdministratorPasscodeRotationTests(CountdownApiFactory factory) => this.factory = factory;

    [Fact]
    public async Task Same_value_passcode_rotation_revokes_an_existing_administrator_session()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });
        var signedIn = await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });
        Assert.Equal(HttpStatusCode.OK, signedIn.StatusCode);

        await factory.ProvisionAdministratorPasscodeAsync("0042");
        var session = await client.GetAsync("/api/admin/session");

        Assert.Equal(HttpStatusCode.Unauthorized, session.StatusCode);
    }
}
