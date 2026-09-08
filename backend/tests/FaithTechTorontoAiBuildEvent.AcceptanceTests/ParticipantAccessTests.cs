using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class ParticipantAccessTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-003/AC1")]
    public async Task Given_a_published_event_and_an_unclaimed_active_entry_when_valid_email_and_code_are_submitted_then_one_binding_is_saved()
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublishedEvent(admin);
        var (registrationId, code) = await AddParticipant(admin, eventId, "Alex");

        using var participant = factory.Browser();
        using var response = await Authenticate(participant, eventId, "alex@example.com", code);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(registrationId, result.GetProperty("participantId").GetGuid());
        Assert.Equal(eventId, result.GetProperty("eventId").GetGuid());

        var cookie = Assert.Single(response.Headers.GetValues("Set-Cookie"), x => x.StartsWith("FaithTech.Participant="));
        Assert.Contains("secure", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains($"path=/api/events/{eventId:D}", cookie, StringComparison.OrdinalIgnoreCase);

        var roster = await admin.GetFromJsonAsync<JsonElement[]>($"/api/admin/events/{eventId}/roster");
        var entry = roster!.Single(x => x.GetProperty("id").GetGuid() == registrationId);
        Assert.True(entry.GetProperty("emailBound").GetBoolean());
    }

    [Fact, Trait("Requirement", "L2-003/AC2")]
    public async Task Given_an_already_bound_entry_when_the_matching_email_with_different_casing_and_whitespace_is_submitted_then_access_resumes_as_the_same_participant()
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublishedEvent(admin);
        var (registrationId, code) = await AddParticipant(admin, eventId, "Alex");

        using var first = factory.Browser();
        (await Authenticate(first, eventId, "alex@example.com", code)).EnsureSuccessStatusCode();

        using var second = factory.Browser();
        using var response = await Authenticate(second, eventId, "  ALEX@Example.com  ", code);
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(registrationId, result.GetProperty("participantId").GetGuid());

        var roster = await admin.GetFromJsonAsync<JsonElement[]>($"/api/admin/events/{eventId}/roster");
        Assert.Single(roster!, x => x.GetProperty("displayName").GetString() == "Alex");
    }

    [Fact, Trait("Requirement", "L2-003/AC3")]
    public async Task Given_two_concurrent_first_access_attempts_with_different_emails_for_one_code_when_processed_then_exactly_one_binding_succeeds()
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublishedEvent(admin);
        var (registrationId, code) = await AddParticipant(admin, eventId, "Alex");

        using var clientA = factory.Browser();
        using var clientB = factory.Browser();
        var responses = await Task.WhenAll(
            Authenticate(clientA, eventId, "first@example.com", code),
            Authenticate(clientB, eventId, "second@example.com", code));
        try
        {
            var succeeded = responses.Count(x => x.StatusCode == HttpStatusCode.OK);
            var denied = responses.Count(x => x.StatusCode == HttpStatusCode.Unauthorized);
            Assert.Equal(1, succeeded);
            Assert.Equal(1, denied);

            var roster = await admin.GetFromJsonAsync<JsonElement[]>($"/api/admin/events/{eventId}/roster");
            var entry = roster!.Single(x => x.GetProperty("id").GetGuid() == registrationId);
            Assert.True(entry.GetProperty("emailBound").GetBoolean());
        }
        finally { foreach (var response in responses) response.Dispose(); }
    }

    [Theory, Trait("Requirement", "L2-003/AC4")]
    [InlineData("malformed-email")]
    [InlineData("wrong-event-code")]
    [InlineData("mismatched-email")]
    [InlineData("duplicate-email")]
    public async Task Given_an_invalid_submission_when_processed_then_access_is_denied_without_disclosing_another_participants_data(string scenario)
    {
        using var admin = await factory.AdministratorBrowser();
        var eventId = await CreatePublishedEvent(admin);
        var (_, code) = await AddParticipant(admin, eventId, "Alex");

        if (scenario == "mismatched-email")
        {
            using var bind = factory.Browser();
            (await Authenticate(bind, eventId, "alex@example.com", code)).EnsureSuccessStatusCode();
        }
        string email = scenario switch
        {
            "malformed-email" => "not-an-email",
            "duplicate-email" => "duplicate@example.com",
            "mismatched-email" => "someone-else@example.com",
            _ => "alex@example.com",
        };
        var (submittedEventId, submittedCode) = scenario == "wrong-event-code"
            ? (await CreatePublishedEvent(admin), code)
            : (eventId, code);
        if (scenario == "duplicate-email")
        {
            var (_, otherCode) = await AddParticipant(admin, eventId, "Riley");
            using var bindOther = factory.Browser();
            (await Authenticate(bindOther, eventId, "duplicate@example.com", otherCode)).EnsureSuccessStatusCode();
        }

        using var participant = factory.Browser();
        using var response = await Authenticate(participant, submittedEventId, email, submittedCode);
        var body = await response.Content.ReadAsStringAsync();
        if (scenario == "malformed-email")
        {
            Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
            var problem = await response.Content.ReadFromJsonAsync<JsonElement>();
            Assert.True(problem.GetProperty("errors").TryGetProperty("email", out _));
        }
        else
        {
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
            Assert.DoesNotContain("Alex", body);
            Assert.DoesNotContain("Riley", body);
            Assert.False(response.Headers.Contains("Set-Cookie"));
        }
    }

    [Fact, Trait("Requirement", "L2-046/AC4")]
    public async Task Given_a_participant_session_for_one_event_when_it_is_used_against_a_different_events_route_then_it_is_rejected()
    {
        using var admin = await factory.AdministratorBrowser();
        var eventA = await CreatePublishedEvent(admin);
        var eventB = await CreatePublishedEvent(admin);
        var (_, codeA) = await AddParticipant(admin, eventA, "Alex");

        using var participant = factory.Browser();
        (await Authenticate(participant, eventA, "alex@example.com", codeA)).EnsureSuccessStatusCode();

        using var crossEventRead = await participant.GetAsync($"/api/events/{eventB}/session");
        Assert.Equal(HttpStatusCode.Unauthorized, crossEventRead.StatusCode);

        using var sameEventRead = await participant.GetAsync($"/api/events/{eventA}/session");
        sameEventRead.EnsureSuccessStatusCode();
    }

    private async Task<Guid> CreatePublishedEvent(HttpClient admin)
    {
        admin.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await admin.PostAsJsonAsync("/api/admin/events", new { title = "Participant access" }); response.EnsureSuccessStatusCode();
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

    private static async Task<HttpResponseMessage> Authenticate(HttpClient client, Guid eventId, string email, string entryCode)
    {
        var token = await client.GetFromJsonAsync<JsonElement>($"/api/events/{eventId}/antiforgery");
        client.DefaultRequestHeaders.Remove("X-CSRF-TOKEN");
        client.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        return await client.PostAsJsonAsync($"/api/events/{eventId}/session", new { email, entryCode });
    }
}
