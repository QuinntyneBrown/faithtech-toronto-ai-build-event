using System.Net;
using System.Net.Http.Json;
using FaithTechTorontoAiBuildEvent.Application.Events;
using FaithTechTorontoAiBuildEvent.Application.Roster;

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

    private static async Task<EventSummary> Create(HttpClient client)
    {
        client.DefaultRequestHeaders.Add("Idempotency-Key", Guid.NewGuid().ToString());
        using var response = await client.PostAsJsonAsync("/api/admin/events", new { title = "Roster lifecycle" }); response.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Remove("Idempotency-Key"); return (await response.Content.ReadFromJsonAsync<EventSummary>())!;
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
