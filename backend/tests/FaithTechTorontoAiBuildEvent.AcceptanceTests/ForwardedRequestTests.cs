using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ForwardedRequestTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Theory, Trait("Requirement", "L2-041;L2-045")]
    [InlineData("10.20.30.40", 200)]
    [InlineData("10.20.30.41", 400)]
    public async Task Given_forwarded_https_when_the_peer_is_checked_then_only_trusted_ingress_is_accepted(string peer, int status)
    {
        using var host = factory.WithWebHostBuilder(builder =>
            builder.UseSetting("Hosting:KnownProxies:0", "10.20.30.40"));
        var response = await host.Server.SendAsync(context =>
        {
            context.Connection.RemoteIpAddress = IPAddress.Parse(peer);
            context.Request.Scheme = "http";
            context.Request.Path = "/api/admin/antiforgery";
            context.Request.Headers["X-Forwarded-Proto"] = "https";
            context.Request.Headers["X-Forwarded-For"] = "203.0.113.9";
        });
        Assert.Equal(status, response.Response.StatusCode);
        Assert.Equal(status == 200 ? "203.0.113.9" : peer, response.Connection.RemoteIpAddress?.ToString());
    }
}
