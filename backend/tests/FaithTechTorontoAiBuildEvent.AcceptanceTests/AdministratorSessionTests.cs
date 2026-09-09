// Acceptance Test
// Traces to: L2-038, L2-041
// Description: A provisioned shared four-digit passcode grants a private administrator session without a username.

using System.Net;
using System.Net.Http.Json;
using Xunit;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorSessionTests : IClassFixture<CountdownApiFactory>
{
    private readonly CountdownApiFactory factory;

    public AdministratorSessionTests(CountdownApiFactory factory)
    {
        this.factory = factory;
    }

    [Fact]
    public async Task Exact_four_digit_provisioned_passcode_creates_administrator_session()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("faithtech-admin=", response.Headers.GetValues("Set-Cookie").Single(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invalid_or_non_four_digit_passcodes_do_not_create_session()
    {
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsJsonAsync("/api/admin/session", new { passcode = "42" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
