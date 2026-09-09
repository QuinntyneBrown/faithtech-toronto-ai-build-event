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
        await factory.ClearAdministratorLoginAttemptsAsync();
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var cookie = response.Headers.GetValues("Set-Cookie").Single();
        Assert.Contains("faithtech-admin=", cookie, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Invalid_or_non_four_digit_passcodes_do_not_create_session()
    {
        await factory.ClearAdministratorLoginAttemptsAsync();
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var response = await client.PostAsJsonAsync("/api/admin/session", new { passcode = "42" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Sixth_failed_check_is_throttled_before_passcode_verification()
    {
        await factory.ClearAdministratorLoginAttemptsAsync();
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        for (var attempt = 0; attempt < 5; attempt++)
        {
            var failed = await client.PostAsJsonAsync("/api/admin/session", new { passcode = "9999" });
            Assert.Equal(HttpStatusCode.Unauthorized, failed.StatusCode);
        }

        var throttled = await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });

        Assert.Equal((HttpStatusCode)429, throttled.StatusCode);
        Assert.True(int.Parse(throttled.Headers.GetValues("Retry-After").Single()) > 0);
    }

    [Fact]
    public async Task Twenty_failed_checks_across_sources_throttle_the_deployment()
    {
        await factory.ClearAdministratorLoginAttemptsAsync();
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        await factory.RecordAdministratorLoginFailuresAsync(20);
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        var throttled = await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });

        Assert.Equal((HttpStatusCode)429, throttled.StatusCode);
        Assert.True(int.Parse(throttled.Headers.GetValues("Retry-After").Single()) > 0);
    }

    [Fact]
    public async Task Successful_login_does_not_erase_prior_failed_checks()
    {
        await factory.ClearAdministratorLoginAttemptsAsync();
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost") });

        for (var attempt = 0; attempt < 4; attempt++) Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/admin/session", new { passcode = "9999" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" })).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsJsonAsync("/api/admin/session", new { passcode = "9999" })).StatusCode);

        var throttled = await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" });

        Assert.Equal((HttpStatusCode)429, throttled.StatusCode);
    }

    [Fact]
    public async Task Sign_out_revokes_the_administrator_session()
    {
        await factory.ClearAdministratorLoginAttemptsAsync();
        await factory.ProvisionAdministratorPasscodeAsync("0042");
        using var client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { BaseAddress = new Uri("https://localhost"), HandleCookies = true });
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/admin/session", new { passcode = "0042" })).StatusCode);

        Assert.Equal(HttpStatusCode.NoContent, (await client.DeleteAsync("/api/admin/session")).StatusCode);

        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/session")).StatusCode);
    }
}
