using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ParticipantSessionTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-004/AC1")]
    public async Task Given_an_authenticated_participant_when_the_session_is_read_after_refresh_then_the_same_identity_is_restored()
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublishedEvent(admin);
        var (client, registrationId, _) = await factory.ParticipantBrowser(admin, eventId, "Alex", "alex@example.com");
        using (client)
        {
            var first = await client.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/session");
            var second = await client.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/session");
            Assert.Equal(registrationId, first.GetProperty("participantId").GetGuid());
            Assert.Equal(registrationId, second.GetProperty("participantId").GetGuid());
            Assert.Equal(eventId, second.GetProperty("eventId").GetGuid());
        }
    }

    [Fact, Trait("Requirement", "L2-004/AC3")]
    public async Task Given_two_independent_sessions_for_one_participant_when_one_is_signed_out_then_the_other_remains_valid()
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublishedEvent(admin);
        var (first, registrationId, code) = await factory.ParticipantBrowser(admin, eventId, "Alex", "alex@example.com");

        using var second = factory.Browser();
        var token = await second.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/antiforgery");
        second.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        (await second.PostAsJsonAsync($"/api/events/{eventId}/session", new { email = "alex@example.com", entryCode = code })).EnsureSuccessStatusCode();

        using (first)
        {
            (await first.DeleteAsync($"/api/events/{eventId}/session")).EnsureSuccessStatusCode();

            using var afterSignOut = await first.GetAsync($"/api/events/{eventId}/session");
            Assert.Equal(HttpStatusCode.Unauthorized, afterSignOut.StatusCode);
        }

        var stillValid = await second.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/session");
        Assert.Equal(registrationId, stillValid.GetProperty("participantId").GetGuid());
    }

    [Theory, Trait("Requirement", "L2-004/AC4")]
    [InlineData(-30, HttpStatusCode.OK)]
    [InlineData(30, HttpStatusCode.Unauthorized)]
    public async Task Given_a_session_authenticated_24_hours_ago_when_read_at_the_exact_boundary_then_validity_matches(int secondsPastExpiry, HttpStatusCode expected)
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublishedEvent(admin);
        var (client, _, _) = await factory.ParticipantBrowser(admin, eventId, "Alex", "alex@example.com");
        using (client)
        {
            using var scope = factory.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
            var session = await db.ParticipantSessions.SingleAsync(x => x.EventId == eventId);
            session.AuthenticatedAtUtc = DateTimeOffset.UtcNow.AddHours(-24).AddSeconds(-secondsPastExpiry);
            await db.SaveChangesAsync();

            using var response = await client.GetAsync($"/api/events/{eventId}/session");
            Assert.Equal(expected, response.StatusCode);
        }
    }

    private async Task<Guid> CreatePublishedEvent(HttpClient admin)
    {
        admin.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await admin.PostAsJsonAsync("/api/admin/events", new { title = "Participant session" }); response.EnsureSuccessStatusCode();
        admin.DefaultRequestHeaders.Remove("Idempotency-Key");
        var item = await response.Content.ReadFromJsonAsync<JsonElement>();
        var eventId = item.GetProperty("id").GetGuid();
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        (await db.Events.SingleAsync(x => x.Id == eventId)).Published = true;
        await db.SaveChangesAsync();
        return eventId;
    }
}
