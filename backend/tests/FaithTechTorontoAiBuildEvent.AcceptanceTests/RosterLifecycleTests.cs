using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Roster;
using FaithTechTorontoAiBuildEvent.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FaithTechTorontoAiBuildEvent.AcceptanceTests;

public sealed class RosterLifecycleTests(EventApiFactory factory) : IClassFixture<EventApiFactory>
{
    [Fact, Trait("Requirement", "L2-002/AC2")]
    public async Task Given_an_existing_participant_when_their_display_name_changes_then_their_identity_submissions_and_credential_stay_attached()
    {
        using var client = await factory.AdministratorBrowser();
        var item = await Create(client);
        var issued = await Add(client, item.Id, "Alex");

        var renamed = await Rename(client, item.Id, issued.Entry.Id, "Alexandra", issued.Entry.Version);
        renamed.EnsureSuccessStatusCode();
        var result = (await renamed.Content.ReadFromJsonAsync<RosterEntry>())!;
        Assert.Equal(issued.Entry.Id, result.Id);
        Assert.Equal("Alexandra", result.DisplayName);
        Assert.Equal(issued.Entry.Active, result.Active);
        Assert.Equal(issued.Entry.FirstAccessAtUtc, result.FirstAccessAtUtc);

        var roster = (await client.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!;
        Assert.Single(roster, x => x.Id == issued.Entry.Id && x.DisplayName == "Alexandra");
    }

    [Fact, Trait("Requirement", "L2-002/AC2")]
    public async Task Given_a_stale_version_when_renaming_then_the_current_entry_is_returned_without_changing_the_stored_name()
    {
        using var client = await factory.AdministratorBrowser();
        var item = await Create(client);
        var issued = await Add(client, item.Id, "Alex");

        using var stale = await Rename(client, item.Id, issued.Entry.Id, "Wrong", "AAAAAAAAAAA=");
        Assert.Equal(HttpStatusCode.Conflict, stale.StatusCode);

        var roster = (await client.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!;
        Assert.Single(roster, x => x.Id == issued.Entry.Id && x.DisplayName == "Alex");
    }

    [Fact, Trait("Requirement", "L2-002/AC2")]
    public async Task Given_a_missing_If_Match_header_when_renaming_then_a_version_is_required()
    {
        using var client = await factory.AdministratorBrowser();
        var item = await Create(client);
        var issued = await Add(client, item.Id, "Alex");

        using var response = await Rename(client, item.Id, issued.Entry.Id, "Alexandra", null);
        Assert.Equal((HttpStatusCode)428, response.StatusCode);
    }

    [Fact, Trait("Requirement", "L2-002/AC2")]
    public async Task Given_a_nonexistent_registration_when_renaming_then_not_found_is_returned()
    {
        using var client = await factory.AdministratorBrowser();
        var item = await Create(client);

        using var response = await Rename(client, item.Id, Guid.NewGuid(), "Alexandra", "AAAAAAAAAAA=");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact, Trait("Requirement", "L2-002/AC3")]
    public async Task Given_a_deactivated_entry_when_its_credential_or_existing_session_is_used_then_event_access_is_denied()
    {
        using var admin = await factory.AdministratorBrowser();
        var item = await CreatePublished(admin);
        var (client, registrationId, code) = await factory.ParticipantBrowser(admin, item.Id, "Alex", "alex@example.com");
        using (client)
        {
            (await client.GetFromJsonAsync<JsonElement>($"/api/events/{item.Id}/session")).GetProperty("participantId").GetGuid();

            var entries = (await admin.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!;
            var version = entries.Single(x => x.Id == registrationId).Version;
            var deactivated = await Deactivate(admin, item.Id, registrationId, version);
            deactivated.EnsureSuccessStatusCode();
            var result = (await deactivated.Content.ReadFromJsonAsync<RosterEntry>())!;
            Assert.False(result.Active);

            using var afterDeactivation = await client.GetAsync($"/api/events/{item.Id}/session");
            Assert.Equal(HttpStatusCode.Unauthorized, afterDeactivation.StatusCode);
        }

        using var freshAttempt = factory.Browser();
        var token = await freshAttempt.GetFromJsonAsync<JsonElement>($"/api/events/{item.Id}/antiforgery");
        freshAttempt.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        using var reauthenticate = await freshAttempt.PostAsJsonAsync($"/api/events/{item.Id}/session", new { email = "alex@example.com", entryCode = code });
        Assert.Equal(HttpStatusCode.Unauthorized, reauthenticate.StatusCode);
    }

    [Fact, Trait("Requirement", "L2-002/AC4")]
    public async Task Given_a_replaced_entry_code_when_the_old_code_or_its_sessions_are_used_then_only_the_new_code_establishes_access()
    {
        using var admin = await factory.AdministratorBrowser();
        var item = await CreatePublished(admin);
        var (client, registrationId, oldCode) = await factory.ParticipantBrowser(admin, item.Id, "Alex", "alex@example.com");
        using var _ = client;
        (await client.GetAsync($"/api/events/{item.Id}/session")).EnsureSuccessStatusCode();

        var entries = (await admin.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!;
        var version = entries.Single(x => x.Id == registrationId).Version;
        var replaced = await ReplaceCode(admin, item.Id, registrationId, version, clearEmailBinding: false);
        replaced.EnsureSuccessStatusCode();
        var issuance = (await replaced.Content.ReadFromJsonAsync<RosterIssuance>())!;
        Assert.NotNull(issuance.Code);
        Assert.NotEqual(oldCode, issuance.Code);
        Assert.Equal(registrationId, issuance.Entry.Id);

        using var oldSession = await client.GetAsync($"/api/events/{item.Id}/session");
        Assert.Equal(HttpStatusCode.Unauthorized, oldSession.StatusCode);

        using var oldCodeAttempt = factory.Browser();
        var oldToken = await oldCodeAttempt.GetFromJsonAsync<JsonElement>($"/api/events/{item.Id}/antiforgery");
        oldCodeAttempt.DefaultRequestHeaders.Add("X-CSRF-TOKEN", oldToken.GetProperty("requestToken").GetString());
        using var oldCodeResponse = await oldCodeAttempt.PostAsJsonAsync($"/api/events/{item.Id}/session", new { email = "alex@example.com", entryCode = oldCode });
        Assert.Equal(HttpStatusCode.Unauthorized, oldCodeResponse.StatusCode);

        using var newCodeAttempt = factory.Browser();
        var newToken = await newCodeAttempt.GetFromJsonAsync<JsonElement>($"/api/events/{item.Id}/antiforgery");
        newCodeAttempt.DefaultRequestHeaders.Add("X-CSRF-TOKEN", newToken.GetProperty("requestToken").GetString());
        using var newCodeResponse = await newCodeAttempt.PostAsJsonAsync($"/api/events/{item.Id}/session", new { email = "alex@example.com", entryCode = issuance.Code });
        newCodeResponse.EnsureSuccessStatusCode();

        var roster = (await admin.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!;
        Assert.Single(roster);
        Assert.DoesNotContain(issuance.Code, JsonSerializer.Serialize(roster));
    }

    [Fact, Trait("Requirement", "L2-002/AC5")]
    public async Task Given_deactivation_followed_by_reactivation_when_the_participant_authenticates_afresh_then_history_is_restored_but_the_revoked_session_still_fails()
    {
        using var admin = await factory.AdministratorBrowser();
        var item = await CreatePublished(admin);
        var (oldSession, registrationId, code) = await factory.ParticipantBrowser(admin, item.Id, "Alex", "alex@example.com");
        using var _ = oldSession;
        (await oldSession.GetAsync($"/api/events/{item.Id}/session")).EnsureSuccessStatusCode();

        var afterAdd = (await admin.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!.Single(x => x.Id == registrationId);
        (await Deactivate(admin, item.Id, registrationId, afterAdd.Version)).EnsureSuccessStatusCode();

        var afterDeactivate = (await admin.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!.Single(x => x.Id == registrationId);
        var reactivated = await Reactivate(admin, item.Id, registrationId, afterDeactivate.Version);
        reactivated.EnsureSuccessStatusCode();
        var reactivatedEntry = (await reactivated.Content.ReadFromJsonAsync<RosterEntry>())!;
        Assert.True(reactivatedEntry.Active);
        Assert.True(reactivatedEntry.EmailBound);
        Assert.Equal(afterAdd.FirstAccessAtUtc, reactivatedEntry.FirstAccessAtUtc);

        using var oldSessionRead = await oldSession.GetAsync($"/api/events/{item.Id}/session");
        Assert.Equal(HttpStatusCode.Unauthorized, oldSessionRead.StatusCode);

        using var freshAttempt = factory.Browser();
        var token = await freshAttempt.GetFromJsonAsync<JsonElement>($"/api/events/{item.Id}/antiforgery");
        freshAttempt.DefaultRequestHeaders.Add("X-CSRF-TOKEN", token.GetProperty("requestToken").GetString());
        using var freshResponse = await freshAttempt.PostAsJsonAsync($"/api/events/{item.Id}/session", new { email = "alex@example.com", entryCode = code });
        freshResponse.EnsureSuccessStatusCode();
        var freshResult = await freshResponse.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal(registrationId, freshResult.GetProperty("participantId").GetGuid());
    }

    [Fact, Trait("Requirement", "L2-002/AC6")]
    public async Task Given_an_inactive_entrys_email_now_bound_elsewhere_when_reactivating_then_it_is_rejected_until_the_binding_is_cleared_by_code_replacement()
    {
        using var admin = await factory.AdministratorBrowser();
        var item = await CreatePublished(admin);
        var (alexSession, alexId, _) = await factory.ParticipantBrowser(admin, item.Id, "Alex", "shared@example.com");
        using (alexSession) { }

        var afterAdd = (await admin.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!.Single(x => x.Id == alexId);
        (await Deactivate(admin, item.Id, alexId, afterAdd.Version)).EnsureSuccessStatusCode();

        var (rileySession, rileyId, _) = await factory.ParticipantBrowser(admin, item.Id, "Riley", "shared@example.com");
        using (rileySession) { }
        Assert.NotEqual(alexId, rileyId);

        var afterDeactivate = (await admin.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!.Single(x => x.Id == alexId);
        using var blockedReactivation = await Reactivate(admin, item.Id, alexId, afterDeactivate.Version);
        Assert.Equal(HttpStatusCode.UnprocessableEntity, blockedReactivation.StatusCode);
        var problem = await blockedReactivation.Content.ReadFromJsonAsync<JsonElement>();
        Assert.True(problem.GetProperty("errors").TryGetProperty("email", out _));

        var stillInactive = (await admin.GetFromJsonAsync<RosterEntry[]>($"/api/admin/events/{item.Id}/roster"))!.Single(x => x.Id == alexId);
        Assert.False(stillInactive.Active);

        var replaced = await ReplaceCode(admin, item.Id, alexId, stillInactive.Version, clearEmailBinding: true);
        replaced.EnsureSuccessStatusCode();
        var clearedEntry = (await replaced.Content.ReadFromJsonAsync<RosterIssuance>())!.Entry;
        Assert.False(clearedEntry.EmailBound);

        var reactivated = await Reactivate(admin, item.Id, alexId, clearedEntry.Version);
        reactivated.EnsureSuccessStatusCode();
        var reactivatedEntry = (await reactivated.Content.ReadFromJsonAsync<RosterEntry>())!;
        Assert.True(reactivatedEntry.Active);
        Assert.False(reactivatedEntry.EmailBound);
    }

    private async Task<EventSummary> CreatePublished(HttpClient admin)
    {
        var item = await Create(admin);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
        (await db.Events.SingleAsync(x => x.Id == item.Id)).Published = true;
        await db.SaveChangesAsync();
        return item;
    }

    private static async Task<EventSummary> Create(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.PostAsJsonAsync("/api/admin/events", new { title = "Roster lifecycle" }); response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key"); return (await response.Content.ReadFromJsonAsync<EventSummary>())!;
    }

    private static async Task<HttpResponseMessage> Deactivate(HttpClient client, Guid eventId, Guid registrationId, string version)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{eventId}/roster/{registrationId}/deactivate");
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        request.Headers.Add("If-Match", $"\"{version}\"");
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> ReplaceCode(HttpClient client, Guid eventId, Guid registrationId, string version, bool clearEmailBinding)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{eventId}/roster/{registrationId}/code") { Content = JsonContent.Create(new { clearEmailBinding }) };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        request.Headers.Add("If-Match", $"\"{version}\"");
        return await client.SendAsync(request);
    }

    private static async Task<HttpResponseMessage> Reactivate(HttpClient client, Guid eventId, Guid registrationId, string version)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{eventId}/roster/{registrationId}/reactivate");
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        request.Headers.Add("If-Match", $"\"{version}\"");
        return await client.SendAsync(request);
    }

    private static async Task<RosterIssuance> Add(HttpClient client, Guid eventId, string name)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/admin/events/{eventId}/roster") { Content = JsonContent.Create(new { displayName = name }) };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.SendAsync(request); response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<RosterIssuance>())!;
    }

    private static async Task<HttpResponseMessage> Rename(HttpClient client, Guid eventId, Guid registrationId, string name, string? version)
    {
        using var request = new HttpRequestMessage(HttpMethod.Put, $"/api/admin/events/{eventId}/roster/{registrationId}/name") { Content = JsonContent.Create(new { displayName = name }) };
        request.Headers.Add("Idempotency-Key", Guid.NewGuid().ToString());
        if (version is not null) request.Headers.Add("If-Match", $"\"{version}\"");
        return await client.SendAsync(request);
    }
}
