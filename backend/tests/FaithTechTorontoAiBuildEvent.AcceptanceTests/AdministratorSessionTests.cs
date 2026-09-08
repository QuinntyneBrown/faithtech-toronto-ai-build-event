using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class AdministratorSessionTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    private async Task<HttpClient> SignIn(string username, string password)
    {
        var client = factory.Browser();
        await Antiforgery(client);
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsJsonAsync("/api/admin/session", new { username, password })).StatusCode);
        await Antiforgery(client);
        return client;
    }

    private static async Task Antiforgery(HttpClient client)
    {
        var token = await client.GetFromJsonAsync<JsonElement>("/api/admin/antiforgery");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
    }

    [Fact, Trait("Requirement", "L2-038/AC3")]
    public async Task Given_two_sessions_when_one_signs_out_then_only_that_session_is_revoked()
    {
        var password = $"Valid9!{Guid.NewGuid():N}";
        var username = await factory.ProvisionAdministrator(password);
        using var first = await SignIn(username, password);
        using var second = await SignIn(username, password);
        Assert.Equal(HttpStatusCode.NoContent, (await first.DeleteAsync("/api/admin/session")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await first.GetAsync("/api/admin/session")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await second.GetAsync("/api/admin/session")).StatusCode);
    }

    [Theory, InlineData(31, 1), InlineData(1, 9), Trait("Requirement", "L2-038/AC5")]
    public async Task Given_an_expired_session_when_reading_or_interacting_then_it_cannot_be_revived(int idleMinutes, int absoluteHours)
    {
        var password = $"Valid9!{Guid.NewGuid():N}";
        var username = await factory.ProvisionAdministrator(password);
        using var client = await SignIn(username, password);
        await AgeSession(client, idleMinutes, absoluteHours);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/admin/session")).StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.PostAsync("/api/admin/session/interaction", null)).StatusCode);
    }

    [Fact, Trait("Requirement", "L2-038/AC5")]
    public async Task Given_background_reads_when_only_a_deliberate_interaction_occurs_then_only_interaction_renews_idle_expiry()
    {
        var password = $"Valid9!{Guid.NewGuid():N}";
        var username = await factory.ProvisionAdministrator(password);
        using var client = await SignIn(username, password);
        await AgeSession(client, 20, 1);
        var before = await client.GetFromJsonAsync<JsonElement>("/api/admin/session");
        var repeated = await client.GetFromJsonAsync<JsonElement>("/api/admin/session");
        Assert.Equal(before.GetProperty("idleExpiresAtUtc").GetString(), repeated.GetProperty("idleExpiresAtUtc").GetString());
        Assert.Equal(HttpStatusCode.NoContent, (await client.PostAsync("/api/admin/session/interaction", null)).StatusCode);
        var after = await client.GetFromJsonAsync<JsonElement>("/api/admin/session");
        Assert.True(after.GetProperty("idleExpiresAtUtc").GetDateTimeOffset() > before.GetProperty("idleExpiresAtUtc").GetDateTimeOffset());
        Assert.Equal(before.GetProperty("absoluteExpiresAtUtc").GetString(), after.GetProperty("absoluteExpiresAtUtc").GetString());
    }

    private async Task AgeSession(HttpClient client, int idleMinutes, int absoluteHours)
    {
        var current = await client.GetFromJsonAsync<JsonElement>("/api/admin/session");
        var actor = current.GetProperty("actorId").GetGuid();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        await db.AdministratorSessions.Where(x => x.AdministratorId == actor).ExecuteUpdateAsync(set => set
            .SetProperty(x => x.AuthenticatedAtUtc, DateTimeOffset.UtcNow.AddHours(-absoluteHours))
            .SetProperty(x => x.LastInteractionAtUtc, DateTimeOffset.UtcNow.AddMinutes(-idleMinutes)));
    }
}
