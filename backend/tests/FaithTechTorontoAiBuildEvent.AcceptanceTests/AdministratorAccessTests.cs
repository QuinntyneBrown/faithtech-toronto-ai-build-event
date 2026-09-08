using System.Net;
using System.Net.Http.Json;
using FaithTechTorontoAiBuildEvent.Api;
using Microsoft.AspNetCore.Mvc.Testing;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorAccessTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public AdministratorAccessTests(WebApplicationFactory<Program> factory)
    {
        client = factory.CreateClient(new() { BaseAddress = new Uri("https://localhost"), AllowAutoRedirect = false });
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
