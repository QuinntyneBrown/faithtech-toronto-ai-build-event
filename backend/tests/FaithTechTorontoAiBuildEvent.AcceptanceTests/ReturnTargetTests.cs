using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ReturnTargetTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-046/AC3")]
    public async Task Given_a_requested_same_event_route_when_authenticating_then_it_is_returned_as_the_authorized_route()
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublished(admin);
        var (_, code) = await AddParticipant(admin, eventId, "Alex");

        using var participant = factory.Browser();
        var token = await participant.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/antiforgery");
        participant.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        using var response = await participant.PostAsJsonAsync($"/api/events/{eventId}/session",
            new { email = "alex@example.com", entryCode = code, returnTo = $"/events/{eventId}/schedule" });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal($"/events/{eventId}/schedule", result.GetProperty("authorizedInitialRoute").GetString());
    }

    [Theory, Trait("Requirement", "L2-046/AC4")]
    [InlineData("https://evil.example.com/steal")]
    [InlineData("//evil.example.com/steal")]
    public async Task Given_an_external_or_malformed_return_target_when_authenticating_then_it_is_ignored(string returnTo)
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublished(admin);
        var (_, code) = await AddParticipant(admin, eventId, "Alex");

        using var participant = factory.Browser();
        var token = await participant.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/antiforgery");
        participant.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        using var response = await participant.PostAsJsonAsync($"/api/events/{eventId}/session",
            new { email = "alex@example.com", entryCode = code, returnTo });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal($"/events/{eventId}", result.GetProperty("authorizedInitialRoute").GetString());
    }

    [Fact, Trait("Requirement", "L2-046/AC4")]
    public async Task Given_a_return_target_for_a_different_event_when_authenticating_then_it_is_ignored()
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublished(admin);
        var otherEventId = await CreatePublished(admin);
        var (_, code) = await AddParticipant(admin, eventId, "Alex");

        using var participant = factory.Browser();
        var token = await participant.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/antiforgery");
        participant.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        using var response = await participant.PostAsJsonAsync($"/api/events/{eventId}/session",
            new { email = "alex@example.com", entryCode = code, returnTo = $"/events/{otherEventId}/schedule" });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal($"/events/{eventId}", result.GetProperty("authorizedInitialRoute").GetString());
    }

    [Fact, Trait("Requirement", "L2-046/AC3")]
    public async Task Given_a_completed_event_when_authenticating_then_the_authorized_route_is_the_showcase()
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublished(admin);
        var (_, code) = await AddParticipant(admin, eventId, "Alex");
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
            var entity = await db.Events.SingleAsync(x => x.Id == eventId);
            entity.EndsAtUtc = DateTimeOffset.UtcNow.AddMinutes(-1);
            await db.SaveChangesAsync();
        }

        using var participant = factory.Browser();
        var token = await participant.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/antiforgery");
        participant.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        using var response = await participant.PostAsJsonAsync($"/api/events/{eventId}/session", new { email = "alex@example.com", entryCode = code });
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal($"/events/{eventId}/showcase", result.GetProperty("authorizedInitialRoute").GetString());
    }

    private async Task<Guid> CreatePublished(HttpClient admin)
    {
        admin.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await admin.PostAsJsonAsync("/api/admin/events", new { title = "Return target" }); response.EnsureSuccessStatusCode();
        admin.DefaultRequestHeaders.Remove("Idempotency-Key");
        var item = await response.Content.ReadFromJsonAsync<JsonElement>();
        var eventId = item.GetProperty("id").GetGuid();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        (await db.Events.SingleAsync(x => x.Id == eventId)).Published = true;
        await db.SaveChangesAsync();
        return eventId;
    }

    private static async Task<(Guid RegistrationId, string Code)> AddParticipant(HttpClient admin, Guid eventId, string name)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{eventId}/roster") { Content = JsonContent.Create(new { displayName = name }) };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await admin.SendAsync(request); response.EnsureSuccessStatusCode();
        var issued = await response.Content.ReadFromJsonAsync<JsonElement>();
        return (issued.GetProperty("entry").GetProperty("id").GetGuid(), issued.GetProperty("code").GetString()!);
    }
}
