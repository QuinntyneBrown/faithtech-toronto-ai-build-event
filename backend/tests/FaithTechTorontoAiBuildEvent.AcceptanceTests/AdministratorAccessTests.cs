using System.Net;
using System.Net.Http.Json;
using FaithTechTorontoAiBuildEvent.Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorAccessTests : IClassFixture<EventApiFactory>
{
    private readonly HttpClient client;
    private readonly EventApiFactory factory;

    public AdministratorAccessTests(EventApiFactory factory)
    {
        this.factory = factory;
        client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
    }

    [Fact, Trait("Requirement", "L2-041/AC2")]
    public async Task Given_plain_http_when_requesting_authentication_material_then_it_is_rejected()
    {
        using var insecure = factory.CreateClient(new() { BaseAddress = new Uri("http://localhost"), AllowAutoRedirect = false });
        var response = await insecure.GetAsync("/api/admin/antiforgery");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.False(response.Headers.Contains("Set-Cookie"));
    }

    [Fact]
    [Trait("Requirement", "L2-038/AC1")]
    public async Task Given_an_anonymous_requester_when_reading_admin_session_then_access_is_denied()
    {
        var response = await client.GetAsync("/api/admin/session");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("no-store", response.Headers.CacheControl?.ToString());
    }

    [Fact]
    [Trait("Requirement", "L2-041/AC3")]
    public async Task Given_no_antiforgery_token_when_signing_in_then_request_is_rejected()
    {
        var response = await client.PostAsJsonAsync("/api/admin/session", new { username = "operator", password = "invalid" });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    [Trait("Requirement", "L2-038/AC4")]
    public async Task Given_a_fresh_deployment_when_requesting_public_registration_then_no_route_exists()
    {
        var response = await client.PostAsJsonAsync("/api/admin/register", new { username = "operator", password = "invalid" });
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
